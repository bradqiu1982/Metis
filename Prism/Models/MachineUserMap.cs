using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Prism.Models
{
    public class MachineUserMap
    {

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

        private static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        public static MachineUserMap GetUserInfo(string ip,string ugroup = null)
        {
            var ret = new MachineUserMap();

            #if DEBUG
            if (string.Compare(ip, "127.0.0.1") == 0)
            {
                ret.username = "BRAD.QIU";
                ret.machine = "SHG-L80003583";
                ret.level = 10;
                return ret;
            }
            #endif

            
            string machine = DetermineCompName(ip);
            var sql = "select machine,username,level from machineusermap where machine = '<machine>' ";
            sql = sql.Replace("<machine>", machine);
            if (ugroup != null)
            {
                sql = sql + " and ugroup like '" + ugroup + "%'";
            }

            var dbret = DBUtility.ExeNPISqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                
                ret.machine = Convert.ToString(line[0]);
                ret.username =  Convert.ToString(line[1]);
                var slv = Convert.ToString(line[2]);
                if (!string.IsNullOrEmpty(slv) && slv.Length > 1)
                { ret.level = Convert.ToInt32(slv.Substring(1)); }
                return ret;
            }

            return ret;
        }

        public static bool IsLxEmployee(string ip, string username, int x,string msg=null)
        {
            #if DEBUG
            if (string.Compare(ip, "127.0.0.1") == 0)
            { return true; }
            #endif

            string machine = DetermineCompName(ip);

            var sql = "select machine,username,level from machineusermap where machine = '<machine>' ";
            sql = sql.Replace("<machine>", machine);

            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " or username = '<username>'";
                sql = sql.Replace("<username>", username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper());
            }

            var dbret = DBUtility.ExeNPISqlWithRes(sql, null);
            if (dbret.Count == 0)
            {
                return false;
            }

            try
            {
                foreach (var line in dbret)
                {
                    var level = Convert.ToString(line[2]);
                    var name = Convert.ToString(line[1]);
                #if !DEBUG
                    if (!string.IsNullOrEmpty(msg))
                    {
                        Log(machine, name, msg);
                    }
                #endif
                    if (!string.IsNullOrEmpty(level) && level.Length > 1)
                    {
                        var lv = Convert.ToInt32(level.Substring(1));
                        if (lv >= x || lv == 0)
                        {
                            return true;
                        }
                    }//end if
                }//en foreach
            }
            catch (Exception ex) { }

            return false;
        }

        public static string EmployeeName(string ip)
        {
            string machine = DetermineCompName(ip);

            var sql = "select machine,username,level from machineusermap where machine = '<machine>' ";
            sql = sql.Replace("<machine>", machine);
            var dbret = DBUtility.ExeNPISqlWithRes(sql, null);
            if (dbret.Count == 0)
            {
                return "System";
            }
            return Convert.ToString(dbret[0][0]);
        }

        private static void Log(string machine, string name, string msg)
        {
            var sql = "insert into WebLog(Machine,Name,MSG,UpdateTime) values(@Machine,@Name,@MSG,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Machine", machine);
            dict.Add("@Name", name);
            dict.Add("@MSG",msg);
            dict.Add("@UpdateTime",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }


        public MachineUserMap()
        {
            machine = "";
            username = "";
            level = -1;
        }

        public string machine { set; get; }
        public string username { set; get; }
        public int level { set; get; }

    }


}