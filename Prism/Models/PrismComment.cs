using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prism.Models
{
    public class PRISMCOMMENTTP
    {
        public static string SCRAP = "SCRAP";
        public static string HPU = "HPU";
    }

    public class SeverHtmlDecode
    {
        public static string Decode(Controller ctrl, string src)
        {
            var ret = ctrl.Server.HtmlDecode(src).Replace("border=\"0\"", "border=\"2\"");
            ret = System.Text.RegularExpressions.Regex.Replace(ret, "<div.*?>", string.Empty).Trim();
            ret = ret.Replace("</div>", "");
            return ret;
        }
    }

    public class PrismComment
    {
        public string CommentID { set; get; }

        private string sComment = "";
        public string Comment
        {
            set { sComment = value; }
            get { return sComment; }
        }

        public string CommentType { set; get; }

        public string dbComment
        {
            get
            {
                if (string.IsNullOrEmpty(sComment))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sComment));
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sComment = "";
                }
                else
                {
                    try
                    {
                        sComment = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sComment = "";
                    }

                }

            }
        }

        public string Reporter { set; get; }

        public DateTime CommentDate { set; get; }


        public static void StoreComment(string id, string desc, string rept, string cmtype)
        {
            var tempcom = new PrismComment();
            tempcom.Comment = ResizeImageFromHtml(desc);

            var sql = "insert into PrismComment(CommentID,Comment,Reporter,CommentDate,CommentType) values('<CommentID>','<Comment>','<Reporter>','<CommentDate>','<CommentType>')";
            sql = sql.Replace("<CommentID>", id).Replace("<Comment>", tempcom.dbComment).Replace("<Reporter>", rept)
                .Replace("<CommentDate>", DateTime.Now.ToString()).Replace("<CommentType>", cmtype);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<PrismComment> RetrieveComment(string id)
        {
            var ret = new List<PrismComment>();
            var csql = "select CommentID,Comment,Reporter,CommentDate,CommentType from PrismComment where CommentID = '<CommentID>' ";
            csql = csql.Replace("<CommentID>", id);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql, null);
            foreach (var r in cdbret)
            {
                var tempcomment = new PrismComment();
                tempcomment.CommentID = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                tempcomment.CommentType = Convert.ToString(r[4]);
                ret.Add(tempcomment);
            }
            return ret;
        }

        public static void UpdateComment(string id, string comment, string updater)
        {
            var tempcom = new PrismComment();
            tempcom.Comment = ResizeImageFromHtml(comment);

            var csql = "update PrismComment set Comment = '<Comment>',Reporter='<Reporter>',CommentDate = '<CommentDate>' where CommentID = '<CommentID>'";
            csql = csql.Replace("<Comment>", tempcom.dbComment).Replace("<Reporter>", updater).Replace("<CommentID>", id).Replace("<CommentDate>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(csql);
        }


        private static string resizeimg(string imgstr)
        {
            var srcidx = imgstr.IndexOf("src=\"");
            var srceidx = imgstr.IndexOf("\"", srcidx + 6);
            var srcstr = imgstr.Substring(srcidx, (srceidx + 1 - srcidx));
            return "<div style=\"text-align: center;\">" + "<img " + srcstr + " style=\"max-width: 90%; height: auto;\" /></div>";
        }

        private static string ResizeImageFromHtml(string src)
        {
            var startidx = 0;
            while (src.IndexOf("<img", startidx) != -1)
            {
                var imgsidx = src.IndexOf("<img", startidx);
                var imgeidx = src.IndexOf(">", imgsidx);
                if (imgeidx != -1)
                {
                    startidx = imgeidx;
                    imgeidx = imgeidx + 1;
                    var imgstr = src.Substring(imgsidx, (imgeidx - imgsidx));
                    var nimgstr = resizeimg(imgstr);
                    src = src.Remove(imgsidx, imgeidx - imgsidx).Insert(imgsidx, nimgstr);
                }
                else
                {
                    startidx = imgsidx + 3;
                }
            }
            return src.Replace("</img>", "");
        }



    }
}