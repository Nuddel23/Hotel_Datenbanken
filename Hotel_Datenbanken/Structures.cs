using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Datenbanken
{
    public static class Structure
    {

        public class NewBuchung
        {
            public int? GuestId { get; set; }
            public List<int> RoomNrs { get; set; }
            public DateOnly CheckIn { get; set; }
            public DateOnly CheckOut { get; set; }
            public string PayMethode { get; set; }
            public List<int> Additionals { get; set; }

        }

        public class Additional
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateOnly Start { get; set; }
            public DateOnly End { get; set; }

        }

        public class Buchung
        {
            public int RoomNr { get; set; }
            public List<Additional> Additionals { get; set; }
            public DateOnly CheckIn { get; set; }
            public DateOnly CheckOut { get; set; }
            public float Price { get; set; }
        }

        public class Rechnung
        {
            public int GuestID { get; set; }
            public List<Buchung> Buchungen { get; set; }
            public DateOnly CheckIn { get; set; }
            public DateOnly CheckOut { get; set; }
            public float Price { get; set; }
        }

    }
}
