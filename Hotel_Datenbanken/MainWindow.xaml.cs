using MySqlConnector;
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
        public MainWindow()
        {
            InitializeComponent();
            Connection();
        }

        public void Connection()
        {
            using (var connection = new MySqlConnection("Server=localhost; User ID = root; Password = ; Database = hotel")) 
            { 
                connection.Open();

                using (var command = new MySqlCommand("SELECT * FROM gast; ",connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetInt32(0) == 1) { 
                            test.Text = reader.GetString(1);
                        }
                    }
                }
            }
        }
    }
}