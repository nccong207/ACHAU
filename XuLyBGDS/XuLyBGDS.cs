using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.Drawing;
using System.Data;

namespace XuLyBGDS
{
    public class XuLyBGDS : ICControl
    {
        List<string> lstTB = new List<string>(new string[] { "MTBaoGia", "MTDonHang", "MTLSX", "MTKH" });
        List<string> lstPk = new List<string>(new string[] { "MTBGID", "MTDHID", "MTLSXID", "MTKHID" });
        string tableName;
        GridView gvMain;
        private DataCustomFormControl _data;
        private InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetail);
        #region ICControl Members

        public void AddEvent()
        {
            tableName = _data.DrTableMaster["TableName"].ToString();
            if (!lstTB.Contains(tableName))
                return;
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;

            if (tableName == "MTLSX" && gvMain.Columns.ColumnByFieldName("Stt") != null)
                gvMain.Columns["Stt"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;

            _data.FrmMain.Load += new EventHandler(FrmMain_Load);
            //gvMain.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gvMain_FocusedRowChanged);

            gvMain.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(gvMain_CustomDrawCell);

        }

        void gvMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if ((tableName == "MTDonHang" || tableName == "MTLSX") && e.Column.FieldName.Equals("NgayGH") && e.CellValue != null && e.CellValue != DBNull.Value)
            {
                object o = gvMain.GetRowCellValue(e.RowHandle, "DaSX");
                if (o == null || o == DBNull.Value || e.ToString() == "" || Boolean.Parse(o.ToString()))
                    return;
                TimeSpan time = Convert.ToDateTime(e.CellValue) - DateTime.Today;
                if (time.Days <= 2 && time.Days >= 0)
                {
                    e.Appearance.BackColor = Color.Red;
                }
            }
        }

        //void gvMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        //{
        //    if (gvMain.IsFocusedView || e.FocusedRowHandle <= 0 || e.PrevFocusedRowHandle < 0
        //        || e.FocusedRowHandle == gvMain.DataRowCount - 1 || e.PrevFocusedRowHandle == gvMain.DataRowCount)
        //        return;
        //    DataRow drvPre = gvMain.GetDataRow(e.PrevFocusedRowHandle);
        //    DataRow drv = gvMain.GetDataRow(e.FocusedRowHandle);
        //    string pk = lstPk[lstTB.IndexOf(tableName)];
        //    if (drvPre[pk].ToString() == drv[pk].ToString())
        //        if (e.FocusedRowHandle > e.PrevFocusedRowHandle)
        //            gvMain.MoveNext();
        //        else
        //            gvMain.MovePrev();
        //}

        void FrmMain_Load(object sender, EventArgs e)
        {

            if ((_data.BsMain.DataSource as DataSet).Tables[0].Columns.Contains("Duyet"))
            {
                //dung class StyleFormatCondition cho Duyet
                StyleFormatCondition d = new StyleFormatCondition();
                gvMain.FormatConditions.Add(d);
                //thiet lap dieu kien
                d.Column = gvMain.Columns["Duyet"];
                d.Condition = FormatConditionEnum.Equal;
                d.Value1 = true;
                //thiet lap dinh dang
                d.Appearance.BackColor = Color.LightGreen;
                d.ApplyToRow = true;
            }

            if ((_data.BsMain.DataSource as DataSet).Tables[0].Columns.Contains("HetHan"))
            {
                StyleFormatCondition hh = new StyleFormatCondition();
                gvMain.FormatConditions.Add(hh);
                hh.Column = gvMain.Columns["HetHan"];
                hh.Condition = FormatConditionEnum.Equal;
                hh.Value1 = true;
                hh.Appearance.BackColor = Color.Gainsboro;
                hh.ApplyToRow = true;
            }

            if ((_data.BsMain.DataSource as DataSet).Tables[0].Columns.Contains("Huy"))
            {
                StyleFormatCondition h = new StyleFormatCondition();
                gvMain.FormatConditions.Add(h);
                h.Column = gvMain.Columns["Huy"];
                h.Condition = FormatConditionEnum.Equal;
                h.Value1 = true;
                h.Appearance.BackColor = Color.Gainsboro;
                h.ApplyToRow = true;
            }

            if ((_data.BsMain.DataSource as DataSet).Tables[0].Columns.Contains("LSX"))
            {
                StyleFormatCondition h = new StyleFormatCondition();
                gvMain.FormatConditions.Add(h);
                h.Column = gvMain.Columns["LSX"];
                h.Condition = FormatConditionEnum.NotEqual;
                h.Value1 = null;
                h.Appearance.BackColor = Color.Yellow;
                h.ApplyToRow = true;
            }

            if ((_data.BsMain.DataSource as DataSet).Tables[0].Columns.Contains("TinhTrangNP"))
            {
                StyleFormatCondition h1 = new StyleFormatCondition();
                gvMain.FormatConditions.Add(h1);
                h1.Column = gvMain.Columns["TinhTrangNP"];
                h1.Condition = FormatConditionEnum.Equal;
                h1.Value1 = "Chưa đủ";
                h1.Appearance.BackColor = Color.Yellow;
                h1.ApplyToRow = true;

                StyleFormatCondition h2 = new StyleFormatCondition();
                gvMain.FormatConditions.Add(h2);
                h2.Column = gvMain.Columns["TinhTrangNP"];
                h2.Condition = FormatConditionEnum.Equal;
                h2.Value1 = "Nhập đủ";
                h2.Appearance.BackColor = Color.LightGreen;
                h2.ApplyToRow = true;
            }

            if ((_data.BsMain.DataSource as DataSet).Tables[0].Columns.Contains("DaSX"))
            {
                StyleFormatCondition h = new StyleFormatCondition();
                gvMain.FormatConditions.Add(h);
                h.Column = gvMain.Columns["DaSX"];
                h.Condition = FormatConditionEnum.Equal;
                h.Value1 = true;
                h.Appearance.BackColor = Color.Gainsboro;
                h.ApplyToRow = true;
            }
          
        }

        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public InfoCustomControl Info
        {
            get { return _info; }
        }

        #endregion
    }
}
