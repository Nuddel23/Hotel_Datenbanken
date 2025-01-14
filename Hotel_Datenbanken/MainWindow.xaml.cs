﻿using MySqlConnector;
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
        MySqlConnection DB;

        Homepage homepage;
        Gäste gäste;
        Zimmer_Buchen zimmer_buchen;
        ErrorPage errorpage;

        public MainWindow()
        {
            InitializeComponent();

            DB = new MySqlConnection("Server=localhost; User ID = root; Password = root; Database = hotel");

            try
            {
                DB.Open();
                homepage = new Homepage(DB, Main);
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
            homepage = new Homepage(DB, Main);
            Main.Content = homepage;
        }

        private void Zimmer_Buchen_Click(object sender, RoutedEventArgs e)
        {
            zimmer_buchen = new Zimmer_Buchen(DB, Main);
            Main.Content = zimmer_buchen;
        }

        private void Gäste_Click(object sender, RoutedEventArgs e)
        {
            gäste = new Gäste(DB, Main);
            Main.Content = gäste;
        }
    }
}
