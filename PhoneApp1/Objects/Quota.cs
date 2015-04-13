using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneApp1.Objects
{
    class Quota
    {
        public bool IsUserPlus { get; set; }
        public int Free { get; set; }
        public int Max { get; set; }
        public int HDQuota { get; set; }
        public int SDQuota { get; set; }

        public Quota(bool isUserPlus, int free, int max, int hDQuota, int sDQuota)
        {
            IsUserPlus = isUserPlus;
            Free = free;
            Max = max;
            HDQuota = hDQuota;
            SDQuota = sDQuota;
        }

        public Quota()
        {
        }
    }
}
