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
            if (sender is TextBox filtertextbox && DataView_buchung != null)
            {
                Buchung_Filtern(filtertextbox.Text, filtertextbox.Name);
            }
        }

        private void Buchung_Filtern(string filtertext, string name)
        {
            filtertext = (string) filtertext.ToString();
            
            if (DataView_buchung != null && filtertext != null)
            {
                // Filter auf die DataView anwenden
                if (string.IsNullOrWhiteSpace(filtertext) || filtertext == name)
                {
                    // Wenn das Textfeld leer ist, wird der Filter entfernt
                    DataView_buchung.RowFilter = string.Empty;
                }
                else
                {
                    // Filter nach Name anwenden (oder anderen Spalten)
                    DataView_buchung.RowFilter = $"Convert([{name}], System.String) LIKE '%{filtertext}%'";

                }

            }
        }

        private void DP_Start_Initialized(object sender, EventArgs e)
        {
            
        }

        private void DateChanged(object sender, SelectionChangedEventArgs e)
        {
            DP_End.DisplayDateStart = DP_Start.SelectedDate;
            if (DP_End.SelectedDate == null || DP_End.SelectedDate < DP_Start.SelectedDate)
            {
                DP_End.SelectedDate = DP_Start.SelectedDate;
            }
            DP_End.IsEnabled = true;
            Buchung_Filtern(DP_Start.SelectedDate.Value.ToString("yyyy-MM-dd"), "Check_in");
            Buchung_Filtern(DP_End.SelectedDate.Value.ToString("yyyy-MM-dd"), "Check_out");
        }

        private void DP_End_Initialized(object sender, EventArgs e)
        {
            DP_End.IsEnabled = false;
        }


    }
}
