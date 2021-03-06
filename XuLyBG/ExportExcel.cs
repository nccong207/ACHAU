using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Data;

namespace XuLyBG
{
    public class ExportExcel
    {
        private string _tmpFile = string.Empty;
        private string _fileName = string.Empty;
        private System.Data.DataTable _dtData;
        private bool _hasFilter = false;

        public ExportExcel(string tmpFile, string fileName, System.Data.DataTable dtData)
        {
            _fileName = fileName;
            _tmpFile = tmpFile;
            _dtData = dtData;
        }
        public bool Export()
        {
            Application app = new ApplicationClass();
            app.Visible = true;
            Workbook wb = app.Workbooks.Open(_tmpFile, Type.Missing, true, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            wb.SaveAs(_fileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            wb.Close(false, _tmpFile, false);
            wb = app.Workbooks.Open(_fileName, Type.Missing, false, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            try
            {
                Worksheet ws = (Worksheet)wb.Sheets[1];
                for (int i = 0; i < _dtData.Rows.Count; i++)
                {
                    //ws.Cells[i + 2, 1] = i + 1;
                    for (int j = 0; j < _dtData.Columns.Count; j++)
                    {
                        ws.Cells[i + 2, j + 1] = _dtData.Rows[i][j];
                    }
                }
            }
            catch {
            }
            finally
            {
                try
                {
                    app.Visible = false;
                    wb.Close(true, _fileName, false);
                    app.Quit();
                }
                catch {
                }
                finally
                {
                    app.Visible = false;
                    app.Quit();
                }
            }
            return true;
        }
    }
}
