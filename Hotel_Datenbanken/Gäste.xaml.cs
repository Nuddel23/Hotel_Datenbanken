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
    /// Interaktionslogik für Gäste.xaml
    /// </summary>
    public partial class Gäste : Page
    {
        MySqlConnection DB;
        Frame frame;

        public Gäste(MySqlConnection DB, Frame frame)
        {
            InitializeComponent();
            this.DB = DB;
            this.frame = frame;
            zeige_Gäste();
        }

        void zeige_Gäste()
        {
            DataTable GastTablle = new DataTable();

            using (var command = new MySqlCommand($"SELECT * FROM gast;", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(GastTablle);
                }
            }
            tabelle.ItemsSource = GastTablle.DefaultView;
        }

        private void Gast_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            Gast_Frame.Content = new Gast_hinzufügen();
            Gast_Frame.Visibility = Visibility.Visible;
            tabelle.Visibility = Visibility.Hidden;
        }

        private void Gast_bearbeiten_Click(object sender, RoutedEventArgs e)
        {
            Gast_Frame.Content = new Gast_bearbeiten();
            Gast_Frame.Visibility = Visibility.Visible;
            tabelle.Visibility = Visibility.Hidden;
        }

        private void Gast_Main(object sender, RoutedEventArgs e)
        {
            Gast_Frame.Visibility = Visibility.Hidden;
            tabelle.Visibility = Visibility.Visible;
        }
    }
}
