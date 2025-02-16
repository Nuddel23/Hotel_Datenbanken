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

            //Preise.Text = $" Kunde: {getGast(Rechnung_ID)}\r\n{Zimmmer(Rechnung_ID)}";
            

            TxtGast.Text = getGast(Rechnung_ID);
            decimal Gesamtkosten = 0;

            DataTable dt = Zimmer2(Rechnung_ID);
            foreach (DataRow row in dt.Rows )
            {
                decimal zimmerpreis = Zimmerpreis(row);
                int ZimmerTage = ((DateTime)row["Check_out"] - (DateTime)row["Check_in"]).Days;

                TextBlock textBlock = new TextBlock();
                textBlock.Text = $"Zimmer {row["Zimmernummer"]} = {zimmerpreis}€\r\nTage = {ZimmerTage}";
                textBlock.FontWeight = FontWeights.Bold;
                Zimmerstack.Children.Add(textBlock);

                //Separator separator = new Separator();
                //separator.Background = "Black";
                //Zimmerstack.Children.Add(separator);

                decimal zusatzleistungkosten = 0;
                StackPanel Zusatzleisungstack = new StackPanel();
                Zusatzleisungstack.Margin = new Thickness(10,0,0,0);
                DataTable Zusatzleisungtable = Zusatzleistung2((int)row["Buchungs_ID"]);
                foreach (DataRow dr in Zusatzleisungtable.Rows)
                {
                    TextBlock zusatzleisung_textblock = new TextBlock();
                    int typ_kosten = (int)dr["Preis"];
                    int zusatzleisungTage = ((DateTime)dr["End_Datum"] - (DateTime)dr["Start_Datum"]).Days;
                    zusatzleistungkosten += typ_kosten * zusatzleisungTage;

                    zusatzleisung_textblock.Text = $"Zusatzleistung {dr["Zusatzleistung"]} = {typ_kosten}€\r\nTage = {zusatzleisungTage}";
                    Zusatzleisungstack.Children.Add(zusatzleisung_textblock);
                    // "Zusatzleistung {Zusatzleistung_typ} = {kosten}\r\nTage = {Tage}\r\nzusatleisung kosten = {gesamtkosten}";
                }
                
                Zimmerstack.Children.Add(Zusatzleisungstack);

                TextBlock zusatzleistungkosten_textblock = new TextBlock();
                zusatzleistungkosten_textblock.FontWeight = FontWeights.Bold;
                zusatzleistungkosten_textblock.Margin = new Thickness(10,0,0,0);
                zusatzleistungkosten_textblock.Text = $"zusatleisung kosten = {zusatzleistungkosten}€";
                Zimmerstack.Children.Add(zusatzleistungkosten_textblock);

                TextBlock Zimmerkosten_textblock = new TextBlock();
                Zimmerkosten_textblock.FontWeight = FontWeights.Bold;
                Zimmerkosten_textblock.Text = $"Zimmerkosten = {zimmerpreis * ZimmerTage + zusatzleistungkosten}€";
                Zimmerstack.Children.Add(Zimmerkosten_textblock);

                Separator separator2 = new Separator();
                //separator.Background = "Black";
                Zimmerstack.Children.Add(separator2);

                Gesamtkosten += zimmerpreis * ZimmerTage + zusatzleistungkosten;
            }
            TxtGesamtpreis.Text = Convert.ToString(Gesamtkosten) + "€";

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
                    return $"{row["Vorname"]} {row["Nachname"]}\r\nEmail: {row["Email"]}\r\nTel: {row["Telefonnummer"]}";
                }
            }
            return "Kein Gast gefunden";
        }

        DataTable Zimmer2(int Rechnung_ID)
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT `rechnung`.`Rechnungs_ID`, `buchung`.*, `zimmer`.*\r\nFROM `rechnung` \r\n\tLEFT JOIN `buchung` ON `buchung`.`Rechnungs_ID` = `rechnung`.`Rechnungs_ID` \r\n\tLEFT JOIN `zimmer` ON `buchung`.`Zimmer_ID` = `zimmer`.`Zimmer_ID`\r\nWHERE `rechnung`.`Rechnungs_ID` = '{Rechnung_ID}';", DB);
            DataTable Buchung_table = new DataTable();

            using (var adapter = new MySqlDataAdapter(cmd))
            {
                adapter.Fill(Buchung_table);
            }
            return Buchung_table;
        }

        DataTable Zusatzleistung2(int Buchungs_ID)
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT `beinhaltet`.*, `zusatzleistung`.*\r\nFROM `beinhaltet` \r\n\tLEFT JOIN `zusatzleistung` ON `beinhaltet`.`Zusatzleistungs_ID` = `zusatzleistung`.`Zusatzleistungs_ID`\r\nWHERE `beinhaltet`.`Buchungs_ID` = '{Buchungs_ID}';", DB);
            DataTable Zusatzleistung_table = new DataTable();

            using (var adapter = new MySqlDataAdapter(cmd))
            {
                adapter.Fill(Zusatzleistung_table);
            }

            return Zusatzleistung_table;
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
