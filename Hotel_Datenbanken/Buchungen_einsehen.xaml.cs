using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Hotel_Datenbanken
{
    /// <summary>
    /// Interaktionslogik für Buchungen_einsehen.xaml
    /// </summary>
    public partial class Buchungen_einsehen : Page
    {
        MySqlConnection DB;
        DataTable GastTabelle = new DataTable();
        DataTable BuchungTabelle = new DataTable();
        DataView DataView_gast;
        DataView DataView_buchung;

        string[] Buchungfiltertypen = { "Zimmertyp", "Check_out", "Check_in", "Balkon", "Terrasse", "Aussicht_Strasse" };
        string[] Buchungfilter = new string[6];
        public Buchungen_einsehen(MySqlConnection DB)
        {
            InitializeComponent();
            this.DB = DB;
            tabellenfüllen();

            DataView_gast = new DataView(GastTabelle);
            DataView_buchung = new DataView(BuchungTabelle);

            tabelle2.ItemsSource = DataView_gast;
            tabelle.ItemsSource = DataView_buchung;
        }

        void tabellenfüllen()
        {
            using (var command = new MySqlCommand($"SELECT * FROM gast;", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(GastTabelle);
                }
            }
            using (var command = new MySqlCommand($"SELECT `buchung`.*, `zimmer`.*\r\nFROM `buchung` \r\n\tLEFT JOIN `zimmer` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID`;", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(BuchungTabelle);
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

        private void Filter_gast_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox filtertextbox && DataView_gast != null)
            {
                string filtertext = filtertextbox.Text;

                // Filter auf die DataView anwenden
                if (string.IsNullOrWhiteSpace(filtertext) || filtertext == filtertextbox.Name)
                {
                    // Wenn das Textfeld leer ist, wird der Filter entfernt
                    DataView_gast.RowFilter = string.Empty;
                }
                else
                {
                    // Filter nach Name anwenden (oder anderen Spalten)
                    DataView_gast.RowFilter = $"{filtertextbox.Name} LIKE '%{filtertext}%'";

                }

            }
        }
        

        private void Filter_adresse_TextChanged(object sender, TextChangedEventArgs e)
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

                    if (string.IsNullOrWhiteSpace(Buchungfilter[i]) || Buchungfilter[i] == Buchungfiltertypen[i])
                    {
                        
                    }
                    else
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

        private void DP_Start_Initialized(object sender, EventArgs e)
        {
            
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

        private void DP_End_Initialized(object sender, EventArgs e)
        {
            DP_End.IsEnabled = false;
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
    }
}
