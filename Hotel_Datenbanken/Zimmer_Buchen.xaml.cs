using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hotel_Datenbanken
{
    /// <summary>
    /// Interaktionslogik für Zimmer_Buchen.xaml
    /// </summary>
    public partial class Zimmer_Buchen : Page
    {
        readonly MySqlConnection DB;
        readonly Frame frame;
        readonly Structure.NewBuchung buchung = new();
        readonly Dictionary<string, int> additionals = [];
        DataTable selectedRoomsTable = new();
        DataView roomView;
        List<int> selectedRoomIds = new List<int> { };
        string? roomType, roomExtra;

        public Zimmer_Buchen(MySqlConnection DB, Frame frame)
        {
            this.DB = DB;
            this.frame = frame;
            InitializeComponent();
            RB_TypeDefault.IsChecked = true;
            string sqlPrompt = "" +
                "SELECT Z.Zimmer_ID, Zimmertyp, Terrasse, Etage, Balkon, Aussicht_Strasse AS \"Straße\"" +
                "FROM zimmer Z " +
                "WHERE 1 = 0";
            SearchRoom(sqlPrompt);
            if (DG_Rooms.ItemsSource is DataView dv)
            {
                DG_SelectedRooms.Columns.Clear();
                foreach (DataColumn columne in dv.Table!.Columns)
                {
                    selectedRoomsTable.Columns.Add(columne.ColumnName, columne.DataType);
                }
                DG_SelectedRooms.ItemsSource = selectedRoomsTable.DefaultView;
            }
            CreatePrompt();
        }

        public void SearchRoom(string prompt)
        {
            DataTable availabelRooms = new();

            using (var command = new MySqlCommand(prompt, DB))
            {
                using var adapter = new MySqlDataAdapter(command);
                if (availabelRooms != null)
                    adapter.Fill(availabelRooms);

            }
            roomView = new DataView(availabelRooms);

            if(selectedRoomIds.Count != 0)
            {
                string filter = string.Join(",", selectedRoomIds);

                roomView.RowFilter = $"[Zimmer_ID] NOT IN ({filter})";
            }


            DG_Rooms.ItemsSource = roomView;
        }

        private void Stack_Additional_Initialized(object sender, EventArgs e)
        {
            Calculate.RechnungPrice(2, DB);
            StackPanel stackAdditional = (StackPanel)sender;
            string query = "SELECT Zusatzleistungs_ID, Zusatzleistung FROM zusatzleistung";

            MySqlCommand cmd = new(query, DB);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CheckBox CBAdditional = new();
                CBAdditional.Checked += CB_Additional_Checked;
                CBAdditional.Name = "CB_" + reader.GetInt32(0);
                CBAdditional.Content = reader.GetString(1); 
                CBAdditional.Height = 30;
                stackAdditional.Children.Add(CBAdditional);

                additionals.Add(reader.GetString(1), reader.GetInt32(0));
                
            }
            reader.Close();
            
        }

        private void CB_Additional_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox[] selectedCBAdditionals = Stack_Additional.Children.OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .ToArray();

            List<int> selectdedAdditional = [];

            foreach (CheckBox cb in selectedCBAdditionals)
            {
                if(additionals.TryGetValue(cb.Content.ToString()!, out int id))
                {
                    selectdedAdditional.Add(id);
                }
            }
            buchung.Additionals = selectdedAdditional;

        }

        private void Prop_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            CreatePrompt();
        }

        private void CreatePrompt()
        {
            string sqlPrompt = "" +
                "SELECT Z.Zimmer_ID, Zimmertyp AS \"Typ\", Terrasse, Etage, Balkon, Aussicht_Strasse AS \"Straße\" " +
                "FROM zimmer Z " +
                "LEFT JOIN buchung B " +
                "ON B.Zimmer_ID = Z.Zimmer_ID " +
                "WHERE 1=1";
            


            RadioButton rbType = Stack_Type.Children.OfType<RadioButton>().Where(rb => rb.IsChecked == true).First();
            roomType = rbType.Content.ToString()!;
            switch (roomType)
            {
                case "Einzel":
                    sqlPrompt += " AND Zimmertyp = \"Einzelzimmer\"";
                    break;

                case "Doppel":
                    sqlPrompt += " AND Zimmertyp = \"Doppelzimmer\"";
                    break;

                case "Suite":
                    sqlPrompt += " AND Zimmertyp = \"Suite\"";
                    break;

                default:
                    break;
            }

            if (CB_Location.IsChecked == true)
            {
                sqlPrompt += " AND Aussicht_Strasse = \"Nein\"";
            }

            if (CB_Balcony.IsChecked == true)
            {
                RadioButton rbBalcony = Stack_Properties.Children.OfType<RadioButton>().Where(rb => rb.IsChecked == true).First();
                roomExtra = rbBalcony.Content.ToString()!;
                switch (roomExtra)
                {
                    case "Balkon: klein":
                        sqlPrompt += " AND Balkon = \"Kleiner Balkon\"";
                        break;

                    case "Balkon: groß":
                        sqlPrompt += " AND Balkon = \"Großer Balkon\"";
                        break;

                    case "Terrasse":
                        sqlPrompt += " AND Terrasse = \"Ja\"";
                        break;

                    default:
                        break;
                }
            }

            if (DP_Start.SelectedDate != null)
            {

                sqlPrompt += " " +
                    "AND Z.Zimmer_ID " +
                    "Not IN( " +
                    "SELECT B.Zimmer_ID " +
                    "FROM buchung B " +
                    "WHERE B.Check_in <= CAST(\"" + DP_End.SelectedDate!.Value.ToString("yyyy-MM-dd") + "\" AS DATE) " +
                    "AND B.Check_out > CAST(\"" + DP_Start.SelectedDate.Value.ToString("yyyy-MM-dd") + "\" AS DATE)) ";
            }

            sqlPrompt += " GROUP BY Z.Zimmer_ID";

            SearchRoom(sqlPrompt);
        }

        private void CB_Balcony_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (RadioButton rb in Stack_Properties.Children.OfType<RadioButton>())
            {
                rb.IsChecked = false;
                rb.IsEnabled = (bool)CB_Balcony.IsChecked!;
            }
        }

        private void DP_Start_Initialized(object sender, EventArgs e)
        {
            DP_Start.DisplayDateStart = DateTime.Now;
        }

        private void DateChanged(object sender, SelectionChangedEventArgs e)
        {
            DP_End.DisplayDateStart = DP_Start.SelectedDate;
            if (DP_End.SelectedDate == null || DP_End.SelectedDate < DP_Start.SelectedDate)
            {
                DP_End.SelectedDate = DP_Start.SelectedDate;
                DP_End.IsEnabled = true;
            }
            if (DP_Start.SelectedDate == null)
            {
                    DP_End.SelectedDate = null;
                    DP_End.IsEnabled = false;
            }

            CreatePrompt();
            ValidateSelection();
        }

        private void DP_End_Initialized(object sender, EventArgs e)
        {
            DP_End.DisplayDateStart = DateTime.Now;
            DP_End.IsEnabled = false;
        }

        private void BtnGuest_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            //Warte auf die funktion von John

        }

        private void BtnConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //--Test--
            buchung.GuestId = 6;
            //--------
            string query = "SELECT g.Vorname, g.Nachname, g.Email, g.Telefonnummer, a.Straße, a.Hausnummer, a.PLZ, p.Ort " +
                "FROM gast g " +
                "INNER JOIN adresse a ON g.Adress_ID = a.Adress_ID " +
                "INNER JOIN PLZ p ON a.PLZ = p.PLZ " +
                $"WHERE g.Gast_ID = {buchung.GuestId}";
            MySqlCommand cmd = new(query, DB);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Lbl_Name.Content = reader.GetString(0);
                Lbl_Lastname.Content = reader.GetString(1);
                Lbl_Email.Content = reader.GetString(2);
                Lbl_Tel.Content = reader.GetString(3);
                Lbl_Address.Content = $"{reader.GetString(4)} {reader.GetString(5)}";
                Lbl_PLZ.Content = $"{reader.GetString(6)} {reader.GetString(7)}";
            }
            reader.Close();

            TB_RoomNrs.Text = string.Join(", ", buchung.RoomNrs);
            Lbl_Start.Content = buchung.CheckIn.ToString("dd.MM.yyyy");
            Lbl_End.Content = buchung.CheckOut.ToString("dd.MM.yyyy");
            TB_PayMethode.Text = buchung.PayMethode;
            
            string additions = "";
            if(buchung.Additionals != null)
            {
                foreach(int id in buchung.Additionals)
                {
                    additions += $"{additionals.FirstOrDefault(x => x.Value == id).Key}, ";
                }
            }
            additions = additions.Remove(additions.Length-2);
            TB_Extra.Text = additions;

            ConfirmationScreen.Visibility = System.Windows.Visibility.Visible;

        }

        private void ValidateSelection()
        {
            bool dateIsChecked = DP_Start.SelectedDate != null && DP_End.SelectedDate != null;
            bool roomIsChecked = selectedRoomIds.Count != 0;
            bool payMethodIsChecked = (CB_PayMethode.SelectedItem as ComboBoxItem)?.Content.ToString()! != "Zahlungsart...";

            if (dateIsChecked && roomIsChecked && payMethodIsChecked)
            {
                buchung.CheckIn = DateOnly.FromDateTime(DP_Start.SelectedDate!.Value);
                buchung.CheckOut = DateOnly.FromDateTime(DP_End.SelectedDate!.Value);
                buchung.RoomNrs = selectedRoomIds;
                buchung.PayMethode = (CB_PayMethode.SelectedItem as ComboBoxItem)?.Content.ToString()!;
                Btn_Confirm.IsEnabled = true;   
            }
            else
            {
                Btn_Confirm.IsEnabled = false;
            }
        }

        private void Btn_SecConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MySqlTransaction transaction = DB.BeginTransaction();

            try
            {
                using MySqlCommand cmd = new();
                cmd.Connection = DB;
                cmd.Transaction = transaction;

                cmd.CommandText = "INSERT INTO rechnung " +
                            $"VALUES (NULL,'-','{buchung.PayMethode}',{buchung.GuestId})";
                cmd.ExecuteNonQuery();

                int rechnungsId = (int)cmd.LastInsertedId;

                foreach (int roomId in buchung.RoomNrs)
                {
                    cmd.CommandText = "INSERT INTO buchung " +
                                $"VALUES (NULL,'{buchung.CheckIn.ToString("yyyy-MM-dd")}','{buchung.CheckOut.ToString("yyyy-MM-dd")}',{roomId},{rechnungsId})";
                    cmd.ExecuteNonQuery();
                    int bookingId = (int)cmd.LastInsertedId;

                    foreach(int additionalId in buchung.Additionals)
                    {
                        cmd.CommandText = "INSERT INTO beinhaltet " +
                                        $"VALUES ({bookingId},{additionalId},'{buchung.CheckIn.ToString("yyyy-MM-dd")}','{buchung.CheckOut.ToString("yyyy-MM-dd")}')";
                        cmd.ExecuteNonQuery();
                    }

                }

                transaction.Commit();
                MessageBox.Show("Haf gefunzt");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(ex.Message);
            }
            
        }

        private void Btn_Return_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            ConfirmationScreen.Visibility = System.Windows.Visibility.Hidden;

        }

        private void Btn_AddRoom_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DG_Rooms.SelectedItem is DataRowView selectedRow)
            {
                DataRow dataRow = selectedRoomsTable.NewRow();
                dataRow.ItemArray = (object[])selectedRow.Row.ItemArray.Clone();

                selectedRoomsTable.Rows.Add(dataRow);
                DG_SelectedRooms.ItemsSource = selectedRoomsTable.DefaultView;

                selectedRoomIds.Add((int)selectedRow.Row[0]!);

                CreatePrompt();
            }

        }

        private void Btn_RemoveRoom_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if(DG_SelectedRooms.SelectedItem is DataRowView selectedRow)
            {
                selectedRoomIds.Remove((int)selectedRow.Row[0]!);
                selectedRow.Row.Delete();
                selectedRoomsTable.AcceptChanges();

                CreatePrompt();
            }

        }

        private void CB_PayMethode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Btn_Confirm != null)
            {
                ValidateSelection();

            }
        }

        private void Tabel_Rooms_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            ValidateSelection();
        }
    }
}
