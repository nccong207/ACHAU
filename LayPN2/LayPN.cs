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
using CDTDatabase;
namespace LayPN
{
    public class LayPN : ICControl
    {
        string tb;
        DataRow drCur;
        GridView gvMain;
        ReportPreview frmDS;
        GridView gvDS;
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        string tenDonGia;
        DataSet ds;
        DataRow drMaster;
        Database data = Database.NewStructDatabase();
        #region ICControl Members

        public void AddEvent()
        {
            tb = _data.DrTableMaster["TableName"].ToString();
            if (tb != "MT43" && tb != "MT44")
                return;
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            //thêm nút chọn DH
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnChon = new SimpleButton();
            btnChon.Name = "btnChon";
            btnChon.Text = "Chọn hàng tồn";
            LayoutControlItem lci = lcMain.AddItem("", btnChon);
            lci.Name = "cusChon";
            btnChon.Click += new EventHandler(btnChon_Click);

            tenDonGia = tb == "MT43" ? "DonGia" : "GiaBan";
            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(_data.BsMain, new EventArgs());

            if (_data.BsMain.Current != null)
            {
                DataRow dr = (_data.BsMain.Current as DataRowView).Row;
                DataTableNewRowEventArgs e = new DataTableNewRowEventArgs(dr);
                PXGC_TableNewRow((_data.BsMain.DataSource as DataSet).Tables[0], e);
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            ds = _data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;

            ds.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(LayPN_ColumnChanged);
            ds.Tables[0].TableNewRow += new DataTableNewRowEventHandler(PXGC_TableNewRow);
        }

        void PXGC_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            if (e.Row == null)
                return;
            if (_data.DrTable.Table.Columns.Contains("ExtraSql")
                && _data.DrTable["ExtraSql"].ToString().Contains("'GC'"))
                e.Row["MaKho"] = "GC";
        }

        void LayPN_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || e.Row.RowState == DataRowState.Detached)
                return;
            if (_data.BsMain.Current != null)
                drMaster = (_data.BsMain.Current as DataRowView).Row;

            if (e.Column.ColumnName.ToUpper().Equals(tenDonGia.ToUpper()) || e.Column.ColumnName.ToUpper().Equals("SOLUONG"))
            {
                if (e.Row[tenDonGia] != DBNull.Value && e.Row["SoLuong"] != DBNull.Value)
                    e.Row["ThanhTien"] = Convert.ToDecimal(e.Row[tenDonGia]) * Convert.ToDecimal(e.Row["SoLuong"]);
            }
        }

        void btnChon_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            drCur = (_data.BsMain.Current as DataRowView).Row;
            string reportId = "1521";
            if (_data.DrTable.Table.Columns.Contains("ExtraSql")
                && _data.DrTable["ExtraSql"].ToString().Contains("'GC'"))
                reportId = "1554";
            frmDS = FormFactory.FormFactory.Create(FormType.Report, reportId) as ReportPreview;
            gvDS = (frmDS.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            gvDS.Columns["SL tồn cuối"].ColumnEdit = new RepositoryItemCalcEdit();
            gvDS.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvDS_CellValueChanged);
            gvDS.DataSourceChanged += new EventHandler(gvDS_DataSourceChanged);
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy = (frmDS.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy.Text = "Chọn hàng tồn";
            btnXuLy.Click += new EventHandler(btnXuLy_Click);
            frmDS.WindowState = FormWindowState.Maximized;
            frmDS.ShowDialog();
        }

        void gvDS_DataSourceChanged(object sender, EventArgs e)
        {
            gvDS.Columns["SL tồn cuối"].ColumnEdit = new RepositoryItemCalcEdit();
        }

        void gvDS_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "SL xuất")
            {
                object oc = gvDS.GetFocusedRowCellValue("SL tồn cuối");
                object ot = gvDS.GetFocusedRowCellValue("SL tồn");
                if (oc != null && oc.ToString() != "" && ot != null && ot.ToString() != "")
                {
                    if (decimal.Parse(oc.ToString()) > decimal.Parse(ot.ToString()))
                        XtraMessageBox.Show("Số lượng tồn cuối không thể lớn hơn tồn đầu",
                            Config.GetValue("PackageName").ToString());
                    else{
                        gvDS.SetFocusedRowCellValue(gvDS.Columns.ColumnByFieldName("Chọn"),1);
                        gvDS.RefreshData();
                    }
                }
            }
        }

        void btnXuLy_Click(object sender, EventArgs e)
        {
            DataTable dtDS = (gvDS.DataSource as DataView).Table;
            dtDS.AcceptChanges();
            if (!dtDS.Columns.Contains("SL còn"))
                dtDS.Columns.Add("SL còn", typeof(Decimal), "[SL tồn] - [SL tồn cuối]");
            DataRow[] drs = dtDS.Select("Chọn = 1 and [SL còn] < 0");
            if (drs.Length > 0)
            {
                XtraMessageBox.Show("Số tồn cuối không được vượt quá số tồn đầu (" + drs[0]["MaNL"].ToString() + ")", Config.GetValue("PackageName").ToString());
                gvDS.FocusedRowHandle = gvDS.GetRowHandle(dtDS.Rows.IndexOf(drs[0]));
                return;
            }
            drs = dtDS.Select("Chọn = 1");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Bạn chưa chọn hàng để xuất", Config.GetValue("PackageName").ToString());
                return;
            }
            frmDS.Close();
            //add du lieu vao danh sach
            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
            string pk = _data.DrTableMaster["Pk"].ToString();
            string dg = tb == "MT43" ? "DonGia" : "DGNhap";

            //LẤY COLUMNS GROUP
            string sql = string.Format(@"SELECT * FROM sysField f INNER JOIN sysTable t ON f.sysTableID = t.sysTableID
                            where t.TableName = '{0}' AND sysPackageID = 12 AND IsGroupCol =1", tb.Replace("MT", "DT"));
            DataTable dtTable = data.GetDataTable(sql);
            //XÓA GROUP
            gvMain.ClearGrouping();

            foreach (DataRow dr in drs)
            {
                if (dtDTKH.Select(string.Format(pk + " = '{0}' and DT42ID = '{1}'", drCur[pk], dr["DT42ID"])).Length > 0)
                    continue;
                gvMain.AddNewRow();
                gvMain.UpdateCurrentRow();
                gvMain.SetFocusedRowCellValue(gvMain.Columns["MaNCC"], dr["MaNCC"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoPN"], dr["SoCT"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["NgayPN"], dr["NgayCT"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["MaNL"], dr["MaNL"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["DL"], dr["DL"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SLNhap"], dr["SL nhập"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLuong"], dr["SL còn"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns[dg], dr["DonGia"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["DT42ID"], dr["DT42ID"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["MaCuon"], dr["MaCuon"]);
            }
            gvMain.RefreshData();

            //GROUP LẠI
            foreach (DataRow i in dtTable.Rows)
            {
                gvMain.Columns.ColumnByFieldName(i["FieldName"].ToString()).Group();
            }
            gvMain.RefreshData();
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
