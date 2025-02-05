using MySqlConnector;
using System;
using System.Collections;
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
        DataRowView row;
        


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

            using (var command = new MySqlCommand($"SELECT `gast`.* , `adresse`.*, `plz`.`Ort`\r\nFROM `gast`\r\n\tLEFT JOIN `adresse` ON `gast`.`Adress_ID` = `adresse`.`Adress_ID` \r\n\tLEFT JOIN `plz` ON `adresse`.`PLZ` = `plz`.`PLZ`;", DB))
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

        private void Gast_bearbeiten_IsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Gast_bearbeiten.IsChecked != null)
            {
                tabelle.IsReadOnly = (bool) !Gast_bearbeiten.IsChecked;
            }
        }

        private void tabelle_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is DataRowView row)
            {
                // Hole die bearbeiteten Daten
                var Gast_ID = row["Gast_ID"]; // Primärschlüssel
                var Vorname = row["Vorname"];
                var Nachname = row["Nachname"];
                var Email = row["Email"];
                var Telefonnummer = row["Telefonnummer"];
                var Adress_ID = row["Adress_ID"];
                var Straße = row["Straße"];
                var Hausnummer = row["Hausnummer"];
                var PLZ = row["PLZ"];
                var Ort = row["Ort"];

                // Erstelle das SQL-UPDATE-Statement
                string sqlUpdate1 = $"UPDATE plz SET Ort = \"{Ort}\" WHERE PLZ = \"{PLZ}\";";
                string sqlUpdate2 = $"UPDATE adresse SET Straße = \"{Straße}\", Hausnummer = \"{Hausnummer}\", PLZ = \"{PLZ}\" WHERE Adress_ID = \"{Adress_ID}\";";
                string sqlUpdate3 = $"UPDATE gast SET Vorname = \"{Vorname}\", Nachname = \"{Nachname}\", Email = \"{Email}\", Telefonnummer = \"{Telefonnummer}\" WHERE Gast_ID = \"{Gast_ID}\";";
                // Rufe eine Methode auf, um das SQL-Statement auszuführen

                MySqlCommand insertCmd = new(sqlUpdate1, DB);
                insertCmd.ExecuteNonQuery();

                insertCmd = new(sqlUpdate2, DB);
                insertCmd.ExecuteNonQuery();

                insertCmd = new(sqlUpdate3, DB);
                insertCmd.ExecuteNonQuery();

            }
        }

        private void Gast_löschen_Click(object sender, RoutedEventArgs e)
        {
            if (tabelle.SelectedIndex == -1)
            {
                MessageBox.Show("nichts wurde ausgewählt");
            }
            else
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Möchtest du den Gast wirklich löschen?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var Gast_ID = row["Gast_ID"]; // Primärschlüssel

                    string sqlDelete = $"DELETE FROM `gast` WHERE `gast`.`Gast_ID` = \"{Gast_ID}\" ";
                    
                    MySqlCommand deleteCmd = new(sqlDelete, DB);
                    
                    deleteCmd.ExecuteNonQuery();
                }
            }
        }

        private void tabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabelle.SelectedIndex != -1)
            {
                row = (DataRowView)tabelle.SelectedItem;
            }
        }
    }
}
