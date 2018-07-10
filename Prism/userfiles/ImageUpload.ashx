<%@ WebHandler Language="C#" Class="ImageUpload" %>

using System;
using System.Web;
using System.IO;
using Prism.Models;

public class ImageUpload : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        try
        {
            HttpPostedFile uploads = context.Request.Files[0];
            string tempfn = System.IO.Path.GetFileName(uploads.FileName);

            string url = "";
            var bimage = "";

            var ext = Path.GetExtension(tempfn).ToLower();
            var allitype = ".jpg,.png,.gif,.jpeg";
            var allvtype = ".mp4,.mp3,.h264,.wmv,.wav,.avi,.flv,.mov,.mkv,.webm,.ogg,.mov,.mpg";

            if (allitype.Contains(ext))
            {
                bimage = "IMG";

                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = context.Server.MapPath(".") + "\\images\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var fn = Path.GetFileNameWithoutExtension(tempfn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(tempfn);
                fn = fn.Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                uploads.SaveAs(imgdir + fn);
                url = "/userfiles/images/" + datestring + "/" + fn;
            }
            else if (allvtype.Contains(ext))
            {
                bimage = "VIDEO";

                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = context.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var fn = Path.GetFileName(uploads.FileName)
                .Replace(" ", "_").Replace("#", "")
                .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");
                fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);

                var onlyname = Path.GetFileNameWithoutExtension(fn);

                var srcvfile = imgdir + fn;
                //store file to local
                uploads.SaveAs(srcvfile);

                //var imgname = onlyname + ".jpg";
                //var imgpath = imgdir + imgname;
                //var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                //ffMpeg.GetVideoThumbnail(srcvfile, imgpath);

                //var oggname = onlyname + ".ogg";
                //var oggpath = imgdir + oggname;
                //var ffMpeg1 = new NReco.VideoConverter.FFMpegConverter();
                //ffMpeg1.ConvertMedia(srcvfile, oggpath, NReco.VideoConverter.Format.ogg);

                if (!ext.Contains("mp4"))
                {
                    var mp4name = onlyname + ".mp4";
                    var mp4path = imgdir + mp4name;
                    var ffMpeg2 = new NReco.VideoConverter.FFMpegConverter();
                    ffMpeg2.ConvertMedia(srcvfile, mp4path, NReco.VideoConverter.Format.mp4);
                    try { System.IO.File.Delete(srcvfile); } catch (Exception ex) { }
                }

                url = "/userfiles/docs/" + datestring + "/" + onlyname;
            }
            else
            {
                bimage = "NORMAL";

                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = context.Server.MapPath(".") + "\\docs\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var fn = Path.GetFileNameWithoutExtension(tempfn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(tempfn);
                fn = fn.Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                uploads.SaveAs(imgdir + fn);
                url = "/userfiles/docs/" + datestring + "/" + fn;
                tempfn = fn;
            }

            if (string.Compare(bimage,"IMG")== 0)
            {
                context.Response.Write("<p><img src='" + url + "'/></p>");
            }
            else if (string.Compare(bimage,"VIDEO")== 0)
            {
                context.Response.Write("<p>"
                +"<video width='640' height='480' controls src='"+url+".mp4' type='video/mp4'>"
                +"Your browser does not support the video tag."
                + "</video></p>");
            }
            else
            {
                context.Response.Write("<p><a href='" + url + "' target='_blank'>"+tempfn+"</a></p>");
            }

            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {
            try
            {

                string url = "Failed to upload file for: " + ex.ToString();
                context.Response.Write(url);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex1)
            { }
        }
    }

    public bool IsReusable {

        get {
            return false;
        }

    }



}
