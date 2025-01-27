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
        int guestId;

        public Zimmer_Buchen(MySqlConnection DB, Frame frame)
        {
            InitializeComponent();
            this.DB = DB;
            this.frame = frame;
            RB_TypeDefault.IsChecked = true;
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

        private void CB_Additional_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var selectedAdditionals = Stack_Additional.Children.OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .ToArray();

            string[] Additionals = new string[selectedAdditionals.Length];

            for (int i = 0; i < selectedAdditionals.Length; i++)
            {
                Additionals[i] = selectedAdditionals[i].Content.ToString()!;
            }
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
            switch (rbType.Content)
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

            Debug.Print(sqlPrompt);

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
            }
            DP_End.IsEnabled = true;
            CreatePrompt();
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

        private void Btn_SecConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {



        }

        private void Btn_Return_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            ConfirmationScreen.Visibility = System.Windows.Visibility.Hidden;

        }
    }
}
