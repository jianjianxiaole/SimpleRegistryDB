using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Collections;

namespace SRDB
{
    /// <summary>
    /// Simple registry data store.
    /// </summary>
    public class SampleRegistyDB
    {
        #region Properites
        private string rootKeyName = @"SRDBV1.0.0";
        #endregion

        public string DbName { get; private set; }

        private RegistryKey CurrentDb
        {
            get
            {
                return Registry.CurrentUser.CreateSubKey(DbName);
            }
        }
        /// <summary>
        /// Total records counts.
        /// </summary>
        public int Counts { get; private set; } = 0;

        public SampleRegistyDB(string name)
        {
            DbName = name;
            using (var root = Registry.CurrentUser.CreateSubKey(rootKeyName))
            {
                var db = root.CreateSubKey(DbName);
                db.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gruop"></param>
        /// <param name="descript"></param>
        public void AddGroup(string group, string descript)
        {
            using (CurrentDb)
            {
                if (CurrentDb.GetSubKeyNames().Contains(group))
                    throw new Exception("Group exists");

                var gkey = CurrentDb.CreateSubKey(group, true);
                gkey.SetValue("Gno", CurrentDb.GetSubKeyNames().Count() + 1, RegistryValueKind.DWord);
                gkey.SetValue("Descript", descript, RegistryValueKind.String);
            }
        }

        public int Members(string group)
        {
            using (CurrentDb)
            {
                var gkey = CurrentDb.CreateSubKey(group, false);
                var value = gkey.GetValue(group, 0);
                return Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="group"></param>
        public void Add(VpnItem item, string group)
        {
            using (CurrentDb)
            {
                var gkey = CurrentDb.CreateSubKey(group);
                if (gkey.GetSubKeyNames().Contains(item.Id))
                    throw new InvalidOperationException("item exist");
                //..
                var vKey = gkey.CreateSubKey(item.Id);
                vKey.SetValue("Host", item.Host, RegistryValueKind.String);
                vKey.SetValue("Descript", item.Host, RegistryValueKind.String);
                vKey.SetValue("User", item.Host, RegistryValueKind.String);
                vKey.SetValue("Password", item.Host, RegistryValueKind.String);
            }
            
        }

        public void Update(VpnItem item, string group)
        {
            using (CurrentDb)
            {
                var gkey = CurrentDb.CreateSubKey(group);
                if (!gkey.GetSubKeyNames().Contains(item.Id))
                    throw new InvalidOperationException("item not exist");
                var vKey = gkey.OpenSubKey(item.Id, true);
                vKey.SetValue("Host", item.Host, RegistryValueKind.String);
                vKey.SetValue("Descript", item.Host, RegistryValueKind.String);
                vKey.SetValue("User", item.Host, RegistryValueKind.String);
                vKey.SetValue("Password", item.Host, RegistryValueKind.String);
            }
        }

        public void Delete(VpnItem item, string group)
        {
            using (CurrentDb)
            {
                var gkey = CurrentDb.OpenSubKey(group);
                gkey.DeleteSubKey(item.Id, false);
            }
        }

        public IEnumerable<VpnGroup> Groups()
        {
            using (CurrentDb)
            {
                var gList = new List<VpnGroup>(CurrentDb.GetSubKeyNames().Count());// CurrentDb.GetSubKeyNames();
                gList.AddRange(CurrentDb.GetSubKeyNames().Select<string, VpnGroup>(
                    g => new VpnGroup(g, CurrentDb.OpenSubKey(g).GetValue("Descript")?.ToString())));
                return gList;
            }
        }

        public object FindFirst(string key)
        {
            throw new NotImplementedException();
        }

        public object FindFirst(string key, string group)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> Get(string group)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> All()
        {
            throw new NotImplementedException();
        }

        #region private
        private void Init(string dbname)
        {
            var userKey = Registry.CurrentUser;
            RegistryKey root,db;

            if (!userKey.GetSubKeyNames().Contains(rootKeyName))
                root = userKey.CreateSubKey(rootKeyName, true);
                
            else
                root = userKey.OpenSubKey(rootKeyName, true);
            
            if (!root.GetSubKeyNames().Contains(dbname))
                db = root.CreateSubKey(dbname, true);            
            else
                db = root.OpenSubKey(dbname, true);

            root.Close();     
        }

        

        #endregion
    }
}
