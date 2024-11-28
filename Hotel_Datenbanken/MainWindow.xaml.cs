using MySqlConnector;
using System.Data;
using System.Security.Policy;
using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TabControl TabControl { get; set; }

        MySqlConnection DB;
        public MainWindow()
        {
            InitializeComponent();

     

            TabControl = tabcontrol;
            DB = new MySqlConnection("Server=localhost; User ID = root; Password = ; Database = test");
            DB.Open();
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

        private void combobox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tabcontrol.SelectedIndex = 1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            tabcontrol.SelectedIndex = 0;
        }
    }
}
