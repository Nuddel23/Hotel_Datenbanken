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
    /// Interaktionslogik für Gast_hinzufügen.xaml
    /// </summary>
    public partial class Gast_hinzufügen : Page
    {
        MySqlConnection DB;
        DataTable GastTabelle = new DataTable();
        DataTable AdressTabelle = new DataTable();
        DataView DataView_gast;
        DataView DataView_adresse;
        int Gast_ID;
        public Gast_hinzufügen(MySqlConnection DB, int Gast_ID)
        {
            InitializeComponent();
            this.DB = DB;
            this.Gast_ID = Gast_ID;
            tabellenfüllen();

            DataView_gast = new DataView(GastTabelle);
            DataView_adresse = new DataView(AdressTabelle);

            tabelle2.ItemsSource = DataView_gast;
            tabelle.ItemsSource = DataView_adresse;
        }

        public int returngast()
        {
            return Gast_ID;
        }

        void tabellenfüllen()
        {
            using (var command = new MySqlCommand($"SELECT `gast`.`Vorname`, `gast`.`Nachname`, `gast`.`Email`, `gast`.`Telefonnummer`\r\nFROM `gast`;", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(GastTabelle);
                }
            }
            using (var command = new MySqlCommand($"SELECT `adresse`.`Straße`, `adresse`.`Hausnummer`, `adresse`.`PLZ`, `plz`.`Ort` FROM `adresse` LEFT JOIN `plz` ON `adresse`.`PLZ` = `plz`.`PLZ`;", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(AdressTabelle);
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
            if (sender is TextBox filtertextbox && DataView_adresse != null)
            {
                string filtertext = filtertextbox.Text;

                // Filter auf die DataView anwenden
                if (string.IsNullOrWhiteSpace(filtertext) || filtertext == filtertextbox.Name)
                {
                    // Wenn das Textfeld leer ist, wird der Filter entfernt
                    DataView_adresse.RowFilter = string.Empty;
                }
                else
                {
                    // Filter nach Name anwenden (oder anderen Spalten)
                    DataView_adresse.RowFilter = $"{filtertextbox.Name} LIKE '%{filtertext}%'";

                }

            }
        }

        private void Hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            //Telefonnummer.Text = tabelle.SelectedIndex.ToString();
            string vorname = Vorname.Text, nachname = Nachname.Text, email = Email.Text, telefonnummer = Telefonnummer.Text;
            string straße = Straße.Text, hausnummer = Hausnummer.Text, ort = Ort.Text, plz = PLZ.Text;

            // Check if PLZ exists
            string query = $"SELECT * FROM plz WHERE PLZ = {plz}";
            MySqlCommand cmd = new(query, DB);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();
                // Insert new PLZ
                query = $"INSERT INTO plz (PLZ, Ort) VALUES (\"{plz}\", \"{ort}\")";
                using MySqlCommand insertCmd = new(query, DB);
                insertCmd.ExecuteNonQuery();
            }
            reader.Close();

            // Check if Address exists
            query = $"SELECT Adress_ID FROM adresse WHERE Straße = \"{straße}\" AND Hausnummer = \"{hausnummer}\" AND PLZ = \"{plz}\"";
            int Adr_ID;
            using (cmd = new(query, DB))
            {
                reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();

                    // Insert new Address
                    query = $"INSERT INTO adresse (Straße, Hausnummer, PLZ) VALUES (\"{straße}\", \"{hausnummer}\", \"{plz}\")";
                    using MySqlCommand insertCmd = new(query, DB);
                    insertCmd.ExecuteNonQuery();

                    query = "SELECT Adress_ID FROM adresse ORDER BY Adress_ID DESC";
                    MySqlCommand scopeCmd = new(query, DB);
                    reader = scopeCmd.ExecuteReader();
                    reader.Read();
                    Adr_ID = reader.GetInt32(0);
                    reader.Close();
                }
                else
                {
                    reader.Read();
                    Adr_ID = reader.GetInt32(0);
                    reader.Close();
                }
                
            }

            query = $"SELECT Gast_ID FROM Gast WHERE Vorname = \"{vorname}\" AND Nachname = \"{nachname}\" AND Email = \"{email}\" AND Telefonnummer = \"{telefonnummer}\"";
            using (cmd = new(query, DB))
            {
                cmd.ExecuteNonQuery();

                reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    query = $"INSERT INTO gast (Vorname, Nachname, Email, Telefonnummer, Adress_ID) VALUES (\"{vorname}\", \"{nachname}\", \"{email}\", \"{telefonnummer}\", \"{Adr_ID}\")";
                    using MySqlCommand insertCmd = new(query, DB);
                    insertCmd.ExecuteNonQuery();

                    query = "SELECT Gast_ID FROM Gast ORDER BY Gast_ID DESC";
              
                    MySqlCommand scopeCmd = new(query, DB);
                    reader = scopeCmd.ExecuteReader();
                    reader.Read();
                    Gast_ID = reader.GetInt32(0);
                    reader.Close();
                }
                else
                {
                    reader.Read();
                    Gast_ID = reader.GetInt32(0);
                    reader.Close();
                }
            }

            MessageBox.Show("Gast wurde erfolgreich hinzugefügt");
            Nachname.Text = Nachname.Name;
            Vorname.Text = Vorname.Name;
            Email.Text = Email.Name;
            Telefonnummer.Text = Telefonnummer.Name;
            Straße.Text = Straße.Name;
            Hausnummer.Text = Hausnummer.Name;
            PLZ.Text = PLZ.Name;
            Ort.Text = Ort.Name;

            tabellenfüllen();
            WertGeändert?.Invoke(this, Gast_ID);
            Window.GetWindow(this)?.Close();
        }

        // Ereignis, das den geänderten Wert zurückgibt
        public event EventHandler<int> WertGeändert;
        private void Abbrechen_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void Bestätigen_Click(object sender, RoutedEventArgs e)
        {
            if (tabelle2.SelectedItem is DataRowView selectedRow)
            {
                WertGeändert?.Invoke(this, (int)selectedRow.Row[0]);
                Window.GetWindow(this)?.Close();
            }
            else
            {
                MessageBox.Show("Es wurde kein Gast ausgewählt");
            }
        }

        private void Adresse_reset_Click(object sender, RoutedEventArgs e)
        {
            Straße.Text = "";
            Hausnummer.Text = "";
            PLZ.Text = "";
            Ort.Text = "";
        }

        private void tabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabelle.SelectedItem is DataRowView selectedRow)
            {
                Straße.Text = (string)selectedRow.Row[0];
                Hausnummer.Text = (string)selectedRow.Row[1];
                PLZ.Text = (string)selectedRow.Row[2];
                Ort.Text = (string)selectedRow.Row[3];
            }
        }
    }

}
