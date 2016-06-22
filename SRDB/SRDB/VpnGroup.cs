using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRDB
{
    public class VpnGroup
    {
        public string Name { get; set; }

        public string Descript { get; set; }

        public VpnGroup() { }

        public VpnGroup(string name, string descript)
        {
            Name = name;
            Descript = descript;
        }
    }
}
