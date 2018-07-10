using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using Oracle.DataAccess.Client;
using System.Web.Caching;
using System.Web.Mvc;
using System.Text;

namespace Metis.Models
{
    public class DBUtility
    {
        private static void logthdinfo(string info)
        {
            try
            {
                var filename = "d:\\log\\sqlexception-" + DateTime.Now.ToString("yyyy-MM-dd");
                if (File.Exists(filename))
                {
                    var content = System.IO.File.ReadAllText(filename);
                    content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
                    System.IO.File.WriteAllText(filename, content);
                }
                else
                {
                    System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
                }
            }
            catch (Exception ex)
            { }

        }

        public static bool IsDebug()
        {
            bool debugging = false;
#if DEBUG
            debugging = true;
#else
            debugging = false;
#endif
            return debugging;
        }

        public static string ConstructCond(List<string> condlist)
        {
            StringBuilder sb1 = new StringBuilder(10 * (condlist.Count + 5));
            sb1.Append("('");
            foreach (var line in condlist)
            {
                sb1.Append(line + "','");
            }
            var tempstr1 = sb1.ToString();
            return tempstr1.Substring(0, tempstr1.Length - 2) + ")";
        }

        public static SqlConnection GetLocalConnector()
        {
            var conn = new SqlConnection();
            try
            {
                //conn.ConnectionString = "Data Source = (LocalDb)\\MSSQLLocalDB; AttachDbFilename = ~\\App_Data\\Prometheus.mdf; Integrated Security = True";
                if (IsDebug())
                {
                    conn.ConnectionString = "Server=wuxinpi;User ID=BSApp;Password=magic@123;Database=BSSupport;Connection Timeout=120;";
                }
                else
                {
                    conn.ConnectionString = "Server=wuxinpi;User ID=BSApp;Password=magic@123;Database=BSSupport;Connection Timeout=120;";
                }

                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                logthdinfo("open connect exception: " + ex.Message + "\r\n");
                CloseConnector(conn);
                return null;
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                logthdinfo("open connect exception: " + ex.Message + "\r\n");
                CloseConnector(conn);
                return null;
            }
        }

        //"Server=CN-CSSQL;uid=SHG_Read;pwd=shgread;Database=InsiteDB;Connection Timeout=30;"
        public static SqlConnection GetConnector(string constr)
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = constr;
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static bool ExeSqlNoRes(SqlConnection conn, string sql)
        {
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeSqlWithRes(SqlConnection conn, string sql, Dictionary<string, string> parameters = null)
        {

            var ret = new List<List<object>>();
            if (conn == null)
                return ret;

            SqlDataReader sqlreader = null;
            SqlCommand command = null;

            try
            {
                command = conn.CreateCommand();
                command.CommandText = sql;
                command.CommandTimeout = 120;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }

                }
                sqlreader.Close();
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }
                ret.Clear();
                return ret;
            }
        }

        public static void CloseConnector(SqlConnection conn)
        {
            if (conn == null)
                return;

            try
            {
                conn.Dispose();
            }
            catch (SqlException ex)
            {
                logthdinfo("close conn exception: " + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                logthdinfo("close conn exception: " + ex.Message + "\r\n");
            }
        }
        /* parameters: 
        * if you want to defense SQL injection,
        * you can prepare @param in sql,
        * and give @param-values in parameters.
        */
        public static bool ExeLocalSqlNoRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var now = DateTime.Now;
            //var msec1 = now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond;

            var conn = GetLocalConnector();
            if (conn == null)
                return false;
            SqlCommand command = null;

            try
            {
                command = conn.CreateCommand();
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                command.ExecuteNonQuery();
                CloseConnector(conn);

                //now = DateTime.Now;
                //var msec2 = now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond;
                //if ((msec2 - msec1) > 40)
                //{
                //    logthdinfo("no res sql: " + sql);
                //    logthdinfo("no res query end: " + " spend " + (msec2 - msec1).ToString());
                //}

                return true;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                try
                {
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e) { }
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                try
                {
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e) { }
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }


        //public static DataTable ExecuteLocalQueryReturnTable(string sql)
        //{
        //    var conn = GetLocalConnector();
        //    try
        //    {
        //        var dt = new DataTable();
        //        SqlDataAdapter myAd = new SqlDataAdapter(sql, conn);
        //        myAd.SelectCommand.CommandTimeout = 0;
        //        myAd.Fill(dt);
        //        return dt;
        //    }
        //    catch (SqlException ex)
        //    {
        //        CloseConnector(conn);
        //        //System.Windows.MessageBox.Show(ex.ToString());
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        CloseConnector(conn);
        //        //System.Windows.MessageBox.Show(ex.ToString());
        //        return null;
        //    }
        //}

        public static DataTable ExecuteSqlReturnTable(SqlConnection conn, string sql)
        {
            try
            {
                var dt = new DataTable();
                SqlDataAdapter myAd = new SqlDataAdapter(sql, conn);
                myAd.SelectCommand.CommandTimeout = 0;
                myAd.Fill(dt);
                return dt;
            }
            catch (SqlException ex)
            {
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }
        public static DataTable NExecuteSqlReturnTable(string sql, Dictionary<string, string> parameters = null)
        {
            var dt = new DataTable();
            var conn = GetLocalConnector();
            if (conn == null)
                return dt;
            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        cmd.Parameters.Add(parameter);
                    }
                }
                SqlDataAdapter myAd = new SqlDataAdapter(cmd);
                myAd.SelectCommand.CommandTimeout = 0;
                myAd.Fill(dt);
                return dt;
            }
            catch (SqlException ex)
            {
                CloseConnector(conn);
                return dt;
            }
            catch (Exception ex)
            {
                CloseConnector(conn);
                return dt;
            }
        }
        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeLocalSqlWithRes(string sql, Cache mycache, Dictionary<string, string> parameters = null)
        {
            //var now = DateTime.Now;
            //var msec1 = now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond;

            if (mycache != null)
            {
                var cret = (List<List<object>>)mycache.Get(sql);
                if (cret != null)
                {
                    return cret;
                }
            }

            var ret = new List<List<object>>();
            var conn = GetLocalConnector();
            if (conn == null)
                return ret;
            SqlDataReader sqlreader = null;
            SqlCommand command = null;

            try
            {
                command = conn.CreateCommand();
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {
                    while (sqlreader.Read())
                    {
                        Object[] values = new Object[sqlreader.FieldCount];
                        sqlreader.GetValues(values);
                        ret.Add(values.ToList<object>());
                    }

                }

                sqlreader.Close();
                CloseConnector(conn);

                //now = DateTime.Now;
                //var msec2 = now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond;
                //if ((msec2 - msec1) > 40)
                //{
                //    logthdinfo("res sql: " + sql);
                //    logthdinfo("res query end: count " + ret.Count.ToString() + " spend " + (msec2 - msec1).ToString());
                //}

                if (mycache != null)
                {
                    mycache.Insert(sql, ret, null, DateTime.Now.AddHours(1), Cache.NoSlidingExpiration);
                }

                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }

                CloseConnector(conn);

                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }

                CloseConnector(conn);

                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetNebulaConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wuxinpi;User ID=NebulaNPI;Password=abc@123;Database=NebulaTrace;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the mes report pdms database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the mes report pdms database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeNebulaSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetNebulaConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }



        private static SqlConnection GetMESConnector()
        {
            var conn = new SqlConnection();
            try
            {
                //conn.ConnectionString = "Server=CN-CSSQL;uid=SHG_Read;pwd=shgread;Database=InsiteDB;Connection Timeout=30;";
                conn.ConnectionString = "Server=wux-csods;uid=NPI_FA;pwd=msW2TH95Pd;Database=InsiteDB;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static bool ExeMESSqlNoRes(string sql)
        {
            var conn = GetMESConnector();
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
                CloseConnector(conn);
                return true;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }
        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeMESSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetMESConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 120;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetRealMESConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=CN-CSSQL;uid=SHG_Read;pwd=shgread;Database=InsiteDB;Connection Timeout=30;";
                //conn.ConnectionString = "Server=wux-csods;uid=NPI_FA;pwd=msW2TH95Pd;Database=InsiteDB;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeRealMESSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetRealMESConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 120;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetPRLConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=cn-csrpt;uid=Active_NPI;pwd=Active@NPI;Database=SummaryDB;Connection Timeout=30;";
                //conn.ConnectionString = "Data Source=cn-csrpt;Initial Catalog=SummaryDB;Integrated Security=True;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the parallel summary database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the parallel summary database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static bool ExePRLSqlNoRes(Controller ctrl, string sql)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var conn = GetPRLConnector();
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
                CloseConnector(conn);
                return true;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }

        }
        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExePRLSqlWithRes(Controller ctrl, string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetPRLConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }


        private static SqlConnection GetMESBackupConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=cn-csrpt;uid=Active_NPI;pwd=Active@NPI;Database=InsiteDB;Connection Timeout=30;";
                //conn.ConnectionString = "Data Source=cn-csrpt;Initial Catalog=SummaryDB;Integrated Security=True;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the parallel InsiteDB database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the parallel InsiteDB database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }
        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeMESBackupSqlWithRes(Controller ctrl, string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetMESBackupConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 120;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetMESReportConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wux-prod02\mes_report;uid=ParaRead;pwd=Para@123;Database=PDMS;Connection Timeout=30;";
                //conn.ConnectionString = "Data Source=cn-csrpt;Initial Catalog=SummaryDB;Integrated Security=True;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the mes report pdms database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the mes report pdms database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeMESReportSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetMESReportConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }



        private static SqlConnection GetMESReportMasterConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wux-prod02\mes_report;uid=Active_NPI;pwd=Active@123;Connection Timeout=30;";
                //conn.ConnectionString = @"Server=wux-prod02\mes_report;uid=Active_NPI;pwd=Active@123;Database=PDMSMaster;Connection Timeout=30;";
                //conn.ConnectionString = "Data Source=cn-csrpt;Initial Catalog=SummaryDB;Integrated Security=True;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the mes report pdms database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the mes report pdms database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeMESReportMasterSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetMESReportMasterConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 120;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetAutoConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wux-prod04\prod01;uid=parallel_read;pwd=abc@1234;Database=Parallel;Connection Timeout=30;";
                //conn.ConnectionString = "Data Source=cn-csrpt;Initial Catalog=SummaryDB;Integrated Security=True;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the auto  database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the auto  database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }
        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeAutoSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetAutoConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 120;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }


        private static SqlConnection GetFAIConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wux-prod03\prod01;uid=OA_FAI_Reader;pwd=123_FAI_#;Database=eDMR;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the FAI summary database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the FAI summary database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }
        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeFAISqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetFAIConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        public static List<List<object>> ExeATESqlWithRes(string sql)
        {

            var ret = new List<List<object>>();

            OracleConnection Oracleconn = null;
            try
            {
                var ConnectionStr = "User Id=extviewer;Password=extviewer;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=shg-oracle)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ateshg)));";

                Oracleconn = new OracleConnection(ConnectionStr);
                try
                {
                    if (Oracleconn.State == ConnectionState.Closed)
                    {
                        Oracleconn.Open();
                    }
                    else if (Oracleconn.State == ConnectionState.Broken)
                    {
                        Oracleconn.Close();
                        Oracleconn.Open();
                    }
                }
                catch (Exception e)
                {
                    //System.Windows.MessageBox.Show(e.Message);
                }

                OracleCommand cmd = new OracleCommand(sql, Oracleconn);
                cmd.CommandType = CommandType.Text;
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    var line = new List<object>();
                    for (int idx = 0; idx < dr.FieldCount; idx++)
                    {
                        line.Add(dr[idx]);
                    }
                    ret.Add(line);
                }

                Oracleconn.Close();
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                //System.Windows.MessageBox.Show(ex.Message);

                try
                {
                    if (Oracleconn != null)
                    {
                        Oracleconn.Close();
                    }
                }
                catch (Exception ex1) { }

            }
            return ret;

        }

    }
}