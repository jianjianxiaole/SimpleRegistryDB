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
    public class SimpleRegistyDB : IDisposable
    {
        #region Properites
        private string rootKeyName = @"SRDBV1.0.0";
        private RegistryKey rootKey;
        #endregion

        public string DbName { get; private set; }

        private RegistryKey WritableDbRoot
        {
            get
            {
                return rootKey.OpenSubKey(DbName,true);
            }
        }

        private RegistryKey ReadOnlyDbRoot
        {
            get
            {
                return rootKey.OpenSubKey(DbName, false);
            }
        }
        /// <summary>
        /// Total records counts.
        /// </summary>
        public int Counts { get; private set; } = 0;

        public SimpleRegistyDB(string name)
        {
            DbName = name;
            rootKey = Registry.CurrentUser.CreateSubKey(rootKeyName, true);
            using (var dbkey = rootKey.CreateSubKey(DbName))
            {
                dbkey.SetValue("CreateTime", DateTime.UtcNow.ToLongTimeString(), RegistryValueKind.String);
            }
        }

        /// <summary>
        /// 添加一个组
        /// </summary>
        /// <param name="gruop"></param>
        /// <param name="descript"></param>
        public void AddGroup(string group, string descript)
        {
            using (WritableDbRoot)
            {
                if (WritableDbRoot.GetSubKeyNames().Contains(group))
                    throw new Exception("Group exists");

                using (var gkey = WritableDbRoot.CreateSubKey(group, true))
                {
                    gkey.SetValue("Gno", WritableDbRoot.GetSubKeyNames().Count() + 1, RegistryValueKind.DWord);
                    gkey.SetValue("Descript", descript, RegistryValueKind.String);
                }
            }
        }

        /// <summary>
        /// 该组下的组员数量
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public int MemberCounts(string group)
        {
            using (WritableDbRoot)
            {
                var gkey = WritableDbRoot.CreateSubKey(group, false);
                var value = gkey.GetValue(group, 0);
                return Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// 向指定组添加一个vpn主机条目
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="group"></param>
        public void Add(VpnItem item, string group)
        {
            using (WritableDbRoot)
            {
                using (var gkey = WritableDbRoot.CreateSubKey(group))
                {
                    if (gkey.GetSubKeyNames().Contains(item.Id))
                        throw new InvalidOperationException("item exist");
                    //..                
                    WriteVpnItemKey(item, gkey.CreateSubKey(item.Id));
                }
            }
            
        }

        

        public void Update(VpnItem item, string group)
        {
            using (WritableDbRoot)
            {
                var gkey = WritableDbRoot.CreateSubKey(group);
                if (!gkey.GetSubKeyNames().Contains(item.Id))
                    throw new InvalidOperationException("条目不存在");
                
                WriteVpnItemKey(item, gkey.CreateSubKey(item.Id));
            }
        }

        public void Delete(VpnItem item, string group)
        {
            using (WritableDbRoot)
            {
                var gkey = WritableDbRoot.OpenSubKey(group);
                gkey.DeleteSubKey(item.Id, false);
            }
        }

        /// <summary>
        /// 删除一个组将删除组下的全部成员
        /// </summary>
        /// <param name="group"></param>
        public void DeleteGroup(string group)
        {
            using (WritableDbRoot)
            {
                WritableDbRoot.DeleteSubKeyTree(group, false);                
            }
        }

        public IEnumerable<VpnGroup> Groups()
        {
            using (WritableDbRoot)
            {
                var gList = new List<VpnGroup>(WritableDbRoot.SubKeyCount);// CurrentDb.GetSubKeyNames();
                gList.AddRange(WritableDbRoot.GetSubKeyNames().Select<string, VpnGroup>(
                    g => new VpnGroup(g, WritableDbRoot.OpenSubKey(g).GetValue("Descript")?.ToString())));
                return gList;
            }
        }

        /// <summary>
        /// 该组下的所有列表
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public IEnumerable<VpnItem> Get(string group)
        {
            using (ReadOnlyDbRoot)
            {
                using (var gkey = ReadOnlyDbRoot.OpenSubKey(group))
                {
                    var vitems = new List<VpnItem>(gkey.SubKeyCount);
                    vitems.AddRange(gkey.GetSubKeyNames().Select<string, VpnItem>( v =>
                        {
                            using (var vkey = gkey.OpenSubKey(v))
                                return ReadVpnFromKey(vkey);
                        }  
                        ));
                    return vitems;
                }                    
            }
        }


        /// <summary>
        /// 返回所有的VPN服务器信息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VpnItem> All()
        {
            using (ReadOnlyDbRoot)
            {
                var groups = ReadOnlyDbRoot.GetSubKeyNames();
                if (groups == null)
                    throw new Exception("没有任何条目信息");
                var vlist = new List<VpnItem>(64);
                foreach(var g in groups)                
                vlist.AddRange(Get(g));
                return vlist;
            }
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

        /// <summary>
        /// key must can be writable
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        private void WriteVpnItemKey(VpnItem item, RegistryKey key)
        {
            key.SetValue("Host", item.Host, RegistryValueKind.String);
            key.SetValue("Group", item.Group, RegistryValueKind.String);
            key.SetValue("Descript", item.Descript, RegistryValueKind.String);
            key.SetValue("User", item.User, RegistryValueKind.String);
            key.SetValue("Password", item.Password, RegistryValueKind.String);
        }


        private VpnItem ReadVpnFromKey(RegistryKey key)
        {

            return new VpnItem(
                key.Name,
                key.GetValue("Host")?.ToString(),
                key.GetValue("Group")?.ToString(),
                key.GetValue("Descript")?.ToString(),
                key.GetValue("User")?.ToString(),
                key.GetValue("Password")?.ToString()
                );
        }

        public void Dispose()
        {
            rootKey?.Close();
        }
        #endregion
    }
}
