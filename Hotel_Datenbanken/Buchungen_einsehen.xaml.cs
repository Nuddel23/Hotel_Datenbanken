﻿using MySqlConnector;
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
        int Rechnung_ID;
        DataTable GastTabelle_table = new DataTable();
        DataTable BuchungTabelle_table = new DataTable();
        DataView DataView_gast;
        DataView DataView_buchung;
        Window Rechnung_window;
        Rechnung rechnung;

        string[] Buchungfiltertypen = { "Zimmertyp", "Check_out", "Check_in", "Balkon", "Terrasse", "Aussicht_Strasse", "Zimmernummer" };
        string[] Buchungfilter = new string[7];
        public Buchungen_einsehen(MySqlConnection DB, int Rechnung_ID)
        {
            InitializeComponent();
            this.DB = DB;
            this.Rechnung_ID = Rechnung_ID;
            tabellenfüllen(null, null);
        }

        void tabellenfüllen(int? buchung_ID, int? gast_ID)
        {
            MySqlCommand gast_command = new MySqlCommand($"SELECT `gast`.`Vorname`, `gast`.`Nachname`, `gast`.`Email`, `gast`.`Telefonnummer`, `gast`.`Gast_ID`\r\nFROM `gast`;", DB);
            MySqlCommand buchung_command = new MySqlCommand($"SELECT `zimmer`.`Zimmernummer`, `buchung`.`Check_in`, `buchung`.`Check_out`, `zimmer`.`Zimmertyp`, `zimmer`.`Etage`, `zimmer`.`Balkon`, `zimmer`.`Terrasse`, `zimmer`.`Aussicht_Strasse`, `buchung`.`Buchungs_ID`\r\nFROM `zimmer` \r\n\tINNER JOIN `buchung` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID`;", DB);

            if (gast_ID != null)
            {
                buchung_command = new MySqlCommand($"SELECT `zimmer`.`Zimmernummer`, `buchung`.`Check_in`, `buchung`.`Check_out`, `zimmer`.`Zimmertyp`, `zimmer`.`Etage`, `zimmer`.`Balkon`, `zimmer`.`Terrasse`, `zimmer`.`Aussicht_Strasse`, `buchung_hat_gast`.* \r\nFROM `zimmer` \r\n\tINNER JOIN `buchung` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID` \r\n\tLEFT JOIN `buchung_hat_gast` ON `buchung_hat_gast`.`Buchungs_ID` = `buchung`.`Buchungs_ID`\r\nWHERE `buchung_hat_gast`.`Gast_ID` = '{gast_ID}';", DB);
            }
            if (buchung_ID != null)
            {
                gast_command = new MySqlCommand($"SELECT `gast`.`Vorname`, `gast`.`Nachname`, `gast`.`Email`, `gast`.`Telefonnummer`, `buchung_hat_gast`.* \r\nFROM `gast` \r\n\tLEFT JOIN `buchung_hat_gast` ON `buchung_hat_gast`.`Gast_ID` = `gast`.`Gast_ID`\r\nWHERE `buchung_hat_gast`.`Buchungs_ID` = '{buchung_ID}';", DB);
            }


            using (var adapter = new MySqlDataAdapter(gast_command))
            {
                GastTabelle_table = new DataTable();
                adapter.Fill(GastTabelle_table);
                DataView_gast = new DataView(GastTabelle_table);
                GastTabelle.ItemsSource = DataView_gast;
            }


            using (var adapter = new MySqlDataAdapter(buchung_command))
            {
                BuchungTabelle_table = new DataTable();
                adapter.Fill(BuchungTabelle_table);
                DataView_buchung = new DataView(BuchungTabelle_table);
                BuchungTabelle.ItemsSource = DataView_buchung;
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
            if (BuchungTabelle.SelectedItem is DataRowView selectedRow)
            {
                tabellenfüllen((int)selectedRow.Row[selectedRow.Row.ItemArray.Length - 1], null);
                if (BuchungTabelle.IsFocused == false)
                {
                    BuchungTabelle.Focus();
                }
            }
        }

        private void GastTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GastTabelle.SelectedItem is DataRowView selectedRow)
            {
                tabellenfüllen(null, (int)selectedRow.Row[selectedRow.Row.ItemArray.Length -1]);
                if (GastTabelle.IsFocused == false)
                {
                    GastTabelle.Focus();
                }
            }
        }

        private void Tabelle_LostFocus(object sender, RoutedEventArgs e)
        {
            tabellenfüllen(null, null);
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
    }
}
