using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Web.Routing;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Web.UI.WebControls;

namespace Prism.Models
{
    public class EmailUtility
    {
        private static void logthdinfo(string info)
        {
            try
            {
                var filename = "d:\\log\\emailexception-" + DateTime.Now.ToString("yyyy-MM-dd");
                if (System.IO.File.Exists(filename))
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
            {
            }
        }

        public static bool IsEmaileValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool SendEmail(Controller ctrl, string title, List<string> tolist, string content, bool isHtml = true, string attachpath = null)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);

                var message = new MailMessage();
                if (!string.IsNullOrEmpty(attachpath))
                {
                    var atts = attachpath.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var att in atts)
                    {
                        if (System.IO.File.Exists(att))
                        {
                            var attach = new Attachment(att);
                            message.Attachments.Add(attach);
                        }
                    }
                }

                message.IsBodyHtml = isHtml;
                message.From = new MailAddress(syscfgdict["APPEMAILADRESS"]);
                foreach (var item in tolist)
                {
                    if (!item.Contains("@"))
                        continue;

                    try
                    {
                        if (item.Contains(";") || item.Contains("/"))
                        {
                            var ts = item.Split(new string[] { ";","/" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var t in ts)
                            {
                                if (IsEmaileValid(t))
                                {
                                    message.To.Add(t);
                                }
                            }
                        }
                        else
                        {
                            if (IsEmaileValid(item))
                            {
                                message.To.Add(item);
                            }
                        }
                    }
                    catch (Exception e) { logthdinfo("Address exception: " + e.Message); }
                }

                message.Subject = title;
                message.Body = content.Replace("\r\n", "<br>").Replace("\r", "<br>");

                SmtpClient client = new SmtpClient();
                client.Host = syscfgdict["EMAILSERVER"];

                if (syscfgdict["EMAILSSL"].Contains("TRUE"))
                { client.EnableSsl = true; }
                else
                { client.EnableSsl = false; }

                client.Timeout = 60000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(syscfgdict["APPEMAILADRESS"], syscfgdict["APPEMAILPWD"]);

                ServicePointManager.ServerCertificateValidationCallback
                    = delegate (object s, X509Certificate certificate, X509Chain chain
                    , SslPolicyErrors sslPolicyErrors) { return true; };

                new Thread(() => {
                    try
                    {
                        client.Send(message);
                    }
                    catch (SmtpFailedRecipientsException ex)
                    {
                        logthdinfo("SmtpFailedRecipientsException exception: " + ex.Message);
                        try
                        {
                            message.To.Clear();
                            foreach (var item in tolist)
                            {
                                if (ex.Message.Contains(item))
                                {
                                    try
                                    {
                                        message.To.Add(item);
                                    }
                                    catch (Exception e) { logthdinfo("Address exception2: " + e.Message); }
                                }
                            }
                            client.Send(message);
                        }
                        catch (Exception ex1)
                        {
                            logthdinfo("nest exception1: " + ex1.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        logthdinfo("Prism send exception: " + ex.Message);
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                logthdinfo("main exception: " + ex.Message);
                return false;
            }
            return true;
        }

        public static string CreateTableHtml(string greetig, string description, string comment, List<List<string>> table,List<List<string>> appendtable = null)
        {
            var idx = 0;
            var content = "<!DOCTYPE html>";
            content += "<html>";
            content += "<head>";
            content += "<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />";
            content += "<title></title>";
            content += "</head>";
            content += "<body>";
            content += "<div><p>" + greetig + ",</p></div>";
            content += "<div><p>" + description + ".</p></div>";
            if (!string.IsNullOrEmpty(comment))
            {
                content += "<div><p>" + comment + "</p>";
            }
            if (table != null && table.Count > 0)
            {
                content += "<div><br>";
                content += "<table border='1' cellpadding='0' cellspacing='0' width='100%'>";
                content += "<thead style='background-color: #006DC0; color: #fff;'>";
                foreach (var th in table[0])
                {
                    content += "<th>" + th + "</th>";
                }
                content += "</thead>";
                foreach (var tr in table)
                {
                    if (idx > 0)
                    {
                        content += "<tr>";
                        foreach (var td in tr)
                        {
                            content += "<td>" + td + "</td>";
                        }
                        content += "</tr>";
                    }
                    idx++;
                }
                content += "</table>";
                content += "</div>";
            }

            if (appendtable != null && appendtable.Count > 0)
            {
                content += "<div><br>";
                content += "<table border='1' cellpadding='0' cellspacing='0' width='100%'>";
                content += "<thead style='background-color: #006DC0; color: #fff;'>";
                foreach (var th in appendtable[0])
                {
                    content += "<th>" + th + "</th>";
                }
                content += "</thead>";
                foreach (var tr in appendtable)
                {
                    if (idx > 0)
                    {
                        content += "<tr>";
                        foreach (var td in tr)
                        {
                            content += "<td>" + td + "</td>";
                        }
                        content += "</tr>";
                    }
                    idx++;
                }
                content += "</table>";
                content += "</div>";
            }

            content += "<br><br>";
            content += "<div><p style='font-size: 12px; font-style: italic;'>This is a system generated message, please do not reply to this email.</p></div>";
            content += "</body>";
            content += "</html>";

            return content;
        }

        public static string CreateImgHtml(string greetig, string description, string comment, List<string> imgs)
        {
            var content = "<!DOCTYPE html>";
            content += "<html>";
            content += "<head>";
            content += "<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />";
            content += "<title></title>";
            content += "</head>";
            content += "<body>";
            content += "<div><p>" + greetig + ",</p></div>";
            content += "<div><p>" + description + ".</p></div>";
            if (!string.IsNullOrEmpty(comment))
            {
                content += "<div><p>" + comment + "</p>";
            }
            foreach (var item in imgs)
            {
                content += item;
            }
            content += "<br><br>";
            content += "<div><p style='font-size: 12px; font-style: italic;'>This is a system generated message, please do not reply to this email.</p></div>";
            content += "</body>";
            content += "</html>";

            return content;
        }

        public static string RetrieveCurrentMachineName()
        {
            var netcomputername = "wuxinpi.china.ads.finisar.com";
            //try { netcomputername = System.Net.Dns.GetHostName(); }
            //catch (Exception ex) { }
            return netcomputername;
        }
    }
}