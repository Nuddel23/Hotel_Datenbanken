using MySqlConnector;

namespace Hotel_Datenbanken
{
    internal class Calculate
    {
        public static int RechnungPrice(int rechnungsId, MySqlConnection DB)
        {
            int completePrice = 0;

            string query = "SELECT b.Buchungs_ID " +
                "FROM rechnung r " +
                "INNER JOIN buchung b ON r.Rechnungs_ID = b.Rechnungs_ID " +
                $"WHERE r.Rechnungs_ID = {rechnungsId}";

            MySqlCommand cmd = new(query, DB);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                List<int> bookings = new List<int>();
                while (reader.Read())
                {
                    bookings.Add(reader.GetInt32(0));
                }
                reader.Close();
                foreach (int booking in bookings)
                {
                    completePrice += BuchungPrice(booking, DB);
                }
            }
            return completePrice;
        }

        public static int BuchungPrice(Structure.NewBuchung buchung, MySqlConnection DB)
        {
            MySqlCommand cmd;
            MySqlDataReader reader;
            int price = 0;
            int days = buchung.CheckOut.DayNumber - buchung.CheckIn.DayNumber;

            foreach (int roomNr in buchung.RoomNrs)
            {
                string query = "SELECT p.Preis " +
                        "FROM zimmer z " +
                        "INNER JOIN preis p On z.Zimmertyp = p.Kategorie " +
                        $"WHERE z.Zimmer_ID = {roomNr}";
                cmd = new(query, DB);
                reader = cmd.ExecuteReader();

                price += reader.GetInt32(0) * days;
                reader.Close();

                query = "SELECT " +
                    "IF(z.Terrasse = \"Ja\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Terrasse\"), 0) + " +
                    "IF(z.Balkon = \"Großer Balkon\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Großer Balkon\"), 0) + " +
                    "IF(z.Balkon = \"Kleiner Balkon\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Kleiner Balkon\"), 0) + " +
                    "IF(z.Aussicht_Strasse = \"Nein\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Nicht Straße\"), 0) AS Preis " +
                    "FROM zimmer z " +
                    $"WHERE z.Zimmer_ID = {roomNr}";

                cmd = new(query, DB);
                reader = cmd.ExecuteReader();
                reader.Read();
                price += reader.GetInt32(0) * days;
            }

            if (buchung.Additionals != null)
            {
                foreach (int zusatzleistung in buchung.Additionals)
                {
                    string query = "SELECT z.Preis " +
                        "FROM zusatzleistung z " +
                        $"WHERE z.Zusatzleistungs_Id = {zusatzleistung}";
                    cmd = new(query, DB);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    price += reader.GetInt32(0) * days;
                    reader.Close();
                }
            }

            return price;
        }

        public static int BuchungPrice(int buchungsId, MySqlConnection DB)
        {
            int price;

            string query = "SELECT p.Preis, b.Check_in, b.Check_out " +
                    "FROM buchung b " +
                    "INNER JOIN zimmer z ON b.Zimmer_ID = z.Zimmer_ID " +
                    "INNER JOIN preis p On z.Zimmertyp = p.Kategorie " +
                    $"WHERE b.Buchungs_ID = {buchungsId}";

            MySqlCommand cmd = new(query, DB);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int days = (reader.GetDateTime(2) - reader.GetDateTime(1)).Days;
            price = reader.GetInt32(0) * days;
            reader.Close();

            query = "SELECT " +
                "IF(z.Terrasse = \"Ja\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Terrasse\"), 0) + " +
                "IF(z.Balkon = \"Großer Balkon\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Großer Balkon\"), 0) + " +
                "IF(z.Balkon = \"Kleiner Balkon\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Kleiner Balkon\"), 0) + " +
                "IF(z.Aussicht_Strasse = \"Nein\", (SELECT p.preis FROM preis p WHERE p.Kategorie = \"Nicht Straße\"), 0) AS Preis " +
                "FROM buchung b " +
                "INNER JOIN zimmer z ON b.Zimmer_ID = z.Zimmer_ID " +
                $"WHERE b.Buchungs_ID = {buchungsId}";

            cmd = new(query, DB);
            reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                price += reader.GetInt32(0) * days;

            }
            reader.Close();

            query = "SELECT z.Preis, be.Start_Datum, be.End_Datum " +
                "FROM buchung b " +
                "INNER JOIN beinhaltet be ON b.Buchungs_ID = be.Buchungs_ID " +
                "INNER JOIN zusatzleistung z ON be.Zusatzleistungs_ID = z.Zusatzleistungs_ID " +
                $"WHERE b.Buchungs_ID = {buchungsId}";

            cmd = new(query, DB);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                days = (reader.GetDateTime(2) - reader.GetDateTime(1)).Days;
                price += reader.GetInt32(0) * days;
            }
            reader.Close();



            return price;
        }
    }
}
