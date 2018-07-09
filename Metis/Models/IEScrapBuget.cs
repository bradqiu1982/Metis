using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metis.Models
{
    public class IEScrapBuget
    {

        public static Dictionary<string, bool> RetrieveAllKey()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select DataKey from IEScrapBuget";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var key = Convert.ToString(line[0]);
                if (!ret.ContainsKey(key))
                {
                    ret.Add(key, true);
                }
            }
            return ret;
        }

        public static void UpdateData(List<IEScrapBuget> alldata, Dictionary<string, bool> allkey)
        {
            var sql = "";
            var param = new Dictionary<string, string>();
            foreach (var item in alldata)
            {
                param.Clear();
                if (allkey.ContainsKey(item.DataKey))
                {
                    sql = "update IEScrapBuget set OutPut=@OutPut,Scrap=@Scrap where DataKey=@DataKey";
                    param.Add("@OutPut", item.OutPut);
                    param.Add("@Scrap",item.Scrap);
                    param.Add("@DataKey",item.DataKey);
                }
                else
                {
                    sql = "insert into IEScrapBuget(DataKey,PN,CostCenter,OutPut,Scrap,Destination) values(@DataKey,@PN,@CostCenter,@OutPut,@Scrap,@Destination)";
                    param.Add("@DataKey", item.DataKey);
                    param.Add("@PN", item.PN);
                    param.Add("@CostCenter", item.CostCenter);
                    param.Add("@OutPut", item.OutPut);
                    param.Add("@Scrap", item.Scrap);
                    param.Add("@Destination", item.Destination);
                }
                DBUtility.ExeLocalSqlNoRes(sql, param);
            }//end foreach
        }

        public static Dictionary<string, List<IEScrapBuget>> RetrieveDataCentDict(string fyear,string fquarter)
        {
            var ret = new Dictionary<string, List<IEScrapBuget>>();
            var sql = "select PN,CostCenter,[OutPut],Scrap,Destination from IEScrapBuget where DataKey like '%_" + fyear + "_" + fquarter + "%'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new IEScrapBuget();
                tempvm.PN = Convert.ToString(line[0]);
                tempvm.CostCenter = Convert.ToString(line[1]);
                tempvm.OutPut = Convert.ToString(line[2]);
                tempvm.Scrap = Convert.ToString(line[3]);
                tempvm.Destination = Convert.ToString(line[4]);

                if (ret.ContainsKey(tempvm.CostCenter))
                {
                    ret[tempvm.CostCenter].Add(tempvm);
                }
                else
                {
                    var templist = new List<IEScrapBuget>();
                    templist.Add(tempvm);
                    ret.Add(tempvm.CostCenter, templist);
                }
            }//end foreach
            return ret;
        }

        public IEScrapBuget()
        {
            DataKey = "";
            PN = "";
            CostCenter = "";
            OutPut = "0.0";
            Scrap = "0.0";
            Destination = "";
        }

        public string DataKey { set; get; }
        public string PN { set; get; }
        public string CostCenter { set; get; }
        public string OutPut { set; get; }
        public string Scrap { set; get; }
        public string Destination { set; get; }
    }
}