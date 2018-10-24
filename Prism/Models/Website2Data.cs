using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Drawing;
using System.Net;

namespace Prism.Models
{
    public class Website2Data
    {

        public Website2Data(string url,string tabxpath)
        {
            WebUrl = url;
            TableXPath = tabxpath;
            TableData = new List<List<string>>();
        }

        public void GetData()
        {
            // Thread 
            var m_thread = new Thread(_Generate);
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
            m_thread.Join();
        }

        private void _Generate()
        {
            //var browser = new WebBrowser { ScrollBarsEnabled = false };
            //browser.ClientSize = new Size(1200, 1900);
            //browser.Navigate(WebUrl);
            //while (browser.ReadyState != WebBrowserReadyState.Complete)
            //{
            //    Application.DoEvents();
            //}
            //Html2Data(browser.DocumentText);

            var client = new RestSharp.RestClient(WebUrl);
            //var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            //request.AddParameter("b64", "true");
            //request.AddParameter("type", "png");
            //request.AddParameter("width", "800");
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                var responseString = response.Content;
                Html2Data(responseString);
            }
            client.ClearHandlers();
        }

        private void Html2Data(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var table = doc.DocumentNode.SelectSingleNode(TableXPath);
            if (table != null)
            {
                TableData =  table.Descendants("tr")
                            .Skip(2)
                            .Select(tr => tr.Descendants("th")
                                            .Select(td => WebUtility.HtmlDecode(td.InnerText))
                                            .ToList())
                            .ToList();
            }
        }

        public string WebUrl { set; get; }
        public string TableXPath { set; get; }

        public List<List<string>> TableData { set; get; }

    }
}