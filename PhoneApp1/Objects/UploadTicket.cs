using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneApp1.Objects
{
    class UploadTicket
    {
        public string Ticket { get; set; }
        public string URI { get; set; }

        public UploadTicket(string ticket, string uRI)
        {
            URI = uRI;
            Ticket = ticket;
        }

        public UploadTicket()
        {
        }
    }
}
