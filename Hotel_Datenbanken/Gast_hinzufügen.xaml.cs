using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    /// Interaktionslogik für Gast_hinzufügen.xaml
    /// </summary>
    public partial class Gast_hinzufügen : Page
    {
        MySqlConnection DB;
        DataTable AdressTabelle = new DataTable();
        DataView DataView;
        public Gast_hinzufügen(MySqlConnection DB)
        {
            InitializeComponent();
            this.DB = DB;
            zeige_adresse();

            DataView = new DataView(AdressTabelle);

            tabelle.ItemsSource = DataView;
        }

        void zeige_adresse()
        {

            using (var command = new MySqlCommand($"SELECT `adresse`.`Straße`, `adresse`.`Hausnummer`, `adresse`.`PLZ`, `plz`.`Ort` FROM `adresse` LEFT JOIN `plz` ON `adresse`.`PLZ` = `plz`.`PLZ`;", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(AdressTabelle);
                }
            }
        }

        private void Bestätigen_Click(object sender, RoutedEventArgs e)
        {
            Telefonnummer.Text = tabelle.SelectedIndex.ToString();
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

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox filtertextbox && DataView != null)
            {
                string filtertext = filtertextbox.Text;

                // Filter auf die DataView anwenden
                if (string.IsNullOrWhiteSpace(filtertext) || filtertext == filtertextbox.Name)
                {
                    // Wenn das Textfeld leer ist, wird der Filter entfernt
                    DataView.RowFilter = string.Empty;
                }
                else
                {
                    // Filter nach Name anwenden (oder anderen Spalten)
                    DataView.RowFilter = $"{filtertextbox.Name} LIKE '%{filtertext}%'";

                }

            }
        }
    }
    
}
