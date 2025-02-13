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
    /// Interaktionslogik für Homepage.xaml
    /// </summary>
    public partial class Homepage : Page
    {
        MySqlConnection DB;
        Frame frame;
        public Homepage(MySqlConnection DB, Frame frame)
        {
            InitializeComponent();
            this.DB = DB;
            this.frame = frame;
            FillTables();
        }

        private void FillTables()
        {
            using (MySqlCommand cmd = new())
            {
                cmd.Connection = DB;
                cmd.CommandText = "SELECT concat(g.Nachname, \", \", g.Vorname) AS Gast, z.Zimmernummer, z.Zimmertyp, b.Check_out " +
                    "FROM buchung b " +
                    "INNER JOIN zimmer z ON b.Zimmer_ID = z.Zimmer_ID " +
                    "INNER JOIN rechnung r ON b.Rechnungs_ID = r.Rechnungs_ID " +
                    "INNER JOIN gast g ON r.Gast_ID = g.Gast_ID " +
                    $"WHERE b.Check_in = {DateTime.Today.ToString("yyyy-MM-dd")}";

                using (MySqlDataAdapter adapter = new(cmd))
                {
                    DataTable fillTable = new();

                    if (adapter != null)
                    {
                        adapter.Fill(fillTable);

                        if(fillTable.Rows.Count > 0)
                        {
                            DG_CheckIns.ItemsSource = fillTable.DefaultView;
                        }
                        else
                        {
                            //muss noch
                            return;
                        }
                    }
                }

                cmd.CommandText = "SELECT concat(g.Nachname, \", \", g.Vorname) AS Gast, z.Zimmernummer, z.Zimmertyp " +
                    "FROM buchung b " +
                    "INNER JOIN zimmer z ON b.Zimmer_ID = z.Zimmer_ID " +
                    "INNER JOIN rechnung r ON b.Rechnungs_ID = r.Rechnungs_ID " +
                    "INNER JOIN gast g ON r.Gast_ID = g.Gast_ID " +
                    $"WHERE b.Check_out = {DateTime.Today.ToString("yyyy-MM-dd")}";

                using (MySqlDataAdapter adapter = new(cmd))
                {
                    DataTable fillTable = new();

                    if (adapter != null)
                    {
                        adapter.Fill(fillTable);

                        DG_CheckOuts.ItemsSource = fillTable.DefaultView;
                    }
                }

                //muss noch
                cmd.CommandText = "";

            }
        }
    }
}
