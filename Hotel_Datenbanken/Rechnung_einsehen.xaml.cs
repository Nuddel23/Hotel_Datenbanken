using MySqlConnector;
using System.Data;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;


namespace Hotel_Datenbanken
{
    public partial class Rechnung_einsehen : Page
    {
        MySqlConnection DB;
        DataTable GastTabelle_table = new DataTable();
        DataTable BuchungTabelle_table = new DataTable();
        DataView? dataViewGast;
        DataView? dataViewBill;

        string[] Buchungfiltertypen = { "Zimmertyp", "Check_out", "Check_in", "Balkon", "Terrasse", "Aussicht_Strasse", "Zimmernummer" };
        string[] Buchungfilter = new string[7];

        public Rechnung_einsehen(MySqlConnection DB)
        {
            InitializeComponent();
            this.DB = DB;
            DP_Date.SelectedDate = DateTime.Now;
            GetRechnungsGast();
        }

        private void GetGastsRechnungen(int gastID)
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                cmd.CommandText = "SELECT r.Zahlungsart, COUNT(*) AS Anz_Zimmer, MIN(DATE_FORMAT(b.Check_In, '%d.%m.%Y')) AS Check_In, MAX( DATE_FORMAT(b.Check_Out, '%d.%m.%Y')) AS Check_Out " +
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

        private void GetPropRechnung(bool date, bool roomNr)
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;

                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT r.Zahlungsart, COUNT(*) AS Anz_Zimmer, MIN(DATE_FORMAT(b.Check_In, '%d.%m.%Y')) AS Check_In, MAX( DATE_FORMAT(b.Check_Out, '%d.%m.%Y')) AS Check_Out " +
                    "FROM rechnung r " +
                    "INNER JOIN buchung b ON r.Rechnungs_ID = b.Rechnungs_ID " +
                    "WHERE r.Rechnungs_ID IN (" +
                    "   SELECT DISTINCT b2.Rechnungs_id " +
                    "   FROM buchung b2" +
                    "   INNER JOIN zimmer z ON b2.Zimmer_ID = z.Zimmer_ID" +
                    "   WHERE 1=1");

                if(date)
                {
                    sb.Append(
                    $" AND '{DP_Date.SelectedDate!.Value.ToString("yyyy-MM-dd")}' BETWEEN b2.Check_In AND b2.Check_Out"
                    );
                    Debug.Print("Date");
                }

                if(roomNr)
                {
                    sb.Append($" AND z.Zimmernummer = {TB_RaumNr.Text}");
                    Debug.Print("Nummer");
                }

                sb.Append(") GROUP BY r.Rechnungs_ID");

                cmd.CommandText = sb.ToString();
               
                using (MySqlDataAdapter adapter = new())
                {
                    adapter.SelectCommand = cmd;

                    DataTable dtGast = new();
                    adapter.Fill(dtGast);
                    dataViewBill = new(dtGast);
                }
                DG_Rechnungen.ItemsSource = dataViewBill;
            }
        }

        private void GetRechnungsGast(int? gastId = null)
        {

            using (MySqlCommand cmd = new())
            {
                StringBuilder sb = new StringBuilder();

                cmd.Connection = DB;
                 sb.Append("SELECT g.Gast_ID, concat(g.Vorname, ' ', g.Nachname) AS 'Name', g.Email, g.Telefonnummer,concat(concat(a.Straße, ' ', a.Hausnummer), ', ', concat(p.PLZ, ' ', p.Ort)) AS Adresse " +
                    "FROM gast g " +
                    "INNER JOIN adresse a ON g.Adress_ID = a.Adress_ID " +
                    "INNER JOIN plz p ON a.PLZ = p.PLZ ");

                if(gastId != null)
                {
                    sb.Append($"WHERE g.Gast_ID = {gastId}");
                }

                cmd.CommandText = sb.ToString();

                using (MySqlDataAdapter adapter = new())
                {
                    adapter.SelectCommand = cmd;

                    DataTable dtBill = new();
                    adapter.Fill(dtBill);
                    dataViewGast = new(dtBill);
                }
                DG_Gast.ItemsSource = dataViewGast;
                DG_fill();
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
            if (sender is TextBox filtertextbox && dataViewGast != null)
            {
                string filtertext = filtertextbox.Text;

                if (string.IsNullOrWhiteSpace(filtertext) || filtertext == filtertextbox.Name)
                {
                    dataViewGast.RowFilter = string.Empty;
                }
                else
                {
                    dataViewGast.RowFilter = $"{filtertextbox.Name} LIKE '%{filtertext}%'";
                }

            }
        }


        private void RoomNr_TextChanged(object sender, TextChangedEventArgs e)
        {
                GetPropRechnung(DP_Date.SelectedDate != null, TB_RaumNr.Text.Length > 0);
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
            
        }

        private void BuchungTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Rechnungen.SelectedItem is DataRowView selectedRow)
            {
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
                Debug.Print(selectedRow.Row[0].ToString());
                GetGastsRechnungen((int)selectedRow.Row[0]);
                if (DG_Gast.IsFocused == false)
                {
                    DG_Gast.Focus();
                }
            }
        }

        private void DP_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
                GetPropRechnung(DP_Date.SelectedDate != null, TB_RaumNr.Text.Length > 0);
        }

        private void TB_RaumNr_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void DG_fill()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DG_Gast.Columns.Count > 0)
                {
                    DG_Gast.Columns[0].Visibility = Visibility.Collapsed; // Erste Spalte verstecken
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }
}
