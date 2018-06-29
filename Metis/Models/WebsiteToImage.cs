using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Metis.Models
{
    public class WebsiteToImage
    {
        private Bitmap m_Bitmap;
        private string m_Url;
        private string m_FileName = string.Empty;

        public WebsiteToImage(string url)
        {
            // Without file 
            m_Url = url;
        }

        public WebsiteToImage(string url, string fileName)
        {
            // With file 
            m_Url = url;
            m_FileName = fileName;
        }

        public Bitmap Generate()
        {
            // Thread 
            var m_thread = new Thread(_Generate);
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
            m_thread.Join();
            return m_Bitmap;
        }

        private void _Generate()
        {
            var browser = new WebBrowser { ScrollBarsEnabled = false };

            browser.ClientSize = new Size(1200, 1900);

            browser.Navigate(m_Url);

            //browser.DocumentCompleted += WebBrowser_DocumentCompleted;

            while (browser.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

            var updateElement = browser.Document.GetElementById("loadcomplete");
            var startkick = DateTime.Now;
            if (updateElement != null)
            {
                while (browser.Document.GetElementById("loadcomplete").InnerHtml.Equals("FALSE"))
                {
                    Application.DoEvents();
                    var endkick = DateTime.Now;
                    if ((endkick - startkick).TotalSeconds > 20)
                    {
                        break;
                    }
                }

                while (true)
                {
                    Application.DoEvents();
                    var endkick = DateTime.Now;
                    if ((endkick - startkick).TotalSeconds > 3)
                    {
                        break;
                    }
                }
            }
            else
            {
                while (true)
                {
                    Application.DoEvents();
                    var endkick = DateTime.Now;
                    if ((endkick - startkick).TotalSeconds > 15)
                    {
                        break;
                    }
                }
            }

            DrawWebPage(browser);

            browser.Dispose();
        }

        private void DrawWebPage(WebBrowser browser)
        {
            //browser.ClientSize = new Size(browser.Document.Body.ScrollRectangle.Width, browser.Document.Body.ScrollRectangle.Bottom);
            browser.ClientSize = new Size(1200, browser.Document.Body.ScrollRectangle.Bottom);
            browser.ScrollBarsEnabled = false;
            //m_Bitmap = new Bitmap(browser.Document.Body.ScrollRectangle.Width, browser.Document.Body.ScrollRectangle.Bottom);
            m_Bitmap = new Bitmap(1200, browser.Document.Body.ScrollRectangle.Bottom);
            browser.BringToFront();

            browser.DrawToBitmap(m_Bitmap, browser.Bounds);

            // Save as file? 
            if (m_FileName.Length > 0)
            {
                // Save 
                m_Bitmap.SaveJPG100(m_FileName);
            }
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Capture 
            var browser = (WebBrowser)sender;
            //browser.ClientSize = new Size(browser.Document.Body.ScrollRectangle.Width, browser.Document.Body.ScrollRectangle.Bottom);
            browser.ClientSize = new Size(1920, 1080);
            browser.ScrollBarsEnabled = false;
            //m_Bitmap = new Bitmap(browser.Document.Body.ScrollRectangle.Width, browser.Document.Body.ScrollRectangle.Bottom);
            m_Bitmap = new Bitmap(1920, 1080);
            browser.BringToFront();

            browser.DrawToBitmap(m_Bitmap, browser.Bounds);

            // Save as file? 
            if (m_FileName.Length > 0)
            {
                // Save 
                m_Bitmap.SaveJPG100(m_FileName);
            }
        }
    }

    public static class BitmapExtensions
    {
        public static void SaveJPG100(this Bitmap bmp, string filename)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            bmp.Save(filename, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static void SaveJPG100(this Bitmap bmp, Stream stream)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            bmp.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();

            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            // Return 
            return null;
        }
    }
}