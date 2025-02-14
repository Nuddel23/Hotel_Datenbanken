using MySqlConnector;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Hotel_Datenbanken
{
    public partial class Rechnung_einsehen : Page
    {
        MySqlConnection DB;
        DataTable GastTabelle_table = new DataTable();
        DataTable BuchungTabelle_table = new DataTable();
        DataView? dataView_gast;
        DataView? dataViewBill;

        string[] Buchungfiltertypen = { "Zimmertyp", "Check_out", "Check_in", "Balkon", "Terrasse", "Aussicht_Strasse", "Zimmernummer" };
        string[] Buchungfilter = new string[7];

        public Rechnung_einsehen(MySqlConnection DB)
        {
            InitializeComponent();
            this.DB = DB;
            tabellenfüllen(null, null);
        }

        void tabellenfüllen(int? buchung_ID, int? gast_ID)
        {
            MySqlCommand gast_command = new MySqlCommand($"SELECT * FROM gast;", DB);
            MySqlCommand buchung_command = new MySqlCommand($"SELECT `buchung`.*, `zimmer`.*\r\nFROM `buchung` \r\n\tLEFT JOIN `zimmer` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID`;", DB);

            if (gast_ID != null)
            {
                buchung_command = new MySqlCommand($"SELECT `buchung`.*, `buchung_hat_gast`.`Gast_ID`, `zimmer`.*\r\nFROM `buchung` \r\n\tLEFT JOIN `buchung_hat_gast` ON `buchung_hat_gast`.`Buchungs_ID` = `buchung`.`Buchungs_ID` \r\n\tLEFT JOIN `zimmer` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID`\r\nWHERE `buchung_hat_gast`.`Gast_ID` = '{gast_ID}';", DB);
            }
            if (buchung_ID != null)
            {
                gast_command = new MySqlCommand($"SELECT `gast`.*, `buchung_hat_gast`.`Buchungs_ID`\r\nFROM `gast` \r\n\tLEFT JOIN `buchung_hat_gast` ON `buchung_hat_gast`.`Gast_ID` = `gast`.`Gast_ID`\r\nWHERE `buchung_hat_gast`.`Buchungs_ID` = '{buchung_ID}';", DB);
            }


            using (var adapter = new MySqlDataAdapter(gast_command))
            {
                GastTabelle_table = new DataTable();
                adapter.Fill(GastTabelle_table);
                dataView_gast = new DataView(GastTabelle_table);
                DG_Gast.ItemsSource = dataView_gast;
            }
        }

        private void GetGastRechnungen(int gastID)
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                cmd.CommandText = "SELECT r.Zahlungsart, COUNT(*) AS Anz_Zimmer, MIN(b.Check_in) AS Check_In, MAX(b.Check_out) AS Check_Out " +
                    "FROM gast g " +
                    "INNER JOIN rechnung r ON g.Gast_ID = r.Gast_ID " +
                    "INNER JOIN buchung b ON r.Rechnungs_ID = b.Rechnungs_ID " +
                    $"WHERE g.Gast_ID = {gastID}";

                using (MySqlDataAdapter adapter = new())
                {
                    adapter.SelectCommand = cmd;

                    DataTable dtBill = new();
                    adapter.Fill(dtBill);
                    dataViewBill = new(dtBill);
                }
                DG_Rechnungen.ItemsSource = dataViewBill;
            }
        }

        private void GetPropRechnung()
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;

                cmd.CommandText = "";
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
            if (sender is TextBox filtertextbox && dataView_gast != null)
            {
                string filtertext = filtertextbox.Text;

                if (string.IsNullOrWhiteSpace(filtertext) || filtertext == filtertextbox.Name)
                {
                    dataView_gast.RowFilter = string.Empty;
                }
                else
                {
                    dataView_gast.RowFilter = $"{filtertextbox.Name} LIKE '%{filtertext}%'";
                }

            }
        }


        private void Filter_Rechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox filtertextbox && dataViewBill != null && filtertextbox.Text != null)
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
            if (dataViewBill != null)
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
                    dataViewBill.RowFilter = string.Join(" AND ", filterConditions);
                }
                else
                {
                    dataViewBill.RowFilter = string.Empty;
                }
            }
        }

        private void DP_Date_Initialized(object sender, EventArgs e)
        {
            DP_Date.SelectedDate = DateTime.Now;
        }

        private void DateChanged(object sender, SelectionChangedEventArgs e)
        {
            Buchung_Filtern();
        }

        private void BuchungTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Rechnungen.SelectedItem is DataRowView selectedRow)
            {
                //tabellenfüllen((int)selectedRow.Row[0], null);
                if (DG_Rechnungen.IsFocused == false)
                {
                    DG_Rechnungen.Focus();
                }
            }
        }

        private void GastTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Gast.SelectedItem is DataRowView selectedRow)
            {
                GetGastRechnungen((int)selectedRow.Row[0]);
                if (DG_Gast.IsFocused == false)
                {
                    DG_Gast.Focus();
                }
            }
        }

        private void Tabelle_LostFocus(object sender, RoutedEventArgs e)
        {
            tabellenfüllen(null, null);
        }
    }
}
