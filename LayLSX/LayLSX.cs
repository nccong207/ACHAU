using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Data;
using CDTLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraEditors.Repository;
using System.Drawing;
using DevExpress.XtraGrid.Columns;
using FormFactory;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using CDTDatabase;

namespace LayLSX
{
    public class LayLSX : ICControl
    {
        GridHitInfo downHitInfo = null;
        DataRow drCur;
        ReportPreview frmDS;
        GridView gvDS;
        GridView gvMain;
        TextEdit teSLHT;
        DataRow drMaster;
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        Database data = Database.NewDataDatabase();
        #region ICControl Members

        public void AddEvent()
        {
            //định dạng thêm cho grid chi tiết hàng
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            //gvMain.CellValueChanged+=new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            Font f = new Font(gvMain.Columns["ChKho"].AppearanceCell.Font, FontStyle.Bold);
            gvMain.Columns["ChKho"].AppearanceCell.Font = f;
            gvMain.Columns["SoTam"].AppearanceCell.Font = f;
            gvMain.Columns["SLTon"].AppearanceCell.Font = f;
            List<string> lstField = new List<string>(new string[] { "ChKho", "ChDai", "SoTam", "SoMT", "Dao", "Lan1", "Cao1", "Lan2", "SoLuong", "SLSX" });
            foreach (GridColumn gc in gvMain.Columns)
            {
                if (lstField.Contains(gc.FieldName))
                    gc.AppearanceCell.BackColor = Color.LightCyan;
               
            }
            //chức năng drag drop để thay đổi thứ tự các dòng
            gvMain.Columns["Stt"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            gvMain.GridControl.AllowDrop = true;
            gvMain.MouseDown += new MouseEventHandler(gvMain_MouseDown);
            gvMain.MouseMove += new MouseEventHandler(gvMain_MouseMove);
            gvMain.GridControl.DragOver += new DragEventHandler(GridControl_DragOver);
            gvMain.GridControl.DragDrop += new DragEventHandler(GridControl_DragDrop);
            //thêm nút chọn LSX
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnChon = new SimpleButton();
            btnChon.Name = "btnChon";
            btnChon.Text = "Chọn LSX";
            LayoutControlItem lci = lcMain.AddItem("", btnChon);
            lci.Name = "cusChon";
            btnChon.Click += new EventHandler(btnChon_Click);
            //chuc nang danh dau hoan thanh
            //gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(_data.BsMain, new EventArgs());
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = _data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            if (_data.BsMain.Current != null)
                drMaster = (_data.BsMain.Current as DataRowView).Row;
            ds.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(LayLSX_ColumnChanged);
        }
        bool setSLSX = false;
        void LayLSX_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if(e.Row.RowState == DataRowState.Deleted || e.Row.RowState == DataRowState.Detached) return;
            if(e.Column.ColumnName.ToUpper().Equals("SLSX"))
            {
                object o = e.Row["SLTon"];
                object slht = e.Row["SLSX"];
                if (slht == DBNull.Value)
                    return;
//                string sql = @"    select		sum(x.soluong) * case when l.dao is null or l.dao = 0 then 1 else l.dao end [flag]
//                                   from		    dtlsx l left join dtxphoi x on l.dtdhid = x.dtdhid
//                                   where		l.dtlsxid = '{0}'
//                                   group by	    l.dtlsxid,l.dao";
                string sql = @"    select		sum(x.soluong) [flag]
                                   from		    dtlsx l left join dtxphoi x on l.dtdhid = x.dtdhid
                                   where		l.dtlsxid = '{0}'
                                   group by	    l.dtlsxid";
                object flag = data.GetValue(string.Format(sql, e.Row["DTLSXID"]));
                if (flag == DBNull.Value)
                {
                    XtraMessageBox.Show("Không thể sửa SLHT khi chưa lập phiếu xuất phôi sóng!", Config.GetValue("PackageName").ToString());
                    gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("SLSX"), null);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("SLTon"), null);
                    e.Row.EndEdit();
                    return;
                }
                if (Convert.ToInt32(slht) > Convert.ToInt32(flag))
                {
                    if(setSLSX == false)
                        XtraMessageBox.Show(string.Format("Mặt hàng '{0}' có SLHT lớn hơn SL xuất phôi: {1} > {2}."
                                        , gvMain.GetFocusedRowCellValue("TenHang"), Convert.ToInt32(slht), Convert.ToInt32(flag))
                                        , Config.GetValue("PackageName").ToString());
                    if (e.Row.RowState == DataRowState.Modified && !setSLSX)
                    {
                        setSLSX = true;
                        gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("SLSX"), e.Row["SLSX", DataRowVersion.Original]);
                        gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("SLTon"), e.Row["SLTon", DataRowVersion.Original]);
                        setSLSX = false;
                    }
                    else if (e.Row.RowState == DataRowState.Added && !setSLSX)
                    {
                        setSLSX = true;
                        gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("SLSX"), null);
                        gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("SLTon"), null);
                        setSLSX = false;
                    }
                    e.Row.EndEdit();
                    return;
                }
                //

                if (o != null && o.ToString() != "" && decimal.Parse(o.ToString()) <= 0)
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], true);
                else
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], false);
            }
        }

        void gvMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //if (e.Column.FieldName == "SLSX")
            //{
            //    setSLSX = false;
            //}
//            if (e.Column.FieldName == "SLSX")
//            {
//                setSLSX = false;
//                object o = gvMain.GetFocusedRowCellValue("SLTon");
//                object slht = gvMain.GetFocusedRowCellValue("SLSX");
//                if(slht == DBNull.Value)
//                    return;
//                //Bổ sung chỉ khi lập phiếu xuất phôi mới cho nhập số lượng hoàn thành
////                string sql = @"    select		isnull(sum(x.soluong) * l.dao,0) [flag]
////                                    from		dtlsx l left join dtxphoi x on l.dtdhid = x.dtdhid
////                                    where		l.dtlsxid = '{0}'
////                                    group by	l.dtlsxid,l.dao";
//                string sql = @"    select		isnull(sum(x.soluong),0) [flag]
//                                    from		dtlsx l left join dtxphoi x on l.dtdhid = x.dtdhid
//                                    where		l.dtlsxid = '{0}'
//                                    group by	l.dtlsxid";
//                object flag = data.GetValue(string.Format(sql, gvMain.GetFocusedRowCellValue("DTLSXID")));
//                if (Convert.ToInt32(slht) > Convert.ToInt32(flag))
//                {
//                    XtraMessageBox.Show(string.Format("Mặt hàng {0} có SLHT lớn hơn SL xuất phôi: {1} > {2}, không cho sửa!"
//                                        ,gvMain.GetFocusedRowCellValue("TenHang"),Convert.ToInt32(slht),Convert.ToInt32(flag))
//                                        , Config.GetValue("PackageName").ToString());
//                    gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("SLSX"),null);
//                    return;
//                }
//                //
                    
//                if (o != null && o.ToString() != "" && decimal.Parse(o.ToString()) <= 0)
//                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], true);
//                else
//                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], false);
               
//            }
        }

        void GridControl_DragDrop(object sender, DragEventArgs e)
        {
            GridControl grid = sender as GridControl;
            GridView view = grid.MainView as GridView;
            if (!view.Editable)
                return;
            GridHitInfo srcHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
            GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
            int sourceRow = srcHitInfo.RowHandle;
            int targetRow = hitInfo.RowHandle;
            MoveRow(sourceRow, targetRow);
        }

        void GridControl_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GridHitInfo)))
            {
                GridHitInfo downHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
                if (downHitInfo == null)
                    return;

                GridControl grid = sender as GridControl;
                GridView view = grid.MainView as GridView;
                GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
                if (hitInfo.InRow && hitInfo.RowHandle != downHitInfo.RowHandle && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                    e.Effect = DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.None;
            } 
        }

        void gvMain_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Button == MouseButtons.Left && downHitInfo != null)
            {
                Size dragSize = SystemInformation.DragSize;
                Rectangle dragRect = new Rectangle(new Point(downHitInfo.HitPoint.X - dragSize.Width / 2,
                    downHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    view.GridControl.DoDragDrop(downHitInfo, DragDropEffects.All);
                    downHitInfo = null;
                }
            }
        }

        void gvMain_MouseDown(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            downHitInfo = null;

            GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None)
                return;
            if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                downHitInfo = hitInfo;
        }

        private void MoveRow(int sourceRow, int targetRow)
        {
            if (sourceRow == targetRow || sourceRow == targetRow + 1)
                return;

            DataRow row1 = gvMain.GetDataRow(targetRow);
            DataRow row2 = gvMain.GetDataRow(targetRow + 1);
            DataRow dragRow = gvMain.GetDataRow(sourceRow);
            decimal val1 = (decimal)row1["Stt"];
            if (row2 == null)
                dragRow["Stt"] = val1 + 1;
            else
            {
                decimal val2 = (decimal)row2["Stt"];
                dragRow["Stt"] = (val1 + val2) / 2;
            }
            gvMain.RefreshData();
        }

        void btnChon_Click(object sender, EventArgs e)
        {
            drCur = (_data.BsMain.Current as DataRowView).Row;
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            //dùng report 1514 trong sysReport
            frmDS = FormFactory.FormFactory.Create(FormType.Report, "1514") as ReportPreview;
            //định dạng thêm cho grid của report
            gvDS = (frmDS.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            StyleFormatCondition hh = new StyleFormatCondition();
            gvDS.FormatConditions.Add(hh);
            hh.Column = gvDS.Columns["SL đã SX"];
            hh.Condition = FormatConditionEnum.NotEqual;
            hh.Value1 = 0;
            hh.Appearance.BackColor = Color.LightCyan;
            hh.ApplyToRow = true;
            gvDS.DataSourceChanged += new EventHandler(gvDS_DataSourceChanged);
            FormatGrid();
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy = (frmDS.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy.Text = "Chọn LSX";
            btnXuLy.Click += new EventHandler(btnXuLy_Click);
            frmDS.WindowState = FormWindowState.Maximized;
            frmDS.ShowDialog();
        }

        void FormatGrid()
        {
            for (int i = 0; i < gvDS.VisibleColumns.Count; i++)
            {
                GridColumn gc = gvDS.VisibleColumns[i];
                if (gc.FieldName == "SoLuong")
                    gc.Caption = "SL tồn";
                if (gc.FieldName == "ChKho" || gc.FieldName == "SoLuong")
                {
                    Font f = new Font(gc.AppearanceCell.Font, FontStyle.Bold);
                    gc.AppearanceCell.Font = f;
                }
                if (gc.FieldName == "SoLuong")
                    gc.AppearanceCell.BackColor = Color.Pink;
            }
        }

        void gvDS_DataSourceChanged(object sender, EventArgs e)
        {
            FormatGrid();
        }

        void btnXuLy_Click(object sender, EventArgs e)
        {
            DataTable dtDS = (gvDS.DataSource as DataView).Table;
            dtDS.AcceptChanges();
            DataRow[] drs = dtDS.Select("Chọn = 1");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Bạn chưa chọn lệnh sản xuất", Config.GetValue("PackageName").ToString());
                return;
            }
            frmDS.Close();
            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
            using (DataTable tmp = dtDTKH.Clone())
            {
                tmp.PrimaryKey = null;
                tmp.Columns["DTKHID"].AllowDBNull = true;
                tmp.Columns["DTKHID"].Unique = false;
                foreach (DataRow dr in drs)
                {
                    tmp.ImportRow(dr);
                    tmp.Rows[tmp.Rows.Count-1]["SoLuong"] = dr["SL đặt"];
                    tmp.Rows[tmp.Rows.Count - 1]["KyThuat"] = dr["Kỹ thuật"];
                    tmp.Rows[tmp.Rows.Count - 1]["IsIn"] = dr["In"];
                    tmp.Rows[tmp.Rows.Count - 1]["IsChap"] = dr["Chạp"];
                    tmp.Rows[tmp.Rows.Count - 1]["IsDongGhim"] = dr["Đóng ghim"];
                    tmp.Rows[tmp.Rows.Count - 1]["IsPhu"] = dr["Độ phủ"];
                    tmp.Rows[tmp.Rows.Count - 1]["IsDan"] = dr["Dan"];
                    tmp.Rows[tmp.Rows.Count - 1]["Dao"] = dr["Cắt dọc"];
                    tmp.Rows[tmp.Rows.Count - 1]["IsBoi"] = dr["Bồi"];
                }
                decimal n = gvMain.DataRowCount == 0 ? 0 : (decimal)gvMain.GetRowCellValue(gvMain.DataRowCount - 1, "Stt");
                foreach (DataRow dr in tmp.Rows)
                {
                    if (dtDTKH.Select(string.Format("MTKHID = '{0}' and DTLSXID = '{1}'", drCur["MTKHID"], dr["DTLSXID"])).Length > 0)
                        continue;
                    n++;
                    dr["MTKHID"] = drCur["MTKHID"];
                    dr["DTKHID"] = Guid.NewGuid();
                    dr["HT"] = false;
                    dr["isKho"] = false;
                    dr["isCB"] = false;
                    dr["Stt"] = n;
                    DataRow drNew = dtDTKH.NewRow();
                    drNew.ItemArray = (object[])dr.ItemArray.Clone();
                    dtDTKH.Rows.Add(drNew);
                    if (drNew["Dao"].ToString() != "" && decimal.Parse(drNew["Dao"].ToString()) != 0)
                        drNew["SoTam"] = decimal.Parse(drNew["SoLuong"].ToString()) / decimal.Parse(drNew["Dao"].ToString());
                    if (drNew["SoTam"].ToString() != "")
                        drNew["SoMT"] = decimal.Parse(drNew["SoTam"].ToString()) * decimal.Parse(drNew["ChDai"].ToString()) / 100;
                } 
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
