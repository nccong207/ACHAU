using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;
using CDTLib;
using System.Data;
using System.Windows.Forms;
using CDTDatabase;
using DevExpress.XtraLayout.Utils;
using System.Drawing;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.IO;
using System.Diagnostics;
using System.Data.OleDb;

namespace XuLyBG
{
    public class XuLyBG : ICControl
    {
        List<string> lstNL = new List<string>(new string[] { "Mat_", "SB_", "MB_", "SC_", "MC_", "SE_", "ME_" });
        DataTable dtNL;
        bool focusing = false;
        bool doikho = false;
        GridView gvMain;
        string dkThung = "";
        string dkTam = "";
        Database db = Database.NewDataDatabase();
        CheckEdit ceKCT;
        GridLookUpEdit gluKH;
        LayoutControl lcMain;
        ComboBoxEdit cbeLoai;
        ComboBoxEdit cbeLoaiThung;
        CalcEdit ceRong;
        List<GridLookUpEdit> lstGiay = new List<GridLookUpEdit>();
        private List<string> visibleCls = new List<string>();
        private List<string> captionCls = new List<string>();
        private DataCustomFormControl _data;
        private InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        string tableName;
        bool blFirst = false;
        GridHitInfo downHitInfo = null;
        CalcEdit ceDai;
        CalcEdit ceDaiG;
        CalcEdit ceRongG;
        DateEdit deNgayCT;
        bool blGiay = false;//Luu trang thai chon giay

        string[] mtFields = new string[] { "NgayCT", "MaKH", "NVPT", "NVTM", "DiaChi" };
        string[] dtFields = new string[] {"Loai","Lop","TenHang","DVT","SLBG","SoLuong","SLPhoi","GiaPhoi","NgayGH","KetCau","Dai",
                    "Rong","Cao","KhoMax","KhoTT","DaiG","RongG","DoKho","DoPhu","Ghim","Dan","SoMau","LoaiThung","Dao","DienTich","Mat_Giay","SB_Giay",
                    "MB_Giay","SC_Giay","MC_Giay","SE_Giay","ME_Giay","Mat_Kho","SB_Kho","MB_Kho","SC_Kho","MC_Kho","Mat_DL","SB_DL","MB_DL","SC_DL",
                    "MC_DL","Mat_DG","SB_DG","MB_DG","SC_DG","MC_DG","SE_DG","ME_DG","CPK","CPK_GC","CK","CK_GC","GiaBan","ThanhTien","KCT","May1","May2","May3","GhiChu" };

        #region ICControl Members

        public void AddEvent()
        {
            tableName = _data.DrTableMaster["TableName"].ToString();
            List<string> lstTB = new List<string>(new string[] { "MTBaoGia", "MTDonHang", "MTLSX" });
            List<string> lstSo = new List<string>(new string[] { "SoBG", "SoDH", "SoDH" });
            if (!lstTB.Contains(tableName))
                return;
            lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gluKH = _data.FrmMain.Controls.Find("MaKH", true)[0] as GridLookUpEdit;
            ceKCT = _data.FrmMain.Controls.Find("KCT", true)[0] as CheckEdit;
            deNgayCT = _data.FrmMain.Controls.Find("NgayCT", true)[0] as DateEdit;

            gvMain.BeforeLeaveRow += new DevExpress.XtraGrid.Views.Base.RowAllowEventHandler(gvMain_BeforeLeaveRow);
            gvMain.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gvMain_FocusedRowChanged);

            CalcEdit ceDT = _data.FrmMain.Controls.Find("DienTich", true)[0] as CalcEdit;
            ceDT.Enter += new EventHandler(ceDT_Enter);
            CalcEdit ceGB = _data.FrmMain.Controls.Find("GiaBan", true)[0] as CalcEdit;
            ceGB.Enter += new EventHandler(ceGB_Enter);
            CalcEdit ceKTT = _data.FrmMain.Controls.Find("KhoTT", true)[0] as CalcEdit;
            ceKTT.Enter += new EventHandler(ceKTT_Enter);
            CalcEdit ceDao = _data.FrmMain.Controls.Find("Dao", true)[0] as CalcEdit;
            ceDao.Enter += new EventHandler(ceDao_Enter);
            ceDaiG = _data.FrmMain.Controls.Find("DaiG", true)[0] as CalcEdit;
            ceDaiG.Enter += new EventHandler(ceDaiG_Enter);
            ceRongG = _data.FrmMain.Controls.Find("RongG", true)[0] as CalcEdit;
            ceRongG.Enter += new EventHandler(ceRongG_Enter);
            cbeLoaiThung = _data.FrmMain.Controls.Find("LoaiThung", true)[0] as ComboBoxEdit;
            cbeLoai = _data.FrmMain.Controls.Find("Loai", true)[0] as ComboBoxEdit;
            cbeLoai.EditValueChanged += new EventHandler(cbeLoai_EditValueChanged);
            ceDai = _data.FrmMain.Controls.Find("Dai", true)[0] as CalcEdit;
            ceRong = _data.FrmMain.Controls.Find("Rong", true)[0] as CalcEdit;


            GridLookUpEdit gluMat = _data.FrmMain.Controls.Find("Mat_Giay", true)[0] as GridLookUpEdit;
            GridLookUpEdit gluSB = _data.FrmMain.Controls.Find("SB_Giay", true)[0] as GridLookUpEdit;
            GridLookUpEdit gluMB = _data.FrmMain.Controls.Find("MB_Giay", true)[0] as GridLookUpEdit;
            GridLookUpEdit gluSC = _data.FrmMain.Controls.Find("SC_Giay", true)[0] as GridLookUpEdit;
            GridLookUpEdit gluMC = _data.FrmMain.Controls.Find("MC_Giay", true)[0] as GridLookUpEdit;
            GridLookUpEdit gluSE = _data.FrmMain.Controls.Find("SE_Giay", true)[0] as GridLookUpEdit;
            GridLookUpEdit gluME = _data.FrmMain.Controls.Find("ME_Giay", true)[0] as GridLookUpEdit;
            
            lstGiay.AddRange(new GridLookUpEdit[] { gluMat, gluSB, gluMB, gluSC, gluMC, gluSE, gluME });
            SetDMGiay();
            foreach (GridLookUpEdit glu in lstGiay)
            {
                glu.Popup += new EventHandler(gluGiay_Popup);
                glu.KeyDown += new KeyEventHandler(glu_KeyDown);
                glu.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluGiay_CloseUp);
            }

            if (tableName == "MTBaoGia")
            {
                gluKH.EditValueChanged += new EventHandler(gluKH_EditValueChanged);

                SimpleButton btnXL = new SimpleButton();
                btnXL.Text = "Nhập từ excel";
                btnXL.Name = "btnXL";
                btnXL.Click += new EventHandler(btnXL_Click);
                LayoutControlItem lci3 = lcMain.AddItem("", btnXL);
                lci3.Name = "cusXL";
            }

            // Set row style for Gridview
            if (tableName == "MTDonHang" || tableName == "MTLSX")
            {
                ceDai.Enter += new EventHandler(ceDai_Enter);
                ceRong.Enter += new EventHandler(ceRong_Enter);
                gvMain.OptionsView.EnableAppearanceEvenRow = false;
                gvMain.OptionsView.EnableAppearanceOddRow = false;
                gvMain.Appearance.FocusedRow.BackColor = Color.Transparent;
                gvMain.RowStyle += new RowStyleEventHandler(gvMain_RowStyle);
            }

            if (tableName == "MTDonHang")
            {
                ceRong.EditValueChanged += new EventHandler(ceRong_EditValueChanged);
                //ceRong.Leave += new EventHandler(ceRong_Leave); tạm thời ẩn chức năng xóa Mat_Kho khi thay đổi chiều rộng
                // Tạo nút lấy khổ giấy
                SimpleButton btnKho = new SimpleButton();
                btnKho.Name = "btnKho";
                btnKho.Text = "Lấy khổ";
                LayoutControlItem lci = lcMain.AddItem("", btnKho);
                lci.Name = "cusKho";
                btnKho.Click += new EventHandler(btnKho_Click);

                SimpleButton btnXuatFile = new SimpleButton();
                btnXuatFile.Text = "Xuất ra Excel";
                btnXuatFile.Name = "btnXuatFile";
                btnXuatFile.Click += new EventHandler(btnXuatFile_Click);
                LayoutControlItem lci2 = lcMain.AddItem("", btnXuatFile);
                lci2.Name = "cusXuatFile";
            }

            if (tableName == "MTLSX")
            {
                _data.FrmMain.Load += new EventHandler(FrmMain_Load);
                _data.FrmMain.FormClosed += new FormClosedEventHandler(FrmMain_FormClosed);
                _data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            }
            gvMain.ShownEditor += new EventHandler(gvMain_ShownEditor);

            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(_data.BsMain, new EventArgs());
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng thực hiện khi đã lưu đơn hàng",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            DataRow drCurrent = (_data.BsMain.Current as DataRowView).Row;

            if (!bool.Parse(drCurrent["Duyet"].ToString()))
            {
                XtraMessageBox.Show("Chỉ xuất file Excel đối với đơn hàng đã duyệt.",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.RestoreDirectory = true;
            sfd.FileName = drCurrent["SoDH"].ToString();
            sfd.Filter = "Excel files (*.xls)|*.xls";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string sql = @"SELECT mt.{0}, dt.{1}
                            FROM MTDonHang mt
                            INNER JOIN DTDonHang dt on mt.MTDHID = dt.MTDHID
                            WHERE mt.MTDHID = '{2}' ORDER BY dt.Stt";
                Database db = Database.NewDataDatabase();

                DataTable dtData = db.GetDataTable(string.Format(sql, string.Join(", mt.", mtFields), string.Join(", dt.", dtFields), drCurrent["MTDHID"]));
                string f = Application.StartupPath + "\\Reports\\HTC\\MauDonHang.xls";

                ExportExcel exportExcel = new ExportExcel(f, sfd.FileName, dtData);
                if (exportExcel.Export() && File.Exists(sfd.FileName))
                    Process.Start(sfd.FileName);
            }
        }

        private bool KiemTraKhoaNgoai(DataRow drSource)
        {
            var fields = new string[] { "MaKH", "NVPT", "NVTM", "Mat_Giay", "SB_Giay", "MB_Giay", "SC_Giay", "MC_Giay", "SE_Giay", "ME_Giay" };
            var tables = new string[] { "DMKH", "DMNhanVien", "DMNVTM", "DMNL", "DMNL", "DMNL", "DMNL", "DMNL", "DMNL", "DMNL" };
            var refFields = new string[] { "MaKH", "MaNV", "MaNV", "Ma", "Ma", "Ma", "Ma", "Ma", "Ma", "Ma" };

            var sql = @" if not exists (select * from HTCAC..{0} where {1} = N'{2}')
                    insert into HTCAC..{0}
                    select * from HTCTL..{0}
                    where {1} = N'{2}'";

            var allFields = new List<string>();
            allFields.AddRange(mtFields);
            allFields.AddRange(dtFields);

            for (int i = 0; i < fields.Length; i++)
            {
                var index = allFields.IndexOf(fields[i]);
                if (drSource[index] != DBNull.Value)
                    if (!db.UpdateByNonQuery(string.Format(sql, tables[i], refFields[i], drSource[index])))
                        return false;
            }
            return true;
        }

        void btnXL_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng thực hiện khi đang thêm báo giá",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            OpenFileDialog f = new OpenFileDialog();
            f.RestoreDirectory = true;
            f.Filter = "Excel files (*.xls)|*.xls";
            if (f.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                var dtSource = new DataTable();
                try
                {
                    //nap data sheet vao _dtSource
                    string cnn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + f.FileName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1'";
                    OleDbDataAdapter myCommand = new OleDbDataAdapter("SELECT * FROM [DonHang$]", cnn);
                    myCommand.Fill(dtSource);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi lấy số liệu từ excel\n" + ex.Message,
                        Config.GetValue("PackageName").ToString());
                    Cursor.Current = Cursors.Default;
                    return;
                }
                if (dtSource.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Không tìm thấy dữ liệu đơn hàng", Config.GetValue("PackageName").ToString());
                    Cursor.Current = Cursors.Default;
                    return;
                }

                var drCurrent = (_data.BsMain.Current as DataRowView).Row;
                for (int i = 0; i < mtFields.Length; i++)
                {
                    drCurrent[mtFields[i]] = dtSource.Rows[0][i];
                }

                var dtDetail = (_data.BsMain.DataSource as DataSet).Tables[1];
                dtDetail.BeginLoadData();
                var mtColumns = mtFields.Length;
                var lstFields = new List<string>(dtFields);
                for (int i = 0; i < dtSource.Rows.Count; i++)
                {
                    var source = dtSource.Rows[i];
                    if (!KiemTraKhoaNgoai(source))
                    {
                        XtraMessageBox.Show("Lỗi cập nhật dữ liệu khách hàng, nhân viên hoặc nguyên liệu giấy");
                        return;
                    }
                    var des = i > 0 ? dtDetail.NewRow() : dtDetail.Rows[dtDetail.Rows.Count - 1];
                    des["MTBGID"] = drCurrent["MTBGID"];
                    if (i > 0) dtDetail.Rows.Add(des);
                    for (int j = 0; j < lstFields.Count; j++)
                    {
                        if (dtDetail.Columns.Contains(lstFields[j]) && source[j + mtColumns] != DBNull.Value)
                        {
                            des[lstFields[j]] = source[j + mtColumns];
                        }
                    }
                    des["Loai"] = "Tấm";
                    var slPhoiIndex = lstFields.IndexOf("SLPhoi") + mtColumns;
                    var giaPhoiIndex = lstFields.IndexOf("GiaPhoi") + mtColumns;
                    if (source[slPhoiIndex] != DBNull.Value) des["SoLuong"] = source[slPhoiIndex];
                    if (source[giaPhoiIndex] != DBNull.Value) des["GiaBan"] = source[giaPhoiIndex];
                    des.EndEdit();
                }
                dtDetail.EndLoadData();
                Cursor.Current = Cursors.Default;
            }
        }

        void ceRongG_Enter(object sender, EventArgs e)
        {
            KhongChoChon(sender, !ceKCT.Checked);
        }

        void ceDaiG_Enter(object sender, EventArgs e)
        {
            KhongChoChon(sender, !ceKCT.Checked);
        }
        
        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataTable dtBaoGia = (_data.BsMain.DataSource as DataSet).Tables[1];
            dtBaoGia.ColumnChanged += new DataColumnChangeEventHandler(dtBaoGia_ColumnChanged);
            if(tableName == "MTLSX")
                dtBaoGia.TableNewRow += new DataTableNewRowEventHandler(dtBaoGia_TableNewRow);
        }

        void dtBaoGia_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            e.Row["Stt"] = (_data.BsMain.DataSource as DataSet).Tables[1].Select("MTLSXID = '"+ drCur["MTLSXID"].ToString() +"'").Length;
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

        #region xử lý Drop-down của Gridview
        void FrmMain_Shown(object sender, EventArgs e)
        {
            gvMain.Columns["Stt"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            //if (tableName == "MTLSX")       //bổ sung tính khối lượng yêu cầu ngay khi lập LSX
            //{
            //    if (_data.BsMain.DataSource == null || _data.BsMain.Current == null)
            //        return;
            //    DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            //    if (drCur.RowState == DataRowState.Added)   //chỉ chạy cho trường hợp tạo LSX mới, các trường hợp khác đã chạy trong ColumnChanged
            //        TinhYC(tableName, new DataColumnChangeEventArgs(drCur, drCur.Table.Columns["DienTich"], drCur["DienTich"]));
            //}
        }

        void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            gvMain.ClearSorting();
        }
        
        void FrmMain_Load(object sender, EventArgs e)
        {
            //chức năng drag drop để thay đổi thứ tự các dòng
        
            gvMain.GridControl.AllowDrop = true;
            gvMain.MouseDown += new MouseEventHandler(gvMain_MouseDown);
            gvMain.MouseMove += new MouseEventHandler(gvMain_MouseMove);
            gvMain.GridControl.DragOver += new DragEventHandler(GridControl_DragOver);
            gvMain.GridControl.DragDrop += new DragEventHandler(GridControl_DragDrop);
        }

        void gvMain_ShownEditor(object sender, EventArgs e)
        {
            if (deNgayCT.Properties.ReadOnly)
                return;

            GridView gv = sender as GridView;

            if (tableName == "MTLSX")
            {
                gvMain.OptionsBehavior.Editable = true;
                List<string> lstCol = new List<string>(new string[] { "MSong", "SLTT", "SLKD", "SLDat","LSong", "SLDC", "MTLSXID" });
                foreach (GridColumn i in gvMain.Columns)
                {
                    if (lstCol.Contains(i.FieldName))
                        i.OptionsColumn.AllowEdit = true;
                    else
                        i.OptionsColumn.AllowEdit = false;
                }
            }
            else
                gvMain.OptionsBehavior.Editable = false;
        }
        #endregion
        #region GridControl - Gridview Event

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
            if (!view.OptionsBehavior.Editable)
                return;
            GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None)
                return;
            if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                downHitInfo = hitInfo;
        }

        void gvMain_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0 && gvMain.IsDataRow(e.RowHandle))
            {
                object slbg = gvMain.GetRowCellValue(e.RowHandle, "SLBG");
                object sldh = gvMain.GetRowCellValue(e.RowHandle, "SoLuong");
                if (slbg == DBNull.Value || sldh == DBNull.Value)
                    e.Appearance.BackColor = Color.Transparent;
                else
                {
                    if (decimal.Parse(slbg.ToString()) > decimal.Parse(sldh.ToString()))
                        e.Appearance.BackColor = Color.Red;
                    else if (decimal.Parse(sldh.ToString()) > decimal.Parse(slbg.ToString()))
                        e.Appearance.BackColor = Color.Yellow;
                    else
                        e.Appearance.BackColor = Color.Transparent;
                }
            }
        }

        void gvMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (!deNgayCT.Properties.ReadOnly)
            {
                focusing = false;
                if (e.PrevFocusedRowHandle >= 0)
                {
                    object opr = gvMain.GetRowCellValue(e.PrevFocusedRowHandle, "Rong");
                    object or = gvMain.GetRowCellValue(e.FocusedRowHandle, "Rong");
                    string pr = (opr == null || opr.ToString() == "") ? "" : opr.ToString();
                    string r = (or == null || or.ToString() == "") ? "" : or.ToString();
                    if ((pr != "" && r == "") || (pr == "" && r != ""))
                        SetDMGiay();
                }
            }
        }

        void gvMain_BeforeLeaveRow(object sender, DevExpress.XtraGrid.Views.Base.RowAllowEventArgs e)
        {
            if (!deNgayCT.Properties.ReadOnly)
                focusing = true;
        }

        private void MoveRow(int sourceRow, int targetRow)
        {
            if (sourceRow == targetRow || sourceRow == targetRow + 1)
                return;
            if (blFirst == false)
            {
                for (int i = 0; i < gvMain.DataRowCount; i++)
                {
                    gvMain.GetDataRow(i)["Stt"] = i;
                }
                blFirst = true;
            }
            DataRow row1 = gvMain.GetDataRow(targetRow);
            DataRow row2 = gvMain.GetDataRow(targetRow + 1);
            DataRow dragRow = gvMain.GetDataRow(sourceRow);
            decimal val1 = (decimal)row1["Stt"];
            if (row2 == null)
                dragRow["Stt"] = val1 + 1;
            else
            {
                decimal val2 = (decimal)row2["Stt"];
                //if (val1 == val2)
                //    val2 -= 0.25M;
                dragRow["Stt"] = (val1 + val2) / 2;
                //dragRow["Stt"] = 2;
            }
            gvMain.RefreshData();
        }

        #endregion

        void ceRong_EditValueChanged(object sender, EventArgs e)
        {
            if (ceRong.Properties.ReadOnly || focusing)
                return;
            doikho = true;
        }

        void ceRong_Leave(object sender, EventArgs e)
        {
            //hàm này dùng để xóa khổ của mặt đầu tiên khi có sự thay đổi chiều rộng -> để người dùng phải nhấn lại nút lấy khổ
            if (ceRong.Properties.ReadOnly || ceRong.EditValue == null || ceRong.EditValue.ToString() == "" || focusing || !doikho)
                return;
            if (gvMain.GetFocusedRowCellValue(gvMain.Columns["Mat_Kho"]) == DBNull.Value)
                return;
            DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            drCur["Rong"] = ceRong.EditValue;
            gvMain.SetFocusedRowCellValue(gvMain.Columns["Mat_Kho"], DBNull.Value);
            gvMain.UpdateCurrentRow();
            doikho = false;
        }

        void btnKho_Click(object sender, EventArgs e)
        {
            if (ceRong.Properties.ReadOnly)
            {
                XtraMessageBox.Show("Vui lòng thực hiện khi thêm/sửa đơn hàng",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            if (dtNL == null)
            {
                dtNL = db.GetDataTable("select Ma, Ten, MaPhu, Kho from wDMNL");
                dtNL.PrimaryKey = new DataColumn[] { dtNL.Columns["Ma"] };
            }
            DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            gvMain.UpdateCurrentRow();
            drCur.EndEdit();
            DataRow dr = gvMain.GetDataRow(gvMain.FocusedRowHandle);
            if (dr.RowState != DataRowState.Added && dr.RowState != DataRowState.Modified)
                return;
            if (dr.RowState == DataRowState.Modified
                && dr["Rong", DataRowVersion.Original].ToString() == dr["Rong", DataRowVersion.Current].ToString())
                return;
            foreach (string s in lstNL)
            {
                string ma = dr[s + "Giay"].ToString();
                if (ma == "")
                    continue;
                DataRow drNL = dtNL.Rows.Find(ma);
                string maphu = drNL["MaPhu"].ToString();
                DataRow[] drs = dtNL.Select("MaPhu = '" + maphu + "' and Kho >= " + dr["Rong"].ToString(), "Kho");
                if (drs.Length > 0)
                {
                    gvMain.SetFocusedRowCellValue(gvMain.Columns[s + "Giay"], drs[0]["Ma"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns[s + "Kho"], drs[0]["Kho"]);
                }
                else
                {
                    XtraMessageBox.Show(dr["TenHang"].ToString() + " không có khổ giấy phù hợp!",
                        Config.GetValue("PackageName").ToString());
                }
            }
            gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("DaiG"),gvMain.GetFocusedRowCellValue("Dai"));
            gvMain.SetFocusedRowCellValue(gvMain.Columns.ColumnByFieldName("RongG"), gvMain.GetFocusedRowCellValue("Rong"));
            gvMain.UpdateCurrentRow();
            SetDMGiay();
        }

        void glu_KeyDown(object sender, KeyEventArgs e)
        {
            GridLookUpEdit glu = sender as GridLookUpEdit;
            if (!glu.Properties.ReadOnly && e.KeyCode == Keys.Delete)
            {
                string fn1 = glu.Name.Replace("_Giay", "_Kho");
                gvMain.SetFocusedRowCellValue(gvMain.Columns[fn1], DBNull.Value);
                string fn2 = glu.Name.Replace("_Giay", "_DL");
                gvMain.SetFocusedRowCellValue(gvMain.Columns[fn2], DBNull.Value);
                string fn3 = glu.Name.Replace("_Giay", "_DG");
                gvMain.SetFocusedRowCellValue(gvMain.Columns[fn3], DBNull.Value);
            }
        }

        void gluGiay_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode != PopupCloseMode.Normal)
                return;
            if (cbeLoai.EditValue == null || cbeLoai.EditValue.ToString() == "")
                return;
            GridLookUpEdit glu = sender as GridLookUpEdit;
            if (tableName == "MTLSX" || tableName == "MTDonHang" || ((tableName == "MTBaoGia") && (cbeLoai.EditValue.ToString() == "Thùng")))
            {
                string fn = glu.Name.Replace("_Giay", "_Kho");
                DataTable dt = (glu.Properties.DataSource as BindingSource).DataSource as DataTable;
                DataRow[] drs = dt.Select("Ma = '" + e.Value.ToString() + "'");
                if (drs.Length > 0)
                { 
                    gvMain.SetFocusedRowCellValue(gvMain.Columns[fn], drs[0]["Kho"]);
                }
            }
        }

        void gluGiay_Popup(object sender, EventArgs e)
        {
            if (cbeLoai.EditValue == null || cbeLoai.EditValue.ToString() == "")
                return;
            GridLookUpEdit glu = sender as GridLookUpEdit;
            if (tableName == "MTBaoGia" && cbeLoai.EditValue.ToString() == "Tấm")
            {
                glu.Properties.View.Columns["Ma"].Visible = false;
                //glu.Properties.View.Columns["MaPhu"].VisibleIndex = 0;
                glu.Properties.View.Columns["Kho"].Visible = false;
            }
            else
            {
                glu.Properties.View.Columns["Ma"].VisibleIndex = 0;
                //glu.Properties.View.Columns["MaPhu"].Visible = false;
                glu.Properties.View.Columns["Kho"].VisibleIndex = 9;
            }
        }

        // Gán thuộc tính display member cho loại giấy
        void SetDMGiay()
        {
            if (cbeLoai.EditValue == null || cbeLoai.EditValue.ToString() == "")
                return;
            string dm = "";
            if (tableName == "MTBaoGia")
            {
                dm = (cbeLoai.EditValue.ToString() == "Tấm") ? "MaPhu" : "Ma";
                if (cbeLoai.EditValue.ToString() == "Tấm")
                    cbeLoaiThung.EditValue = null;
                else
                    cbeLoaiThung.EditValue = "Thường";
            } 
            else
                if (tableName == "MTDonHang")
                {
                    if (ceRong.Properties.ReadOnly || cbeLoai.EditValue.ToString() == "Thùng")
                        dm = "Ma";
                    else
                        dm = (ceRong.EditValue == null || ceRong.EditValue.ToString() == "") ? "MaPhu" : "Ma";
                }
            if (dm != "")
                foreach (GridLookUpEdit glu in lstGiay)
                    glu.Properties.DisplayMember = dm;
        }

        void gluKH_EditValueChanged(object sender, EventArgs e)
        {
            if (gvMain.GridControl.Tag != null && (bool)gvMain.GridControl.Tag)
                return;
            if (gluKH.Properties.ReadOnly)
                return;
            if (gvMain.DataRowCount == 0)    //grid chi tiet chua co dong nao
            {
                gvMain.UpdateCurrentRow();
                gvMain.AddNewRow();
                gvMain.UpdateCurrentRow();
            }
            else
            {
                DataTable dt = (_data.BsMain.DataSource as DataSet).Tables[1];
                for (int i = 0; i < gvMain.DataRowCount; i++)
                {
                    DataRow dr = gvMain.GetDataRow(i);
                    dtBaoGia_ColumnChanged(dt, new DataColumnChangeEventArgs(dr, dt.Columns["Loai"], dr["Loai"]));
                }
            }
        }

        void cbeLoai_EditValueChanged(object sender, EventArgs e)
        {
            if (gvMain.GridControl.Tag != null && (bool)gvMain.GridControl.Tag)
                return;
            if (cbeLoai.EditValue == null)
                return;
            string loai = cbeLoai.EditValue.ToString();
            if (loai == "")
                return;
            SetDMGiay();
            if (tableName == "MTDonHang")
            {
                gvMain.Columns["Dai"].OptionsColumn.AllowFocus = (loai == "Tấm");
                gvMain.Columns["Rong"].OptionsColumn.AllowFocus = (loai == "Tấm");
                LayoutControlItem lci = lcMain.Items.FindByName("cusKho") as LayoutControlItem;
                if (lci != null)
                    lci.Visibility = (loai == "Thùng") ? LayoutVisibility.Never : LayoutVisibility.Always;
            }
            if (!gluKH.Properties.ReadOnly && gluKH.EditValue == DBNull.Value && cbeLoaiThung.EditValue != DBNull.Value && tableName != "MTLSX")
                XtraMessageBox.Show("Vui lòng chọn khách hàng trước", Config.GetValue("PackageName").ToString());
            bool isThung = loai == "Thùng";
            LayoutVisibility lv = isThung ? LayoutVisibility.Always : LayoutVisibility.Never;
            if (tableName == "MTLSX")
                gvMain.Columns["Stt"].SortOrder = DevExpress.Data.ColumnSortOrder.None;
            foreach (BaseLayoutItem li in lcMain.Items)
            {
                if (li.Text == "Loại thùng")
                    li.Visibility = lv;
                if (li.GetType() != typeof(LayoutControlGroup))
                    continue;
                if (li.Text == "Thung")
                    li.Visibility = lv;
                if (tableName == "MTBaoGia" && li.Text == "Khổ")
                    li.Visibility = lv;
            }
            if (tableName == "MTLSX")
                gvMain.Columns["Stt"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        }

        #region Enter ...
        void ceDao_Enter(object sender, EventArgs e)
        {
            KhongChoChon(sender, !ceKCT.Checked);
        }

        void ceKTT_Enter(object sender, EventArgs e)
        {
            KhongChoChon(sender, !ceKCT.Checked);
        }

        void ceGB_Enter(object sender, EventArgs e)
        {
            KhongChoChon(sender, !ceKCT.Checked);
        }

        void ceDT_Enter(object sender, EventArgs e)
        {
            KhongChoChon(sender, !ceKCT.Checked);
        }

        void ceRong_Enter(object sender, EventArgs e)
        {
            //KhongChoChon(sender, cbeLoai.Text == "Thùng");
        }

        void ceDai_Enter(object sender, EventArgs e)
        {
            //KhongChoChon(sender, cbeLoai.Text == "Thùng");
        }

        private void KhongChoChon(object sender, bool condition)
        {
            if (condition)
                lcMain.FocusHelper.GetNextControl(sender).Focus();
        }
        
        #endregion

        void CapNhatDieuKhoan(DataRow dr)
        {
            string loai = dr["Loai"].ToString();
            if (loai == "")
                dr["DieuKhoan"] = DBNull.Value;
            if (loai == "Tấm")
            {
                if (dkTam == "")
                {
                    object o = db.GetValue("select top 1 DkTam from CHDKBG");
                    dkTam = o == null ? "" : o.ToString();
                }
                if (dr["DieuKhoan"].ToString() != dkTam)
                    dr["DieuKhoan"] = dkTam;
            }
            if (loai == "Thùng")
            {
                if (dkThung == "")
                {
                    object o = db.GetValue("select top 1 DkThung from CHDKBG");
                    dkThung = o == null ? "" : o.ToString();
                }
                if (dr["DieuKhoan"].ToString() != dkThung)
                    dr["DieuKhoan"] = dkThung;
            }
            dr.EndEdit();
        }
        
        void dtBaoGia_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (tableName == "MTLSX" && (gvMain.GridControl.Tag != null && (bool)gvMain.GridControl.Tag) && 
                    (e.Column.ColumnName.EndsWith("_DL")))   //bổ sung cho trường hợp đang đổ số liệu từ đơn hàng vào LSX
            {
                //bổ sung tính số lượng phôi
                //TinhSLPhoi(tableName, e);
                //Tính yc
                TinhYC(tableName, e);
            }
            //form chua hien thi, hoac dang change focus cua grid mat hang -> khong chay cong thuc
            if (!_data.FrmMain.Visible || focusing || (gvMain.GridControl.Tag != null && (bool)gvMain.GridControl.Tag))
                return;
            if (tableName == "MTBaoGia" && e.Column.ColumnName == "Loai")
                CapNhatDieuKhoan(e.Row);
            string f = e.Column.ColumnName.ToUpper();
            if (f.EndsWith("GIAY") && gluKH.EditValue == DBNull.Value && tableName != "MTLSX")
                XtraMessageBox.Show("Vui lòng chọn khách hàng", Config.GetValue("PackageName").ToString());
            if (f.EndsWith("GIAY") && e.Row["Lop"] == DBNull.Value)
                XtraMessageBox.Show("Vui lòng chọn số lớp", Config.GetValue("PackageName").ToString());
            if (e.Row["Loai"].ToString() == "" || e.Row["Lop"].ToString() == "")
                return;
            bool isTam = e.Row["Loai"].ToString() == "Tấm";
            //tinh so luong phoi
            TinhSLPhoi(tableName, e);
            //tinh kho max
            TinhKhoMax(tableName, e);
            //Tính các thông số
            TinhTSKThuat(tableName, e);
            //Tính số lượng YC
            TinhYC(tableName, e);
            //Tính giá bán
            TinhGiaBan(tableName,Convert.ToBoolean(e.Row["KCT"]), e);
            //tinh thanh tien cua DonHang
            TinhTTien(tableName, e); 
            e.Row.EndEdit();
        }

        decimal LayHoaHong()
        {
            if (gluKH.EditValue == null || gluKH.EditValue.ToString() == "")
                return 0;
            DataTable dtKH = (gluKH.Properties.DataSource as BindingSource).DataSource as DataTable;
            DataRow[] drs = dtKH.Select("MaKH = '" + gluKH.EditValue.ToString() + "'");
            if (drs.Length == 0)
                return 0;
            decimal hhpt = drs[0]["NVPT_HH"].ToString() == "" ? 0 : decimal.Parse(drs[0]["NVPT_HH"].ToString());
            decimal hhtm = drs[0]["NVTM_HH"].ToString() == "" ? 0 : decimal.Parse(drs[0]["NVTM_HH"].ToString());
            return (hhtm + hhpt);
        }

        private decimal ObjToDec(object o, decimal defaultIfEmpty = 0)
        {
            return o == null || o == DBNull.Value || o.ToString() == string.Empty ? defaultIfEmpty : Convert.ToDecimal(o);
        }

        private void TinhSLPhoi(string tableName,DataColumnChangeEventArgs e)
        {
            //Tính số lượng phôi
            if ((tableName == "MTDonHang" && e.Column.ColumnName.ToUpper() == "SOLUONG") 
                || (tableName == "MTLSX" && e.Column.ColumnName.ToUpper() == "SLSX"))
            {
                var sl = tableName == "MTDonHang" ? ObjToDec(e.Row["SoLuong"]) : ObjToDec(e.Row["SLSX"]);
                var dao = ObjToDec(e.Row["Dao"], 1);
                if (dao == 0) dao = 1;
                if (e.Row["Loai"].ToString() == "Thùng")
                {
                    if (ObjToDec(e.Row["Rong"]) + ObjToDec(e.Row["Cao"]) <= ObjToDec(e.Row["RongG"]))
                    {
                        e.Row["SLPhoi"] = sl / dao;
                    }
                    else
                    {
                        decimal y = Math.Round((ObjToDec(e.Row["Rong"]) + ObjToDec(e.Row["Cao"])) / ObjToDec(e.Row["RongG"], 1), 0);
                        if (y == 1)
                            e.Row["SLPhoi"] = sl / dao;
                        else
                            e.Row["SLPhoi"] = sl / (y == 0 ? 1 : y);
                    }
                }
                else
                    e.Row["SLPhoi"] = sl;
                //}
            }
        }

        //TÍNH KHỐI LƯỢNG GIẤY CẦN SỬ DỤNG (HÀM Cũ)
        //private void TinhYC(string tableName, DataColumnChangeEventArgs e)
        //{
        //    if (tableName != "MTLSX")   //hiện tại chỉ tính cho lệnh sản xuất
        //        return;
        //    string f = e.Column.ColumnName.ToUpper();
        //    //Tấm: tính khi khổ giấy thay đổi hoặc định lượng thay đổi
        //    //Thùng: tính khi diện tích thay đổi hoặc định lượng thay đổi
        //    if (f.EndsWith("_DL"))
        //    {
        //        decimal dt;
        //        if (e.Row["Loai"].ToString() == "Thùng")
        //            dt = Convert.ToDecimal(e.Row["DienTich"]);
        //        else
        //            dt = Convert.ToDecimal(e.Row["Dai"]) * Convert.ToDecimal(e.Row["Rong"]) / 10000;
        //        decimal hs = f.StartsWith("M") ? 1M : 1.5M;
        //        //Bổ sung điều kiện a = khổ max / giấy rộng
        //        decimal a = Convert.ToDecimal(e.Row["KhoMax"]) / Convert.ToDecimal(e.Row["RongG"]);
        //        a = decimal.ToInt32(a);
        //        decimal yc=0;
        //        //Bổ sung điều kiện rộng + cao
        //        if (e.Row["Loai"].ToString() == "Thùng")
        //        {
        //            if (Convert.ToDecimal(e.Row["Rong"]) + (e.Row["Cao"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["Cao"])) <= Convert.ToDecimal(e.Row["RongG"]))
        //            {
        //                if (a == Convert.ToDecimal(e.Row["Dao"])
        //                   || (a != Convert.ToDecimal(e.Row["Dao"]) && Convert.ToDecimal(e.Row["KhoMax"]) < Convert.ToDecimal(e.Row["KhoTT"])))
        //                    yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLSX"]) / 1000;
        //                else
        //                    yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * a / 1000;
        //            }
        //            else
        //            {
        //                decimal y = Math.Round(((Convert.ToDecimal(e.Row["Rong"]) + (e.Row["Cao"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["Cao"]))) /
        //                    (Convert.ToDecimal(e.Row["RongG"]) == 0 ? 1 : Convert.ToDecimal(e.Row["RongG"]))), 0);
        //                if (y == 1)
        //                {
        //                    if (a == Convert.ToDecimal(e.Row["Dao"])
        //                        || (a != Convert.ToDecimal(e.Row["Dao"]) && Convert.ToDecimal(e.Row["KhoMax"]) < Convert.ToDecimal(e.Row["KhoTT"])))
        //                        yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLSX"]) / 1000;
        //                    else
        //                        yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * a / 1000;
        //                }
        //                yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLSX"]) * y / 1000;
        //            }
        //        }
        //        else
        //            yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLSX"]) / 1000;

        //        string fn = f.Replace("_DL", "_YC");
        //        e.Row[fn] = yc;
        //    }
        //    if (f == "DIENTICH" || f == "SLPhoi" || f == "SLSX")
        //    {
        //        foreach (string s in lstNL)
        //        {
        //            //Kiểm tra định lượng rỗng, nếu rỗng thì không tính yc.
        //            if (e.Row[s + "DL"] == DBNull.Value)
        //                continue;
        //            decimal dt;
        //            if (e.Row["Loai"].ToString() == "Thùng")
        //                dt = Convert.ToDecimal(e.Row["DienTich"]);
        //            else
        //                dt = Convert.ToDecimal(e.Row["Dai"]) * Convert.ToDecimal(e.Row["Rong"]) / 10000;
        //            decimal hs = s.StartsWith("M") ? 1M : 1.5M;
        //            decimal yc = 0;
        //            //Bổ sung điều kiện a = khổ max / giấy rộng
        //            decimal a = Convert.ToDecimal(e.Row["KhoMax"]) / Convert.ToDecimal(e.Row["RongG"]);
        //            a = decimal.ToInt32(a);
        //            if (e.Row["Loai"].ToString() == "Thùng")
        //            {
        //                if (Convert.ToDecimal(e.Row["Rong"]) + (e.Row["Cao"] == DBNull.Value?0: Convert.ToDecimal(e.Row["Cao"])) <= Convert.ToDecimal(e.Row["RongG"]))
        //                {
        //                    if (a == Convert.ToDecimal(e.Row["Dao"])
        //                        || (a != Convert.ToDecimal(e.Row["Dao"]) && Convert.ToDecimal(e.Row["KhoMax"]) < Convert.ToDecimal(e.Row["KhoTT"])))
        //                        yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLSX"]) / 1000;
        //                    else
        //                        yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * a / 1000;
        //                }
        //                else
        //                {
        //                    decimal y = Math.Round(((Convert.ToDecimal(e.Row["Rong"]) + (e.Row["Cao"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["Cao"]))) /
        //                   (Convert.ToDecimal(e.Row["RongG"]) == 0 ? 1 : Convert.ToDecimal(e.Row["RongG"]))), 0);
        //                    if (y == 1)
        //                    {
        //                        if (a == Convert.ToDecimal(e.Row["Dao"])
        //                        || (a != Convert.ToDecimal(e.Row["Dao"]) && Convert.ToDecimal(e.Row["KhoMax"]) < Convert.ToDecimal(e.Row["KhoTT"])))
        //                            yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLSX"]) / 1000;
        //                        else
        //                            yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * a / 1000;
        //                    }
        //                    else
        //                        yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLSX"]) * y / 1000;

        //                }
        //            }
        //            else
        //                yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLSX"]) / 1000;
        //            string fn = s + "YC";
        //            e.Row[fn] = yc;
        //        }
        //    }
        //}

        //TÍNH KHỐI LƯỢNG GIẤY CẦN SỬ DỤNG

        private void TinhYC(string tableName, DataColumnChangeEventArgs e)  //hàm mới (lấy lại công thức cũ - chỉ thay SLSX bằng SLPhoi * Dao)
        {
            if (tableName != "MTLSX")   //hiện tại chỉ tính cho lệnh sản xuất
                return;
            string f = e.Column.ColumnName.ToUpper();
            //Tấm: tính khi khổ giấy thay đổi hoặc định lượng thay đổi
            //Thùng: tính khi diện tích thay đổi hoặc định lượng thay đổi
            if (f.EndsWith("_DL"))
            {
                decimal dt;
                if (e.Row["Loai"].ToString() == "Thùng")
                    dt = Convert.ToDecimal(e.Row["DienTich"]);
                else
                    dt = Convert.ToDecimal(e.Row["DaiG"]) * Convert.ToDecimal(e.Row["RongG"]) / 10000;
                decimal hs = f.StartsWith("M") ? 1M : 1.5M;
                ////Bổ sung điều kiện a = khổ max / giấy rộng
                //decimal a = Convert.ToDecimal(e.Row["KhoMax"]) / Convert.ToDecimal(e.Row["RongG"]);
                //a = decimal.ToInt32(a);
                decimal yc = 0;
                //if (a == Convert.ToDecimal(e.Row["Dao"])
                //   || (a != Convert.ToDecimal(e.Row["Dao"]) && Convert.ToDecimal(e.Row["KhoMax"]) < Convert.ToDecimal(e.Row["KhoTT"])))
                yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * Convert.ToDecimal(e.Row["Dao"]) / 1000;
                //else
                //    yc = hs * Convert.ToDecimal(e.Row[f]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * a / 1000;

                string fn = f.Replace("_DL", "_YC");
                e.Row[fn] = yc;
            }
            if (f == "DIENTICH" || f == "SLPHOI" || f == "DAO")
            {
                foreach (string s in lstNL)
                {
                    //Kiểm tra định lượng rỗng, nếu rỗng thì không tính yc.
                    if (e.Row[s + "DL"] == DBNull.Value)
                        continue;
                    decimal dt;
                    if (e.Row["Loai"].ToString() == "Thùng")
                        dt = Convert.ToDecimal(e.Row["DienTich"]);
                    else
                        dt = Convert.ToDecimal(e.Row["DaiG"]) * Convert.ToDecimal(e.Row["RongG"]) / 10000;
                    decimal hs = s.StartsWith("M") ? 1M : 1.5M;
                    decimal yc = 0;
                    //Bổ sung điều kiện a = khổ max / giấy rộng
                    //decimal a = Convert.ToDecimal(e.Row["KhoMax"]) / Convert.ToDecimal(e.Row["RongG"]);
                    //a = decimal.ToInt32(a);
                    //if (a == Convert.ToDecimal(e.Row["Dao"])
                    //    || (a != Convert.ToDecimal(e.Row["Dao"]) && Convert.ToDecimal(e.Row["KhoMax"]) < Convert.ToDecimal(e.Row["KhoTT"])))
                    yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * Convert.ToDecimal(e.Row["Dao"]) / 1000;
                    //else
                    //    yc = hs * Convert.ToDecimal(e.Row[s + "DL"]) * dt * Convert.ToDecimal(e.Row["SLPhoi"]) * a / 1000;
                    string fn = s + "YC";
                    e.Row[fn] = yc;
                }
            }
        }
        

        //NHẢY KHỔ CỦA LOẠI GIẤY
        
        //TÍNH KHỔ MAX
        private void TinhKhoMax(string tableName,DataColumnChangeEventArgs e)
        {
            string f = e.Column.ColumnName.ToUpper();
            //Tấm: chỉ tính khi khổ giấy thay đổi, đối với đơn hàng và lệnh sản xuất
            //Thùng: chỉ tính khi khổ giấy thay đổi, đối với báo giá, đơn hàng, lệnh sản xuất
            if (f.EndsWith("_KHO") && (!tableName.Equals("MTBAOGIA") || (tableName.Equals("MTBAOGIA") && e.Row["Loai"].Equals("Thùng"))))
            {
                decimal tmp = 0;
                foreach (string s in lstNL)
                {
                    decimal sk = e.Row[s + "Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row[s + "Kho"].ToString());
                    if (sk > tmp)
                        tmp = sk;
                }
                e.Row["KhoMax"] = tmp;
            }
        }

        //TÍNH THÔNG SỐ KỸ THUẬT
        //Báo giá: loại thùng
        //Khi khổ max thay đổi.
        //Đơn hàng: cả tấm và thùng
        //Khi khổ max thay đổi.
        //Lệnh sản xuất: cả tấm và thùng
        //Khi khổ max thay đổi.
        private void TinhTSKThuat(string tableName,DataColumnChangeEventArgs e)
        {
            string f = e.Column.ColumnName.ToUpper();
            
            //if (e.Row["Loai"].Equals("Tấm") && !tableName.Equals("MTBAOGIA") && (f.EndsWith("_DL") || f.EndsWith("_DG")
            //   || f == "KCT" || f == "CPK" || f == "CK" || f == "DAI" || f == "RONG" || f == "LOAI" || f == "LOP" || f == "MAKH" || f == "KHOMAX"))
            if (e.Row["Loai"].Equals("Tấm") && !tableName.Equals("MTBAOGIA") && (f == "KHOMAX" || f == "DAIG" || f == "RONGG"))
            {
                int lop = Int32.Parse(e.Row["Lop"].ToString());
                decimal giay_rong = 0;
                decimal giay_dai = 0;
                if (f != "DAIG" && f != "RONGG" && f != "KHOMAX") //Cột thay đổi là dài, rộng, cao. Gắn giá trị cho cột đang thay đổi sẽ lỗi
                {
                    //tinh dien tich
                    decimal dai = e.Row["Dai"].ToString() == "" ? 0 : decimal.Parse(e.Row["Dai"].ToString());
                    decimal rong = e.Row["Rong"].ToString() == "" ? 0 : decimal.Parse(e.Row["Rong"].ToString());
                    giay_rong = rong;
                    giay_dai = dai;
                    e.Row["DaiG"] = giay_dai;
                    e.Row["RongG"] = giay_rong;
                }
                else
                {
                    giay_dai = e.Row["DaiG"].ToString() =="" ? 0: decimal.Parse(e.Row["DaiG"].ToString());
                    giay_rong = e.Row["RongG"].ToString() == ""? 0: decimal.Parse(e.Row["RongG"].ToString());
                }
                e.Row["DienTich"] = giay_dai * giay_rong / 10000;
                //tinh dao
                if (giay_rong != 0)
                {
                    decimal kmax = e.Row["KhoMax"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["KhoMax"]);
                    //decimal t = Math.Round(kmax / giay_rong, 1);
                    decimal t = kmax / giay_rong;

                    decimal dao = 0;
                    if (t >= 1 && t < 2)
                        dao = 1;
                    if (t >= 2 && t < 3)
                        dao = 2;
                    if (t >= 3 && t < 4)
                        dao = 3;
                    if (t >= 4 && t < 5)
                        dao = 4;
                    if (t >= 5 && t < 6)
                        dao = 5;
                    if (t >= 6 && t < 7)
                        dao = 6;
                    if (t >= 7 && t < 8)
                        dao = 7;
                    if (t >= 8 && t < 9)
                        dao = 8;
                    if (t >= 9 && t < 10)
                        dao = 9;
                    if (t >= 10 && t < 11)
                        dao = 10;
                    if (t >= 11 && t < 12)
                        dao = 11;
                    if (t >= 12 && t < 13)
                        dao = 12;
                    if (t >= 13 && t < 14)
                        dao = 13;
                    if (t >= 14 && t < 15)
                        dao = 14;
                    if (t >= 15 && t < 16)
                        dao = 15;
                    e.Row["Dao"] = dao;
                    //tinh kho tt
                    decimal ktt = giay_rong * dao;
                    if (lop == 3)
                        ktt += 2;
                    else
                        ktt += 3;
                    e.Row["KhoTT"] = Math.Round(ktt);
                }
            }
            else if (!e.Row["Loai"].Equals("Tấm") && (f == "DAI" || f == "RONG" || f == "CAO" || f == "KHOMAX" || f == "DAIG" || f == "RONGG"))
            //else if (!e.Row["Loai"].Equals("Tấm") && f == "KHOMAX")
            {
                int lop = Int32.Parse(e.Row["Lop"].ToString());
                string lt = e.Row["LoaiThung"].ToString();
                decimal giay_rong = 0, giay_dai = 0;
                if (f != "DAIG" && f != "RONGG" && f!= "KHOMAX") //Cột thay đổi là dài, rộng, cao. Gắn giá trị cho cột đang thay đổi sẽ lỗi
                {
                    //tinh dien tich
                    decimal dai = e.Row["Dai"].ToString() == "" ? 0 : decimal.Parse(e.Row["Dai"].ToString());
                    decimal rong = e.Row["Rong"].ToString() == "" ? 0 : decimal.Parse(e.Row["Rong"].ToString());
                    decimal cao = e.Row["Cao"].ToString() == "" ? 0 : decimal.Parse(e.Row["Cao"].ToString());
                    decimal t1 = 0;
                    decimal t2 = 3.5M;
                    giay_rong = rong + cao + t1;
                    giay_dai = (dai + rong) * 2 + t2;

                    e.Row["DaiG"] = giay_dai;
                    e.Row["RongG"] = giay_rong;
                }
                else
                {
                    giay_dai = decimal.Parse(e.Row["DaiG"].ToString() == "" ? "0" : e.Row["DaiG"].ToString());
                    giay_rong = decimal.Parse(e.Row["RongG"].ToString() == "" ? "0" : e.Row["RongG"].ToString());
                }
                    
                decimal t3 = lop == 3 ? 5 : 6;
                decimal dt = giay_rong >= 200 ? (giay_dai + t3) * giay_rong / 10000 : giay_dai * giay_rong / 10000;
                e.Row["DienTich"] = dt;
                //tinh dao
                decimal dao = 0;
                if (giay_rong != 0)
                {
                    decimal kmax = e.Row["KhoMax"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["KhoMax"]);
                    //decimal t = Math.Round(kmax / giay_rong, 1);
                    decimal t = kmax / giay_rong;
                    if (t >= 1 && t < 2)
                        dao = 1;
                    if (t >= 2 && t < 3)
                        dao = 2;
                    if (t >= 3 && t < 4)
                        dao = 3;
                    if (t >= 4 && t < 5)
                        dao = 4;
                    if (t >= 5 && t < 6)
                        dao = 5;
                    if (t >= 6 && t < 7)
                        dao = 6;
                    if (t >= 7 && t < 8)
                        dao = 7;
                    if (t >= 8 && t < 9)
                        dao = 8;
                    if (t >= 9 && t < 10)
                        dao = 9;
                    if (t >= 10 && t < 11)
                        dao = 10;
                    if (t >= 11 && t < 12)
                        dao = 11;
                    if (t >= 12 && t < 13)
                        dao = 12;
                    if (t >= 13 && t < 14)
                        dao = 13;
                    if (t >= 14 && t < 15)
                        dao = 14;
                    if (t >= 15 && t < 16)
                        dao = 15;
                }
                
                //tinh kho tt
                decimal ktt = giay_rong * dao;
                if (lop == 3)
                    ktt += 2;
                else
                    ktt += 3;

                e.Row["Dao"] = dao;
                e.Row["KhoTT"] = Math.Round(ktt);
            }
            //Tính số lượng phôi
            //if(tableName == "MTLSX")
            //{
                
            //    if (e.Column.ColumnName.ToUpper() == "RONGG" || e.Column.ColumnName.ToUpper() == "SLSX")
            //    {
            //        if (e.Row["Loai"].ToString() == "Thùng")
            //        {
            //            if (Convert.ToDecimal(e.Row["Rong"]) + (e.Row["Cao"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["Cao"])) 
            //                <= Convert.ToDecimal(e.Row["RongG"]))
            //            {
            //                e.Row["SLPhoi"] = Convert.ToDecimal(e.Row["SLSX"]) / (Convert.ToDecimal(e.Row["Dao"]) == 0 ? 1 : Convert.ToDecimal(e.Row["Dao"]));
            //            }
            //            else
            //            {
            //                decimal y = Math.Round((Convert.ToDecimal(e.Row["Rong"]) + (e.Row["Cao"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["Cao"]))) /
            //                    (Convert.ToDecimal(e.Row["RongG"]) == 0 ? 1 : Convert.ToDecimal(e.Row["RongG"])), 0);
            //                if (y == 1)
            //                    e.Row["SLPhoi"] = Convert.ToDecimal(e.Row["SLSX"]) / (Convert.ToDecimal(e.Row["Dao"]) == 0 ? 1 : Convert.ToDecimal(e.Row["Dao"]));
            //                else
            //                    e.Row["SLPhoi"] = Convert.ToDecimal(e.Row["SLSX"]) / y;
            //            }
            //        }
            //        else
            //            e.Row["SLPhoi"] = Convert.ToDecimal(e.Row["SLSX"]) / (Convert.ToDecimal(e.Row["Dao"]) == 0 ? 1 : Convert.ToDecimal(e.Row["Dao"]));
            //    }
            //}
        }

        //TÍNH GIÁ BÁN
        //Báo giá
        //Điều kiện: Có khi kct = 0
        //- Khi đổi: DL, DG, Lớp, Loại, MaKH, CPK ,CK, KCT
        //Đơn hàng
        //Điều kiện: Có khi kct = 0
        //- Khi đổi:Số lượng, DL, Khổ, DG, Lớp, Loại, MaKH, CPK ,CK, KCT, Số màu, Dài, Rộng, Cao, Độ khó, Độ phủ, Khổ max
        private void TinhGiaBan(string tableName,bool kct, DataColumnChangeEventArgs e)
        {
            string f = e.Column.ColumnName.ToUpper();
            int lop = Int32.Parse(e.Row["Lop"].ToString());
            //dinh luong 7 lop
            decimal mdl = e.Row["Mat_DL"].ToString() == "" ? 0 : decimal.Parse(e.Row["Mat_DL"].ToString());
            decimal sbdl = e.Row["SB_DL"].ToString() == "" ? 0 : decimal.Parse(e.Row["SB_DL"].ToString());
            decimal mbdl = e.Row["MB_DL"].ToString() == "" ? 0 : decimal.Parse(e.Row["MB_DL"].ToString());
            decimal scdl = e.Row["SC_DL"].ToString() == "" ? 0 : decimal.Parse(e.Row["SC_DL"].ToString());
            decimal mcdl = e.Row["MC_DL"].ToString() == "" ? 0 : decimal.Parse(e.Row["MC_DL"].ToString());
            decimal sedl = e.Row["SE_DL"].ToString() == "" ? 0 : decimal.Parse(e.Row["SE_DL"].ToString());
            decimal medl = e.Row["ME_DL"].ToString() == "" ? 0 : decimal.Parse(e.Row["ME_DL"].ToString());

            //kho 7 lop
            decimal mk = e.Row["Mat_Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row["Mat_Kho"].ToString());
            decimal sbk = e.Row["SB_Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row["SB_Kho"].ToString());
            decimal mbk = e.Row["MB_Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row["MB_Kho"].ToString());
            decimal sck = e.Row["SC_Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row["SC_Kho"].ToString());
            decimal mck = e.Row["MC_Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row["MC_Kho"].ToString());
            decimal sek = e.Row["SE_Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row["SE_Kho"].ToString());
            decimal mek = e.Row["ME_Kho"].ToString() == "" ? 0 : decimal.Parse(e.Row["ME_Kho"].ToString());

            //don gia kg 7 lop
            decimal mdg = e.Row["Mat_DG"].ToString() == "" ? 0 : decimal.Parse(e.Row["Mat_DG"].ToString());
            decimal sbdg = e.Row["SB_DG"].ToString() == "" ? 0 : decimal.Parse(e.Row["SB_DG"].ToString());
            decimal mbdg = e.Row["MB_DG"].ToString() == "" ? 0 : decimal.Parse(e.Row["MB_DG"].ToString());
            decimal scdg = e.Row["SC_DG"].ToString() == "" ? 0 : decimal.Parse(e.Row["SC_DG"].ToString());
            decimal mcdg = e.Row["MC_DG"].ToString() == "" ? 0 : decimal.Parse(e.Row["MC_DG"].ToString());
            decimal sedg = e.Row["SE_DG"].ToString() == "" ? 0 : decimal.Parse(e.Row["SE_DG"].ToString());
            decimal medg = e.Row["ME_DG"].ToString() == "" ? 0 : decimal.Parse(e.Row["ME_DG"].ToString());
            
//TẤM
            if (bool.Parse(e.Row["KCT"].ToString()) == false && e.Row["Loai"].Equals("Tấm") && !tableName.Equals("MTLSX") && (f.EndsWith("_DL") || f.EndsWith("_DG") || f == "KCT" || f == "CPK" || f == "CK" || f == "LOAI" || f == "LOP" || f == "MAKH"))
            {
                 //tinh tong don gia giay
                decimal g = (mdl * mdg) / 1000
                            + 1.5M * (sbdl * sbdg) / 1000 + (mbdl * mbdg) / 1000
                            + 1.5M * (scdl * scdg) / 1000 + (mcdl * mcdg) / 1000
                            + 1.5M * (sedl * sedg) / 1000 + (medl * medg) / 1000;

                //tinh chi phi gian tiep
                object o = db.GetValue("select sum(Tien * HSLop) from CHChiPhi where Lop = '" + lop.ToString() + "'");
                decimal gt = (o == null || o.ToString() == "") ? 0 : decimal.Parse(o.ToString());

                //ty le hao hut
                o = Config.GetValue("HH" + lop.ToString());
                decimal hh = (o == null || o.ToString() == "") ? 0 : decimal.Parse(o.ToString());

                //ty le loi nhuan
                o = Config.GetValue("LN" + lop.ToString());
                decimal ln = (o == null || o.ToString() == "") ? 0 : decimal.Parse(o.ToString());

                //chi phi khac
                decimal cpk = (e.Row["CPK"].ToString() == "") ? 0 : decimal.Parse(e.Row["CPK"].ToString());
                //chiet khau
                decimal ck = (e.Row["CK"].ToString() == "") ? 0 : decimal.Parse(e.Row["CK"].ToString());

                //tong gia ban
                decimal gb = (g + gt) * (100 + hh + ln) / 100 + cpk - ck;
                decimal hoahong = LayHoaHong();
                gb = gb / ((100 - hoahong) / 100);
                e.Row["GiaBan"] = gb;
            }
//THÙNG
            else if (bool.Parse(e.Row["KCT"].ToString()) == false && e.Row["Loai"].Equals("Thùng") && !tableName.Equals("MTLSX") && 
                    (f.EndsWith("_DL") || f.EndsWith("_KHO") || f.EndsWith("_DG") || f == "KCT" || f == "CPK" || f == "CK" || f == "LOAI" || f == "LOP" || f == "MAKH"
                    || f == "SOLUONG" || f == "SOMAU" || f == "DAI" || f == "RONG" || f == "CAO" || f == "DOKHO" || f == "DOPHU" || f == "KHOMAX"))
            {
                decimal dt = decimal.Parse(e.Row["DienTich"].ToString() == "" ? "0" : e.Row["DienTich"].ToString());
                decimal giay_dai = decimal.Parse(e.Row["DaiG"].ToString() == "" ? "0" : e.Row["DaiG"].ToString());
                decimal giay_rong = decimal.Parse(e.Row["RongG"].ToString() == "" ? "0" : e.Row["RongG"].ToString());
                decimal dao = decimal.Parse(e.Row["Dao"].ToString() == "" ? "0" : e.Row["Dao"].ToString());
                decimal ktt = decimal.Parse(e.Row["KhoTT"].ToString() == "" ? "0" : e.Row["KhoTT"].ToString());
                //tinh tong don gia giay
                decimal g = (mdl * mdg) * dt / 1000
                            + 1.5M * (sbdl * sbdg) * dt / 1000 + (mbdl * mbdg) * dt / 1000
                            + 1.5M * (scdl * scdg) * dt / 1000 + (mcdl * mcdg) * dt / 1000
                            + 1.5M * (sedl * sedg) * dt / 1000 + (medl * medg) * dt / 1000;

                //tinh chi phi gian tiep
                object o = db.GetValue("select sum(Tien) from CHChiPhi where Lop = '" + lop.ToString() + "'");
                decimal gt = (o == null || o.ToString() == "") ? 0 : decimal.Parse(o.ToString());
                gt = giay_dai >= 200 ? dt * gt * 1.2M : dt * gt;
                decimal sl;
                if(tableName == "MTBaoGia")
                    sl = e.Row["SoLuong"].ToString() == "" ? 0 : decimal.Parse(e.Row["SoLuong"].ToString());
                else
                    sl = e.Row["SLBG"].ToString() == "" ? 0 : decimal.Parse(e.Row["SLBG"].ToString());
                //tinh chi phi hao hut
                o = db.GetValue("select HaoHut from CHHaoHut where Lop = '" + lop.ToString() + "' and " + sl.ToString().Replace(",", ".") + " between TuSL and DenSL");
                decimal tlhh         = (o == null || o.ToString() == "") ? 0 : decimal.Parse(o.ToString());
                decimal hh = tlhh * (g + gt);

                //tinh lai
                decimal l = (gt + g) * 0.06M;
                //tinh so mau
                decimal m = e.Row["SoMau"].ToString() == "" ? 0 : decimal.Parse(e.Row["SoMau"].ToString());
                m = (m == 0) ? 0 : (dt * 200) + ((m - 1) * 50 * dt);
                //tinh do phu
                decimal dp = Boolean.Parse(e.Row["DoPhu"].ToString()) ? 100 : 0;
                //tinh do kho
                decimal dk = Boolean.Parse(e.Row["DoKho"].ToString()) ? (g + hh) * 0.02M : 0;

                //gia ban chua co lai lo
                decimal gb = g + hh + gt + l + m + dp + dk;
                
                //tinh lai lo
                if (dao == 0)
                {
                    
                    List<string> lstTam = new List<string>(new string[] { "CAO", "RONG", "MAT_KHO"});
                    if (!tableName.Equals("MTLSX") || (tableName.Equals("MTLSX") && downHitInfo != null)) 
                        if (e.Row["KhoMax"] != DBNull.Value && Convert.ToInt32(e.Row["RongG"]) != 0 && lstTam.Contains(f)
                            && ((!tableName.Equals("MTLSX")) || (tableName.Equals("MTLSX") && downHitInfo.InRow)))
                            XtraMessageBox.Show("Khổ giấy chưa đúng!");
                }
                else
                {
                    decimal ld = (giay_dai >= 200) ? giay_dai + 6 : giay_dai;
                    decimal glo = ((mk - ktt) * ld / 10000) * ((mdl * mdg) / 1000)
                                + 1.5M * ((sbk - ktt) * ld / 10000) * (sbdl * sbdg) / 1000 + ((mbk - ktt) * ld / 10000) * (mbdl * mbdg) / 1000
                                + 1.5M * ((sck - ktt) * ld / 10000) * (scdl * scdg) / 1000 + ((mck - ktt) * ld / 10000) * (mcdl * mcdg) / 1000
                                + 1.5M * ((sek - ktt) * ld / 10000) * (sedl * sedg) / 1000 + ((mek - ktt) * ld / 10000) * (medl * medg) / 1000;
                    glo = glo / dao;
                    decimal kmax = e.Row["KhoMax"] == DBNull.Value ? 0 : Convert.ToDecimal(e.Row["KhoMax"]);
                    decimal t = kmax / giay_rong;

                    decimal lk = 0;
                    if (t >= 1 && t < 2)
                        lk = giay_dai * 0;
                    if (t >= 2 && t < 3)
                        lk = giay_dai * 1.6M;
                    if (t >= 3 && t < 4)
                        lk = giay_dai * 3.2M;
                    if (t >= 4)
                        lk = giay_dai * 4.8M;
                    lk = lk / 10000 / dao;
                    decimal glai = lk * ((mdl * mdg) / 1000)
                                + 1.5M * lk * (sbdl * sbdg) / 1000 + lk * (mbdl * mbdg) / 1000
                                + 1.5M * lk * (scdl * scdg) / 1000 + lk * (mcdl * mcdg) / 1000
                                + 1.5M * lk * (sedl * sedg) / 1000 + lk * (medl * medg) / 1000;
                    glai = glai / dao;
                    decimal lailo = glo - glai;
                    gb = lailo > 0 ? gb + lailo : gb;

                    //chi phi khac
                    decimal cpk = (e.Row["CPK"].ToString() == "") ? 0 : decimal.Parse(e.Row["CPK"].ToString());
                    //chiet khau
                    decimal ck = (e.Row["CK"].ToString() == "") ? 0 : decimal.Parse(e.Row["CK"].ToString());

                    //tong gia ban
                    gb = gb + cpk - ck;
                    decimal hoahong = LayHoaHong();
                    gb = gb / ((100 - hoahong) / 100);
                    e.Row["GiaBan"] = gb;
                }
            }
            
        }

        //TÍNH THÀNH TIỀN
        //Đơn hàng: tấm
        //- Khi đổi: SLDH, Giá bán, dài, rộng
        //Đơn hàng: thùng
        //- Khi đổi: SLDH, Giá bán
        private void TinhTTien(string tableName, DataColumnChangeEventArgs e)
        {
            string f = e.Column.ColumnName.ToUpper();
            if (tableName == "MTDonHang" && e.Row["Loai"].Equals("Tấm") && (f == "GIABAN" || f == "SOLUONG" || f == "DAI" || f == "RONG"))
            {
                decimal gb = e.Row["GiaBan"] == DBNull.Value ? 0 : (decimal)e.Row["GiaBan"];
                decimal sl = e.Row["SoLuong"] == DBNull.Value ? 0 : (decimal)e.Row["SoLuong"];
                decimal d = e.Row["Dai"] == DBNull.Value ? 0 : (decimal)e.Row["Dai"];
                decimal r = e.Row["Rong"] == DBNull.Value ? 0 : (decimal)e.Row["Rong"];
                e.Row["SL2"] = Math.Round(sl * d * r / 10000, 0);
                e.Row["ThanhTien"] = Math.Round(sl * gb * d * r / 10000, 0);          
            }
            else if (tableName == "MTDonHang" && !e.Row["Loai"].Equals("Tấm") && (f == "GIABAN" || f == "SOLUONG"))
            {
                decimal gb = e.Row["GiaBan"] == DBNull.Value ? 0 : (decimal)e.Row["GiaBan"];
                decimal sl = e.Row["SoLuong"] == DBNull.Value ? 0 : (decimal)e.Row["SoLuong"];
                e.Row["ThanhTien"] = sl * gb;
            }
        }

    }
}
