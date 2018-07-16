using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Documents;

namespace Prism.Models
{
    public class ExcelReader
    {
        private static Excel.Workbook OpenBook(Excel.Workbooks books, string fileName, bool readOnly, bool editable,
bool updateLinks)
        {

            Excel.Workbook book = books.Open(
                fileName, updateLinks, readOnly,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, editable, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);
            return book;
        }

        private static void ReleaseRCM(object o)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
            }
            catch
            {
            }
            finally
            {
                o = null;
            }
        }

        private static bool WholeLineEmpty(List<string> line)
        {
            bool ret = true;
            foreach (var item in line)
            {
                if (!string.IsNullOrEmpty(item.Trim()))
                {
                    ret = false;
                }
            }
            return ret;
        }

        private static void logthdinfo(string info)
        {
            var filename = "d:\\log\\excelexception-" + DateTime.Now.ToString("yyyy-MM-dd");
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

        private static string[] columnFlag = {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
              "AA","AB","AC","AD","AE","AF","AG","AH","AI","AJ","AK","AL","AM","AN","AO","AP","AQ","AR","AS","AT","AU","AV","AW","AX","AY","AZ",
              "BA","BB","BC","BD","BE","BF","BG","BH","BI","BJ","BK","BL","BM","BN","BO","BP","BQ","BR","BS","BT","BU","BV","BW","BX","BY","BZ",
              "CA","CB","CC","CD","CE","CF","CG","CH","CI","CJ","CK","CL","CM","CN","CO","CP","CQ","CR","CS","CT","CU","CV","CW","CX","CY","CZ",
              "DA","DB","DC","DD","DE","DF","DG","DH","DI","DJ","DK","DL","DM","DN","DO","DP","DQ","DR","DS","DT","DU","DV","DW","DX","DY","DZ",
              "EA","EB","EC","ED","EE","EF","EG","EH","EI","EJ","EK","EL","EM","EN","EO","EP","EQ","ER","ES","ET","EU","EV","EW","EX","EY","EZ",
              "FA","FB","FC","FD","FE","FF","FG","FH","FI","FJ","FK","FL","FM","FN","FO","FP","FQ","FR","FS","FT","FU","FV","FW","FX","FY","FZ",
              "GA","GB","GC","GD","GE","GF","GG","GH","GI","GJ","GK","GL","GM","GN","GO","GP","GQ","GR","GS","GT","GU","GV","GW","GX","GY","GZ",
              "HA","HB","HC","HD","HE","HF","HG","HH","HI","HJ","HK","HL","HM","HN","HO","HP","HQ","HR","HS","HT","HU","HV","HW","HX","HY","HZ",
              "IA","IB","IC","ID","IE","IF","IG","IH","II","IJ","IK","IL","IM","IN","IO","IP","IQ","IR","IS","IT","IU","IV","IW","IX","IY","IZ",
              "JA","JB","JC","JD","JE","JF","JG","JH","JI","JJ","JK","JL","JM","JN","JO","JP","JQ","JR","JS","JT","JU","JV","JW","JX","JY","JZ"};

        private static List<List<string>> RetrieveDataFromExcel2(Excel.Worksheet sheet, int columns = 101, bool getlink = false)
        {
            var ret = new List<List<string>>();
            var totalrow = 100000;

            int emptycount = 0;

            for (var rowidx = 1; rowidx < totalrow; rowidx++)
            {
                var newline = new List<string>();
                try
                {
                    var range = sheet.get_Range(columnFlag[0] + rowidx.ToString(), columnFlag[99] + rowidx.ToString());
                    var saRet = (System.Object[,])range.get_Value(Type.Missing);

                    for (var colidx = 1; colidx < columns; colidx++)
                    {
                        if (saRet[1, colidx] != null)
                        {
                            newline.Add(saRet[1, colidx].ToString().Replace("'", "").Replace("\"", "").Trim());
                        }
                        else
                        {
                            newline.Add("");
                        }
                    }


                    if (getlink)
                    {
                        var linkrange = range.Cells;

                        foreach (Excel.Range c in linkrange)
                        {
                            if (c.Hyperlinks.Count > 0)
                            {
                                newline.Add(c.Hyperlinks[1].SubAddress);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    newline.Clear();
                }

                if (!WholeLineEmpty(newline))
                {
                    emptycount = 0;
                    ret.Add(newline);
                }
                else
                {
                    emptycount = emptycount + 1;
                }


                if (emptycount > 20 || rowidx > 500000)
                {
                    break;
                }

                if (rowidx == totalrow - 1)
                {
                    totalrow = totalrow + 100000;
                }
            }

            return ret;
        }

        public static List<List<string>> RetrieveDataFromExcel(string wholefn, string sheetname, int columns = 101,bool getlink=false)
        {
            var data = new List<List<string>>();

            Excel.Application excel = null;
            Excel.Workbook wkb = null;
            Excel.Workbooks books = null;
            Excel.Worksheet sheet = null;

            try
            {
                excel = new Excel.Application();
                excel.DisplayAlerts = false;
                books = excel.Workbooks;
                wkb = OpenBook(books, wholefn, true, false, false);

                if (string.IsNullOrEmpty(sheetname))
                {
                    sheet = wkb.Sheets[1] as Excel.Worksheet;
                }
                else
                {
                    sheet = wkb.Sheets[sheetname] as Excel.Worksheet;
                }

                var ret = RetrieveDataFromExcel2(sheet, columns,getlink);

                wkb.Close();
                excel.Quit();

                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(wkb);
                Marshal.ReleaseComObject(books);
                Marshal.ReleaseComObject(excel);

                if (wkb != null)
                    ReleaseRCM(wkb);
                if (excel != null)
                    ReleaseRCM(excel);

                sheet = null;
                wkb = null;
                books = null;
                excel = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return ret;

            }
            catch (Exception ex)
            {
                logthdinfo(DateTime.Now.ToString() + " Exception on " + wholefn + " :" + ex.Message + "\r\n\r\n");

                if (sheet != null)
                    Marshal.ReleaseComObject(sheet);
                if (wkb != null)
                    Marshal.ReleaseComObject(wkb);
                if (books != null)
                    Marshal.ReleaseComObject(books);
                if (excel != null)
                    Marshal.ReleaseComObject(excel);

                if (wkb != null)
                    ReleaseRCM(wkb);
                if (excel != null)
                    ReleaseRCM(excel);

                sheet = null;
                wkb = null;
                books = null;
                excel = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return data;
            }

        }

        public static List<List<string>> RetrieveDataFromExcel_bak(string wholefn, string sheetname)
        {
            var data = new List<List<string>>();
            Excel.Application excel = null;
            Excel.Workbook wkb = null;
            Excel.Workbooks books = null;
            Excel.Worksheet sheet = null;

            try
            {
                excel = new Excel.Application();
                excel.DisplayAlerts = false;
                books = excel.Workbooks;
                wkb = OpenBook(books, wholefn, true, false, false);

                if (string.IsNullOrEmpty(sheetname))
                {
                    sheet = wkb.Sheets[1] as Excel.Worksheet;
                }
                else
                {
                    sheet = wkb.Sheets[sheetname] as Excel.Worksheet;
                }


                var excelRange = sheet.UsedRange;
                object[,] valueArray = (object[,])excelRange.get_Value(
                Excel.XlRangeValueDataType.xlRangeValueDefault);
                var totalrows = sheet.UsedRange.Rows.Count;
                var totalcols = sheet.UsedRange.Columns.Count;

                for (int row = 1; row <= totalrows; ++row)
                {
                    var line = new List<string>();
                    for (int col = 1; col <= totalcols; ++col)
                    {
                        if (valueArray[row, col] == null)
                        {
                            line.Add(string.Empty);
                        }
                        else
                        {
                            line.Add(valueArray[row, col].ToString().Trim().Replace("'", "").Replace("\"", ""));
                        }
                    }
                    //if (!WholeLineEmpty(line))
                    //{
                    data.Add(line);
                    //}
                }


                wkb.Close();
                excel.Quit();

                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(wkb);
                Marshal.ReleaseComObject(books);
                Marshal.ReleaseComObject(excel);

                if (wkb != null)
                    ReleaseRCM(wkb);
                if (excel != null)
                    ReleaseRCM(excel);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return data;
            }
            catch (Exception ex)
            {
                logthdinfo(DateTime.Now.ToString() + " Exception on " + wholefn + " :" + ex.Message + "\r\n\r\n");

                if (ex.Message.Contains("DISP_E_OVERFLOW") && sheet != null)
                {
                    var ret = RetrieveDataFromExcel2(sheet);

                    if (sheet != null)
                        Marshal.ReleaseComObject(sheet);
                    if (wkb != null)
                        Marshal.ReleaseComObject(wkb);
                    if (books != null)
                        Marshal.ReleaseComObject(books);
                    if (excel != null)
                        Marshal.ReleaseComObject(excel);

                    if (wkb != null)
                        ReleaseRCM(wkb);
                    if (excel != null)
                        ReleaseRCM(excel);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    return ret;
                }
                else
                {
                    data.Clear();

                    if (sheet != null)
                        Marshal.ReleaseComObject(sheet);
                    if (wkb != null)
                        Marshal.ReleaseComObject(wkb);
                    if (books != null)
                        Marshal.ReleaseComObject(books);
                    if (excel != null)
                        Marshal.ReleaseComObject(excel);

                    if (wkb != null)
                        ReleaseRCM(wkb);
                    if (excel != null)
                        ReleaseRCM(excel);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    return data;
                }
            }

        }
    }
}