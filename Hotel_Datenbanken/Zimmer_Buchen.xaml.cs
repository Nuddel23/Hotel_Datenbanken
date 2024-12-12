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
    /// Interaktionslogik für Zimmer_Buchen.xaml
    /// </summary>
    public partial class Zimmer_Buchen : Page
    {
        MySqlConnection DB;
        Frame frame;
        public Zimmer_Buchen(MySqlConnection DB, Frame frame)
        {
            InitializeComponent();
            this.DB = DB;
            this.frame = frame;
            Connection();
        }

        public void Connection()
        {
            var command = new MySqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA='test'", DB);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    combobox.Items.Add(reader.GetString(0));
                    //test.Text = reader.GetString(i);
                }
            }
        }

        private void combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTable GastTablle = new DataTable();

            using (var command = new MySqlCommand($"SELECT * FROM " + combobox.SelectedValue + "; ", DB))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(GastTablle);
                }
            }
            tabelle.ItemsSource = GastTablle.DefaultView;
        }
    }
}
