using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Windows.Controls;

namespace Hotel_Datenbanken
{
    /// <summary>
    /// Interaktionslogik für Zimmer_Buchen.xaml
    /// </summary>
    public partial class Zimmer_Buchen : Page
    {
        MySqlConnection DB;
        Frame frame;
        BuchungsStructure.NewBuchung buchung = new BuchungsStructure.NewBuchung();
        Dictionary<string, int> additionals = new Dictionary<string, int>();
        string roomType;

        public Zimmer_Buchen(MySqlConnection DB, Frame frame)
        {
            this.DB = DB;
            this.frame = frame;
            InitializeComponent();
            RB_TypeDefault.IsChecked = true;
            Debug.Print(Calculate.RechnungPrice(3, DB).ToString());
        }

        public void SearchRoom(string prompt)
        {
            DataTable availabelRooms = new DataTable();

            using (var command = new MySqlCommand(prompt, DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    if (availabelRooms != null)
                        adapter.Fill(availabelRooms);
                }
                
            }
            Tabel_Rooms.ItemsSource = availabelRooms!.DefaultView;
           
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
                CheckBox CBAdditional = new CheckBox();
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

            List<int> selectdedAdditional = new List<int>();

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
                "SELECT Z.Zimmer_ID, Zimmertyp, Terrasse, Etage, Balkon, Aussicht_Strasse " +
                "FROM zimmer Z " +
                "LEFT JOIN buchung B " +
                "ON B.Zimmer_ID = Z.Zimmer_ID " +
                "WHERE 1=1";


            RadioButton rbType = Stack_Type.Children.OfType<RadioButton>().Where(rb => rb.IsChecked == true).First();
            roomType = rbType.Content.ToString()!;
            switch (roomType)
            {
                case "Standart":
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
                switch (rbBalcony.Content)
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



        }

        private void BtnConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            ConfirmationScreen.Visibility = System.Windows.Visibility.Visible;

        }

        private void ValidateSelection()
        {
            bool DateIsChecked = DP_Start.SelectedDate != null && DP_End.SelectedDate != null;
            bool RoomIsChecked = Tabel_Rooms.SelectedIndex != -1;

            if (DateIsChecked && RoomIsChecked)
            {
                buchung.CheckIn = DateOnly.FromDateTime(DP_Start.SelectedDate!.Value);
                buchung.CheckOut = DateOnly.FromDateTime(DP_End.SelectedDate!.Value);
                buchung.RoomNr = (int)((DataRowView)Tabel_Rooms.SelectedItem)[0];
                Btn_Confirm.IsEnabled = true;
            }
            else
            {
                Btn_Confirm.IsEnabled = false;
            }
        }

        private void Btn_SecConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            Debug.Print(Calculate.BuchungPrice(buchung, DB).ToString());

        }

        private void Btn_Return_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            ConfirmationScreen.Visibility = System.Windows.Visibility.Hidden;

        }

        private void Tabel_Rooms_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            
            ValidateSelection();
        }

        private void GetFullPrice()
        {
            int anzDays = buchung.CheckOut.DayNumber - buchung.CheckIn.DayNumber;

            string query = $"SELECT Preis From preis WHERE Kategorie = {roomType}";

        }
    }
}
