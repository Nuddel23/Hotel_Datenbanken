using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaktionslogik für Gäste.xaml
    /// </summary>
    public partial class Gäste : Page
    {
        MySqlConnection DB;
        Frame frame;
        DataTable GastTablle = new DataTable();
        DataView DataView;
        Gast_hinzufügen gast_hinzufügen;


        public Gäste(MySqlConnection DB, Frame frame)
        {
            InitializeComponent();
            this.DB = DB;
            this.frame = frame;
            zeige_Gäste();

            DataView = new DataView(GastTablle);

            tabelle.ItemsSource = DataView;
        }

        void zeige_Gäste()
        {

            using (var command = new MySqlCommand($"SELECT `gast`.`Vorname`, `gast`.`Nachname`, `gast`.`Email`, `gast`.`Telefonnummer`, `adresse`.`Straße`, `adresse`.`Hausnummer`, `adresse`.`PLZ`, `plz`.`Ort`\r\nFROM `gast`\r\n\tLEFT JOIN `gast_hat_adresse` ON `gast`.`Gast_ID` = `gast_hat_adresse`.`Gast_ID` \r\n\tLEFT JOIN `adresse` ON `gast_hat_adresse`.`Adress_ID` = `adresse`.`Adress_ID` \r\n\tLEFT JOIN `plz` ON `adresse`.`PLZ` = `plz`.`PLZ`;", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(GastTablle);
                }
            }
        }

        Window Gast_hinzufügen_Window;
        private void Gast_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            gast_hinzufügen = new Gast_hinzufügen(DB);
            Gast_hinzufügen_Window = new Window();
            Gast_hinzufügen_Window.Content = gast_hinzufügen;
            Gast_hinzufügen_Window.Width = 800;
            Gast_hinzufügen_Window.Height = 500;
            Gast_hinzufügen_Window.Show();

            /*Gast_Frame.Content = new Gast_hinzufügen();
            Gast_Frame.Visibility = Visibility.Visible;
            Gast_Seite.Visibility = Visibility.Hidden;*/
        }

        private void Gast_bearbeiten_Click(object sender, RoutedEventArgs e)
        {
            Gast_Frame.Content = new Gast_bearbeiten();
            Gast_Frame.Visibility = Visibility.Visible;
            Gast_Seite.Visibility = Visibility.Hidden;
        }

        private void Gast_Main(object sender, RoutedEventArgs e)
        {
            Gast_Frame.Visibility = Visibility.Hidden;
            Gast_Seite.Visibility = Visibility.Visible;
        }

        private void Name_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox_sender = (TextBox)sender;
            if (textbox_sender.Text.Length.Equals(0))
            {
                textbox_sender.Text = textbox_sender.Name;
            }
        }

        private void Name_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox_sender = (TextBox)sender;
            if (textbox_sender.Text.Equals(textbox_sender.Name))
            {
                textbox_sender.Clear();
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
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
