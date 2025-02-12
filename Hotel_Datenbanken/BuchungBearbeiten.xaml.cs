using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hotel_Datenbanken
{
    /// <summary>
    /// Interaktionslogik für Zimmer_Buchen.xaml
    /// </summary>
    public partial class BuchungBearbeiten : Page
    {
        readonly MySqlConnection DB;
        readonly Frame frame;

        public BuchungBearbeiten(MySqlConnection DB, Frame frame)
        {
            this.DB = DB;
            this.frame = frame;
            InitializeComponent();
        }

    }
}
