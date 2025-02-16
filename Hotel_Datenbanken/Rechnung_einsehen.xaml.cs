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
        readonly Frame frame;
        DataTable GastTabelle_table = new DataTable();
        DataTable BuchungTabelle_table = new DataTable();
        DataView? dataViewGast;
        DataView? dataViewBill;

        string[] Buchungfiltertypen = { "Zimmertyp", "Check_out", "Check_in", "Balkon", "Terrasse", "Aussicht_Strasse", "Zimmernummer" };
        string[] Buchungfilter = new string[7];

        public Rechnung_einsehen(MySqlConnection DB, Frame frame)
        {
            InitializeComponent();
            this.DB = DB;
            this.frame = frame;
            DP_Date.SelectedDate = DateTime.Now;
            GetRechnungsGast();
        }

        private void GetGastsRechnungen(int gastID)
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                cmd.CommandText = "SELECT r.Zahlungsart, COUNT(*) AS Anz_Zimmer, MIN(DATE_FORMAT(b.Check_In, '%d.%m.%Y')) AS Von, MAX( DATE_FORMAT(b.Check_Out, '%d.%m.%Y')) AS Bis " +
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

        private void GetPropRechnung(int? gastID = null)
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;

                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT r.Rechnungs_ID, r.Gast_ID, r.Zahlungsart, COUNT(*) AS Anz_Zimmer, MIN(DATE_FORMAT(b.Check_In, '%d.%m.%Y')) AS Von, MAX( DATE_FORMAT(b.Check_Out, '%d.%m.%Y')) AS Bis " +
                    "FROM rechnung r " +
                    "INNER JOIN buchung b ON r.Rechnungs_ID = b.Rechnungs_ID " +
                    "WHERE r.Rechnungs_ID IN (" +
                    "   SELECT DISTINCT b2.Rechnungs_id " +
                    "   FROM buchung b2" +
                    "   INNER JOIN zimmer z ON b2.Zimmer_ID = z.Zimmer_ID" +
                    "   WHERE 1=1");

                if(DP_Date.SelectedDate != null)
                {
                    sb.Append(
                    $" AND '{DP_Date.SelectedDate!.Value.ToString("yyyy-MM-dd")}' BETWEEN b2.Check_In AND b2.Check_Out"
                    );
                }

                if(TB_RaumNr.Text.Length > 0)
                {
                    sb.Append($" AND z.Zimmernummer = {TB_RaumNr.Text}");
                }

                sb.Append(") ");

                if(gastID != null)
                {
                    sb.Append($"AND r.Gast_ID = {gastID} ");
                }

                sb.Append("GROUP BY r.Rechnungs_ID");
                Debug.WriteLine(sb.ToString());
                cmd.CommandText = sb.ToString();
               
                using (MySqlDataAdapter adapter = new())
                {
                    adapter.SelectCommand = cmd;

                    DataTable dtGast = new();
                    adapter.Fill(dtGast);
                    dataViewBill = new(dtGast);
                }
                DG_Rechnungen.ItemsSource = dataViewBill;
                DG_format(DG_Rechnungen, 2);
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
                Debug.WriteLine(sb.ToString());
                cmd.CommandText = sb.ToString();

                using (MySqlDataAdapter adapter = new())
                {
                    adapter.SelectCommand = cmd;

                    DataTable dtBill = new();
                    adapter.Fill(dtBill);
                    dataViewGast = new(dtBill);
                }
                DG_Gast.ItemsSource = dataViewGast;
                DG_format(DG_Gast);
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
                GetPropRechnung();
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

        private void DG_RechnungSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(DG_Rechnungen.SelectedItem is DataRowView selectedRow)
            {
                GetRechnungsGast((int)selectedRow.Row[1]);
            }
        }

        private void GastTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Gast.SelectedItem is DataRowView selectedRow)
            {
                GetPropRechnung((int)selectedRow.Row[0]);
            }
        }

        private void DP_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            GetPropRechnung();
        }

        private void TB_RaumNr_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void DG_format(DataGrid toFormat, int anzColoumns = 1)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (toFormat.Columns.Count > 0)
                {
                    for (int i = 0; i < anzColoumns; i++)
                    {
                        toFormat.Columns[i].Visibility = Visibility.Collapsed;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void DG_Rechnungen_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DG_Rechnungen.SelectedItem is DataRowView selectedRow)
            { 
                Buchungen_einsehen buchungen_Einsehen = new(DB, (int)selectedRow.Row[0]);
                frame.Content = buchungen_Einsehen;
            }
        }
    }
}
