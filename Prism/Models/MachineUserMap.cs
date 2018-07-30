using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prism.Models
{
    public class MachineUserMap
    {
        public MachineUserMap()
        {
            machine = "";
            username = "";
        }

        public static void AddMachineUserMap(string machine, string username)
        {
            var sql = "delete from machineusermap where machine = '<machine>'";
            sql = sql.Replace("<machine>", machine);
            DBUtility.ExeLocalSqlNoRes(sql);

            var tempname = username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim();

            sql = "insert into machineusermap(machine,username) values(@machine,@username)";
            var param = new Dictionary<string, string>();
            param.Add("@machine", machine.ToUpper().Trim());
            param.Add("@username", tempname.ToUpper().Trim());
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void TryAddMachineUserMap(string machine, string username)
        {
            var exist = RetrieveUserMap(machine);
            if (exist.Count == 0)
            {
                var tempname = username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim();
                var sql = "insert into machineusermap(machine,username) values(@machine,@username)";
                var param = new Dictionary<string, string>();
                param.Add("@machine", machine.ToUpper().Trim());
                param.Add("@username", tempname.ToUpper().Trim());
                DBUtility.ExeLocalSqlNoRes(sql, param);
            }
        }

        public static Dictionary<string, string> RetrieveUserMap(string machine = "", string username = "")
        {
            var ret = new Dictionary<string, string>();

            var sql = "select machine,username from machineusermap where 1 = 1";
            if (!string.IsNullOrEmpty(machine))
            {
                sql = sql + " and machine = '<machine>'";
                sql = sql.Replace("<machine>", machine);
            }
            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " and username = '<username>'";
                sql = sql.Replace("<username>", username);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new MachineUserMap();
                tempvm.machine = Convert.ToString(line[0]);
                tempvm.username = Convert.ToString(line[1]);
                if (!ret.ContainsKey(tempvm.machine))
                {
                    ret.Add(tempvm.machine, tempvm.username);
                }
            }
            return ret;
        }

        public string machine { set; get; }
        public string username { set; get; }
    }
}