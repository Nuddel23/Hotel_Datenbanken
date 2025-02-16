using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Hotel_Datenbanken.Structure;


namespace Hotel_Datenbanken
{
    /// <summary>
    /// Interaktionslogik für Buchungen_einsehen.xaml
    /// </summary>
    public partial class Buchungen_einsehen : Page
    {
        MySqlConnection DB;
        int Rechnungs_ID;
        int? BZ_Id;
        int? buchungsId;
        DataTable GastTabelle_table = new DataTable();
        DataTable BuchungTabelle_table = new DataTable();
        readonly Dictionary<string, int> additionals = [];
        DateTime buchungCheckIn;
        DateTime buchungCheckOut;
        DataView DataView_gast;
        DataView DataView_buchung;
        Window Rechnung_window;
        Rechnung rechnung;

        string[] Buchungfiltertypen = { "Zimmertyp", "Check_out", "Check_in", "Balkon", "Terrasse", "Aussicht_Strasse", "Zimmernummer" };
        string[] Buchungfilter = new string[7];
        public Buchungen_einsehen(MySqlConnection DB, int Rechnungs_ID)
        {
            this.DB = DB;
            this.Rechnungs_ID = Rechnungs_ID;
            InitializeComponent();
            CB_Placeholder.IsSelected = true;
            tabellenfüllen();
        }

        void tabellenfüllen()
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                cmd.CommandText = "SELECT  b.Buchungs_ID, z.Zimmernummer, DATE_FORMAT(b.Check_In, '%d.%m.%Y') AS Start, DATE_FORMAT(b.Check_Out, '%d.%m.%Y') AS Ende, z.Zimmertyp, z.Etage, z.Balkon, z.Terrasse, z.Aussicht_Strasse " +
                "FROM zimmer z " +
                "INNER JOIN buchung b ON b.Zimmer_ID = z.Zimmer_ID " +
                $"WHERE b.Rechnungs_ID = {Rechnungs_ID}";

                Debug.Print(cmd.CommandText);
                using (MySqlDataAdapter adapter = new())
                {
                    BuchungTabelle_table = new DataTable();
                    adapter.SelectCommand = cmd;

                    adapter.Fill(BuchungTabelle_table);
                    DataView_buchung = new DataView(BuchungTabelle_table);
                    DG_Buchungen.ItemsSource = DataView_buchung;
                    DG_format(DG_Buchungen);
                }
            }
        }

        private void Filter_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox_sender = (TextBox)sender;
            if (textbox_sender.Text.Length.Equals(0))
            {
                textbox_sender.Text = textbox_sender.Name;
            }
        }

        private void Filter_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox_sender = (TextBox)sender;
            if (textbox_sender.Text.Equals(textbox_sender.Name))
            {
                textbox_sender.Clear();
            }
        }

        private void Filter_Gast_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox filtertextbox && DataView_gast != null)
            {
                string filtertext = filtertextbox.Text;

                if (string.IsNullOrWhiteSpace(filtertext) || filtertext == filtertextbox.Name)
                {
                    DataView_gast.RowFilter = string.Empty;
                }
                else
                {
                    DataView_gast.RowFilter = $"{filtertextbox.Name} LIKE '%{filtertext}%'";
                }

            }
        }


        private void Filter_Buchung_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox filtertextbox && DataView_buchung != null && filtertextbox.Text != null)
            {
                for (int i = 0; i < Buchungfiltertypen.Length; i++)
                {
                    if (filtertextbox.Name == Buchungfiltertypen[i])
                    {
                        Buchungfilter[i] = filtertextbox.Text;
                    }
                }

                Buchung_Filtern();
            }
        }


        private void Buchung_Filtern()
        {
            if (DataView_buchung != null)
            {
                List<string> filterConditions = new List<string>();
                for (int i = 0; i < Buchungfiltertypen.Length; i++)
                {

                    if ((string.IsNullOrWhiteSpace(Buchungfilter[i]) || Buchungfilter[i] == Buchungfiltertypen[i]) == false)
                    {
                        filterConditions.Add($"Convert([{Buchungfiltertypen[i]}], System.String) LIKE '%{Buchungfilter[i]}%'");
                    }
                }
                if (filterConditions.Count > 0)
                {
                    DataView_buchung.RowFilter = string.Join(" AND ", filterConditions);
                }
                else
                {
                    DataView_buchung.RowFilter = string.Empty;
                }
            }
        }

        private void DateChanged(object sender, SelectionChangedEventArgs e)
        {
            DP_End.DisplayDateStart = DP_Start.SelectedDate;
            DP_End.IsEnabled = true;

            if (DP_End.SelectedDate != null)
            {
                Buchungfilter[1] = DP_End.SelectedDate.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                Buchungfilter[1] = "";
            }

            if (DP_Start.SelectedDate != null)
            {
                Buchungfilter[2] = DP_Start.SelectedDate.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                Buchungfilter[2] = "";
            }
            Buchung_Filtern();
        }

        private void CB_Balcony_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (RadioButton rb in Stack_Properties.Children.OfType<RadioButton>())
            {
                rb.IsChecked = false;
                rb.IsEnabled = (bool)CB_Balcony.IsChecked!;
            }
            if (CB_Balcony.IsChecked == false)
            {
                Buchungfilter[3] = "";
                Buchungfilter[4] = "";
                Buchung_Filtern();

            }
        }

        private void ZimmerType_Change(object sender, RoutedEventArgs e)
        {
            RadioButton rbType = Stack_Type.Children.OfType<RadioButton>().Where(rb => rb.IsChecked == true).First();
            switch (rbType.Content.ToString())
            {
                case "Standart":
                    Buchungfilter[0] = "Einzelzimmer";

                    break;

                case "Doppel":
                    Buchungfilter[0] = "Doppelzimmer";

                    break;

                case "Suite":
                    Buchungfilter[0] = "Suite";

                    break;

                case "Alle":
                    Buchungfilter[0] = "";

                    break;

                default:
                    Buchungfilter[0] = "";

                    break;
            }
            Buchung_Filtern();
        }

        private void ZimmerPropety_Changed(object sender, RoutedEventArgs e)
        {
            RadioButton rbBalcony = Stack_Properties.Children.OfType<RadioButton>().Where(rb => rb.IsChecked == true).First();
            switch (rbBalcony.Content)
            {
                case "Balkon: klein":
                    Buchungfilter[3] = "Kleiner Balkon";
                    Buchungfilter[4] = "";
                    break;

                case "Balkon: groß":
                    Buchungfilter[3] = "Großer Balkon";
                    Buchungfilter[4] = "";
                    break;

                case "Terrasse":
                    Buchungfilter[4] = "Ja";
                    Buchungfilter[3] = "";
                    break;
                default:
                    Buchungfilter[3] = "";
                    Buchungfilter[4] = "";
                    break;
            }
            Buchung_Filtern();
        }

        private void CB_Location_Click(object sender, RoutedEventArgs e)
        {
            if (CB_Location.IsChecked == true)
            {
                Buchungfilter[5] = "Nein";
            }
            else
            {
                Buchungfilter[5] = "";
            }
            Buchung_Filtern();
        }

        private void BuchungTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Buchungen.SelectedItem is DataRowView selectedRow)
            {
                buchungsId = (int)selectedRow.Row[0];
                buchungCheckIn = DateTime.Parse(selectedRow.Row[2].ToString()!);
                buchungCheckOut = DateTime.Parse(selectedRow.Row[3].ToString()!);
                GetZusatz();
                GB_New.IsEnabled = true;
            }
        }

        private void GetZusatz()
        {

            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                cmd.CommandText = "SELECT b.BZ_ID, z.Zusatzleistung AS Leistung, Date_Format(b.Start_Datum, '%d.%m.%Y') AS Von, Date_Format(b.End_Datum, '%d.%m.%Y') AS Bis " +
                    "FROM beinhaltet b " +
                    "INNER JOIN zusatzleistung z ON b.Zusatzleistungs_ID = z.Zusatzleistungs_ID " +
                    $"WHERE b.Buchungs_ID = {buchungsId}";


                using (MySqlDataAdapter adapter = new())
                {
                    DataTable dt = new DataTable();
                    adapter.SelectCommand = cmd;

                    adapter.Fill(dt);
                    DG_Additionals.ItemsSource = dt.DefaultView;
                    DG_format(DG_Additionals);
                }

                DP_AddStart.DisplayDateStart = buchungCheckIn;
                DP_AddStart.DisplayDateEnd = buchungCheckOut;
                DP_AddEnd.DisplayDateStart = DateTime.Now;
                DP_AddEnd.DisplayDateEnd = buchungCheckOut;

                DP_NewAddStart.DisplayDateStart = buchungCheckIn;
                DP_NewAddStart.DisplayDateEnd = buchungCheckOut;
                DP_NewAddEnd.DisplayDateStart = buchungCheckIn;
                DP_NewAddEnd.DisplayDateEnd = buchungCheckOut;
            }
        }

        private void Tabelle_LostFocus(object sender, RoutedEventArgs e)
        {
            tabellenfüllen();
        }

        private void Rechnungen_anzeigen_Click(object sender, RoutedEventArgs e)
        {
            rechnung = new Rechnung(DB, 1);
            Rechnung_window = new Window();
            Rechnung_window.Content = rechnung;
            Rechnung_window.Width = 820;
            Rechnung_window.Height = 500;
            Rechnung_window.Show();
        }

        private void Stack_Additional_Initialized(object sender, EventArgs e)
        {
            StackPanel stackAdditional = (StackPanel)sender;

            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                cmd.CommandText = "SELECT Zusatzleistungs_ID, Zusatzleistung FROM zusatzleistung";

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        CheckBox CBAdditional = new();
                        CBAdditional.Checked += CB_Additional_Checked;
                        CBAdditional.Name = "CB_" + reader.GetInt32(0);
                        CBAdditional.Content = reader.GetString(1);
                        CBAdditional.Height = 30;
                        stackAdditional.Children.Add(CBAdditional);

                        if (!additionals.ContainsKey(reader.GetString(1)))
                        {
                            additionals.Add(reader.GetString(1), reader.GetInt32(0));
                        }
                    }
                }

            }
        }

        private void DG_format(DataGrid toFormat, int anzColoumns = 1)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (toFormat.Columns.Count > 0)
                {
                    for (int i = 0; i < anzColoumns; i++)
                    {
                        toFormat.Columns[i].Visibility = Visibility.Collapsed;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void CB_Additional_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            //CheckBox[] selectedCBAdditionals = Stack_Additional.Children.OfType<CheckBox>()
            //    .Where(cb => cb.IsChecked == true)
            //    .ToArray();

            //List<int> selectdedAdditional = [];

            //foreach (CheckBox cb in selectedCBAdditionals)
            //{
            //    if (additionals.TryGetValue(cb.Content.ToString()!, out int id))
            //    {
            //        selectdedAdditional.Add(id);
            //    }
            //}
            //buchung.Additionals = selectdedAdditional;
        }

        private void DG_Additionals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(DG_Additionals.SelectedItem is DataRowView selectedRow)
            {
                BZ_Id = (int)selectedRow.Row[0];
                DP_AddStart.SelectedDate = DateTime.Parse(selectedRow.Row[2].ToString()!);
                if (buchungCheckIn >= DateTime.Now)
                { 
                    DP_AddStart.DisplayDateStart = buchungCheckIn;
                    DP_AddEnd.DisplayDateStart = buchungCheckIn;
                }
                else
                {
                    DP_AddStart.DisplayDateStart = DateTime.Now;
                }

                DP_AddEnd.SelectedDate = DateTime.Parse(selectedRow.Row[3].ToString()!);

                Btn_Save.IsEnabled = false;
            }
        }

        private void DP_AddStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DP_AddEnd.DisplayDateStart = DP_AddStart.SelectedDate;
            if (DP_AddEnd.SelectedDate == null || DP_AddEnd.SelectedDate < DP_AddStart.SelectedDate)
            {
                DP_AddEnd.SelectedDate = DP_AddStart.SelectedDate;
                DP_AddEnd.IsEnabled = true;
            }
            if (DP_AddStart.SelectedDate == null)
            {
                DP_AddEnd.SelectedDate = null;
                DP_AddEnd.IsEnabled = false;
            }
            Validate();
        }

        private void Validate()
        {
            if (DG_Additionals.SelectedItem is DataRowView selectedRow)
            {
                bool startChanged = DateTime.Parse(selectedRow.Row[2].ToString()!) != DP_AddStart.SelectedDate;
                bool endChanged = DateTime.Parse(selectedRow.Row[3].ToString()!) != DP_AddEnd.SelectedDate;

                if((startChanged || endChanged) && BZ_Id != null)
                {
                    Btn_Save.IsEnabled = true;
                }
                else
                {
                    Btn_Save.IsEnabled = false;
                }
            }
        }
        private void DP_AddEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Validate();
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            MySqlTransaction transaction = DB.BeginTransaction();

            using (MySqlCommand cmd = new())
            {
                try
                {
                    cmd.Connection = DB;
                    cmd.Transaction = transaction;

                    cmd.CommandText = 
                        $"UPDATE beinhaltet b SET b.Start_Datum = '{DP_AddStart.SelectedDate:yyyy-MM-dd}', End_Datum = '{DP_AddEnd.SelectedDate:yyyy-MM-dd}' " +
                        $"WHERE b.BZ_ID = {BZ_Id}";
                    Debug.Print(cmd.CommandText);
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    GetZusatz();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Etwas ist Schiefgelaufen:\n\r\r" + ex.Message);
                }
            }
        }

        private void CB_Zusatzleistung_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateNew();
        }
        private void DP_NewSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender == DP_NewAddStart)
            {
                DP_NewAddEnd.DisplayDateStart = DP_NewAddStart.SelectedDate;
                if (DP_NewAddEnd.SelectedDate == null || DP_NewAddEnd.SelectedDate < DP_NewAddStart.SelectedDate)
                {
                    DP_NewAddEnd.SelectedDate = DP_NewAddStart.SelectedDate;
                    DP_NewAddEnd.IsEnabled = true;
                }
                if (DP_NewAddStart.SelectedDate == null)
                {
                    DP_NewAddEnd.SelectedDate = null;
                    DP_NewAddEnd.IsEnabled = false;
                }
            }
            ValidateNew();
        }

        private void ValidateNew()
        {
            bool dateIsChecked = DP_NewAddStart.SelectedDate != null && DP_NewAddEnd.SelectedDate != null;
            bool additionalIsSelected = (CB_Zusatzleistung.SelectedItem as ComboBoxItem)?.Content.ToString()! != "Zusatzleistung...";

            if (dateIsChecked && additionalIsSelected)
            {
                Btn_New.IsEnabled = true;
            }
            else
            {
                Btn_New.IsEnabled = false;
            }
        }

        private void Btn_New_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = DP_NewAddStart.SelectedDate;
            DateTime? endDate = DP_NewAddEnd.SelectedDate;
            string zusatzleistung = (CB_Zusatzleistung.SelectedItem as ComboBoxItem)?.Content.ToString()!;
            int zusatzleistungId = 0;
            if(additionals.TryGetValue(zusatzleistung, out int id))
            {
                zusatzleistungId = id;
            }

            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                
                cmd.CommandText = 
                    "SELECT b.BZ_ID " +
                    "FROM beinhaltet b " +
                    $"WHERE b.Zusatzleistungs_ID = {zusatzleistungId} " +
                    $"AND b.Buchungs_ID = {buchungsId} " +
                    "AND b.BZ_ID " +
                    "AND NOT( " +
                    $"b.End_Datum < '{startDate:yyyy-MM-dd}' " +
                    $"OR b.Start_Datum > '{endDate:yyyy-MM-dd}')";

                Debug.Print(cmd.CommandText);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        MessageBox.Show($"In dem Zeitraum liegt bereits eine buchung von {zusatzleistung} vor.");
                        return;
                    }
                    else
                    {
                        reader.Close();
                        try
                        {
                            cmd.CommandText =
                                    "INSERT INTO beinhaltet " +
                                    $"VALUES (NULL, {buchungsId},{zusatzleistungId},'{startDate:yyyy-MM-dd}','{endDate:yyyy-MM-dd}')";
                            cmd.ExecuteNonQuery();

                            MessageBox.Show($"{zusatzleistung} vom {startDate:dd.MM.yyyy} bis {endDate:dd.MM.yyyy} hinzugefügt.");
                            GetZusatz();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Etwas ist Schiefgelaufen:\n\r\r" + ex.Message);
                        }


                    }
                }

            }


        }
    }
}
