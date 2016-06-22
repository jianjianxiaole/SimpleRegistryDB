using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRDB
{
    /// <summary>
    /// Host, Descript,User,Password
    /// </summary>
    public class VpnItem
    {
        public string Id { get; private set; }

        public string Host { get; set; }

        public string Descript { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public VpnItem()
        {
            Id = Guid.NewGuid().ToString();
        }

        public VpnItem(string host, string descript, string user, string passwd)
        {
            Host = host;
            Descript = descript;
            User = user;
            Password = passwd;
            Id = Guid.NewGuid().ToString();
        }
    }
}
