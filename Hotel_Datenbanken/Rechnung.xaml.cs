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
    /// Interaktionslogik für Rechnung.xaml
    /// </summary>
    public partial class Rechnung : Page
    {
        MySqlConnection DB;
        int Rechnung_ID;
        public Rechnung(MySqlConnection DB, int Rechnung_ID)
        {
            InitializeComponent();
            this.DB = DB;
            this.Rechnung_ID = Rechnung_ID;

            Rechnung_ID = 1;

            Preise.Text = $" Kunde: {getGast(Rechnung_ID)}\r\n{Zimmmer(Rechnung_ID)}";
        }

        string getGast(int Rechnung_ID)
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT `rechnung`.`Rechnungs_ID`, `gast`.`Vorname`, `gast`.`Nachname`, `gast`.`Email`, `gast`.`Telefonnummer`\r\nFROM `rechnung` \r\n\tLEFT JOIN `gast` ON `rechnung`.`Gast_ID` = `gast`.`Gast_ID`\r\nWHERE `rechnung`.`Rechnungs_ID` = '{Rechnung_ID}';", DB);

            using (var adapter = new MySqlDataAdapter(cmd))
            {
                DataTable GastTabelle_table = new DataTable();
                adapter.Fill(GastTabelle_table);

                if (GastTabelle_table.Rows.Count > 0)
                {
                    DataRow row = GastTabelle_table.Rows[0];
                    return $"{row["Vorname"]} {row["Nachname"]}\r\n Email: {row["Email"]}\r\n Tel: {row["Telefonnummer"]}\r\n";
                }
            }
            return "Kein Gast gefunden";
        }

        string Zimmmer(int Rechnung_ID)
        {
            string returnstring = "";
            decimal Gesamtpreis = 0;
            MySqlCommand cmd = new MySqlCommand($"SELECT `rechnung`.`Rechnungs_ID`, `buchung`.*, `zimmer`.*\r\nFROM `rechnung` \r\n\tLEFT JOIN `buchung` ON `buchung`.`Rechnungs_ID` = `rechnung`.`Rechnungs_ID` \r\n\tLEFT JOIN `zimmer` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID`\r\nWHERE `rechnung`.`Rechnungs_ID` = '{Rechnung_ID}';", DB);
            DataTable Buchung_table = new DataTable();

            using (var adapter = new MySqlDataAdapter(cmd))
            {
                adapter.Fill(Buchung_table);   
            }

            foreach (DataRow row in Buchung_table.Rows)
            {
                decimal zimmerpreis = Zimmerpreis(row);
                decimal zusatzleistungkosten = 0;
                int Tage = ((DateTime)row["Check_out"] - (DateTime)row["Check_in"]).Days;

                returnstring += $"Zimmer {row["Zimmernummer"]} = {zimmerpreis}\r\nTage = {Tage}\r\n";

                returnstring += Zusatzleistung((int)row["Buchungs_ID"],ref zusatzleistungkosten);

                returnstring += $"\r\nZimmerpreis = {(zimmerpreis * Tage) + zusatzleistungkosten}";

                Gesamtpreis += (zimmerpreis * Tage) + zusatzleistungkosten;
            }

            return returnstring += $"\r\nGesamt Preis = {Gesamtpreis}";
        }

        string Zusatzleistung(int Buchungs_ID, ref decimal zusatzleistungkosten)
        {
            string returnstring = "";
            decimal returnpreis = 0;

            MySqlCommand cmd = new MySqlCommand($"SELECT `beinhaltet`.*, `zusatzleistung`.*\r\nFROM `beinhaltet` \r\n\tLEFT JOIN `zusatzleistung` ON `beinhaltet`.`Zusatzleistungs_ID` = `zusatzleistung`.`Zusatzleistungs_ID`\r\nWHERE `beinhaltet`.`Buchungs_ID` = '{Buchungs_ID}';", DB);
            DataTable Zusatzleistung_table = new DataTable();

            using (var adapter = new MySqlDataAdapter(cmd))
            {
                adapter.Fill(Zusatzleistung_table);
            }

            foreach (DataRow row in Zusatzleistung_table.Rows)
            {
                int typ_kosten = (int)row["Preis"];
                int Tage = ((DateTime)row["End_Datum"] - (DateTime)row["Start_Datum"]).Days;
                returnpreis += typ_kosten * Tage;
                returnstring += $"Zusatzleistung {row["Zusatzleistung"]} = {typ_kosten}\r\nTage = {Tage}\r\n";
            }
            zusatzleistungkosten = returnpreis;

            return returnstring += $"\r\nzusatleisung kosten = {returnpreis}";
            // "Zusatzleistung {Zusatzleistung_typ} = {kosten}\r\nTage = {Tage}\r\nzusatleisung kosten = {gesamtkosten}";
        }

        decimal Zimmerpreis(DataRow Zimmer_row)
        {
            decimal returndecimal = 0;
            Boolean terrasse = (string)Zimmer_row["Terrasse"] == "Ja";
            Boolean nicht_straße = (string)Zimmer_row["Aussicht_Strasse"] == "Nein";

            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `preis`;", DB);
            using (var adapter = new MySqlDataAdapter(cmd))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow preis_row in table.Rows)
                {
                    if ((string)Zimmer_row["Zimmertyp"] == (string)preis_row["Kategorie"])
                    {
                        returndecimal += (decimal)preis_row["Preis"];
                    }

                    if ((string)Zimmer_row["Balkon"] == (string)preis_row["Kategorie"])
                    {
                        returndecimal += (decimal)preis_row["Preis"];
                    }

                    if (terrasse && (string)preis_row["Kategorie"] == "Terrasse")
                    {
                        returndecimal += (decimal)preis_row["Preis"];
                    }

                    if (nicht_straße && (string)preis_row["Kategorie"] == "Nicht Straße")
                    {
                        returndecimal += (decimal)preis_row["Preis"];
                    }
                }
            }
            return returndecimal;
        }


    }
}
