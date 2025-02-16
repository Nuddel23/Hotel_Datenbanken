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

        public Homepage(MySqlConnection DB)
        {
            InitializeComponent();
            this.DB = DB;
            Filltabellen();
        }

        void Filltabellen()
        {
            using (var command = new MySqlCommand($"SELECT concat(g.Nachname, \", \", g.Vorname) AS Gast, z.Zimmernummer, z.Zimmertyp, b.Check_out " +
                    "FROM buchung b " +
                    "INNER JOIN zimmer z ON b.Zimmer_ID = z.Zimmer_ID " +
                    "INNER JOIN rechnung r ON b.Rechnungs_ID = r.Rechnungs_ID " +
                    "INNER JOIN gast g ON r.Gast_ID = g.Gast_ID " +
                    $"WHERE b.Check_in = CURRENT_DATE()", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DG_CheckIns.ItemsSource = dt.DefaultView;
                }
            }

            using (var command = new MySqlCommand($"SELECT concat(g.Nachname, \", \", g.Vorname) AS Gast, z.Zimmernummer, z.Zimmertyp " +
                    "FROM buchung b " +
                    "INNER JOIN zimmer z ON b.Zimmer_ID = z.Zimmer_ID " +
                    "INNER JOIN rechnung r ON b.Rechnungs_ID = r.Rechnungs_ID " +
                    "INNER JOIN gast g ON r.Gast_ID = g.Gast_ID " +
                    $"WHERE b.Check_out = CURRENT_DATE()", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DG_CheckOuts.ItemsSource = dt.DefaultView;
                }
            }

            using (var command = new MySqlCommand($"SELECT `zimmer`.`Zimmernummer`, `zimmer`.`Zimmertyp`, `zimmer`.`Etage`, `zimmer`.`Terrasse`, `zimmer`.`Balkon`, `zimmer`.`Aussicht_Strasse` FROM `zimmer` LEFT JOIN `buchung` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID` WHERE `Check_in` IS NULL AND `Check_out` IS NULL OR DATE(`buchung`.`Check_in`) > (NOW() + INTERVAL 7 DAY) AND DATE(`buchung`.`Check_out`) > (NOW() + INTERVAL 7 DAY) OR DATE(`buchung`.`Check_in`) < NOW() AND DATE(`buchung`.`Check_out`) < NOW() GROUP BY `zimmer`.`Zimmer_ID`; ", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DG_FreeRooms.ItemsSource = dt.DefaultView;
                }
            }
        }
    }
}
