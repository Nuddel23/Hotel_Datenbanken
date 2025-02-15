using MySqlConnector;
using System.Windows;

namespace Hotel_Datenbanken
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection DB;

        Homepage homepage;
        Gäste gäste;
        Zimmer_Buchen zimmer_buchen;
        ErrorPage errorpage;
        Buchungen_einsehen buchungen_einsehen;
        Rechnung_einsehen rechnung_einsehen;

        public MainWindow()
        {
            InitializeComponent();

            DB = new MySqlConnection("Server=localhost; User ID = root; Password = root; Database = hotel");

            try
            {
                DB.Open();
                homepage = new Homepage(DB);
                Main.Content = homepage;
            }
            catch (Exception ex)
            {
                errorpage = new ErrorPage("Datenbank konnte nicht verbunden werden \n\r" + ex.Message);
                Main.Content = errorpage;
                ButtonMenu.Visibility = Visibility.Hidden;
            }
        }

        private void Homepage_Click(object sender, RoutedEventArgs e)
        {
            homepage = new Homepage(DB);
            Main.Content = homepage;
        }

        private void Zimmer_Buchen_Click(object sender, RoutedEventArgs e)
        {
            zimmer_buchen = new Zimmer_Buchen(DB, Main);
            Main.Content = zimmer_buchen;
        }

        private void Gäste_Click(object sender, RoutedEventArgs e)
        {
            gäste = new Gäste(DB);
            Main.Content = gäste;
        }

        private void Buchungen_einsehen_Click(object sender, RoutedEventArgs e)
        {
            buchungen_einsehen = new Buchungen_einsehen(DB,0);
            Main.Content = buchungen_einsehen;
        }

        private void Rechnung_einsehen_Click(object sender, RoutedEventArgs e)
        {
            rechnung_einsehen = new Rechnung_einsehen(DB);
            Main.Content = rechnung_einsehen;
        }
    }
}
