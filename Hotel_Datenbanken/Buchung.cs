using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Datenbanken
{
    public static class BuchungsStructure
    {

        public class NewBuchung
        {

            public List<int> GuestIds { get; set; }
            public int RoomNr { get; set; }
            public DateOnly CheckIn { get; set; }
            public DateOnly CheckOut { get; set; }
            public List<int> Additionals { get; set; }

        }

    }
}
