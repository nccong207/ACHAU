using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using CDTLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using CDTDatabase;

namespace NhapNLTuExcel
{
    public class NhapNLTuExcel : ICControl
    {
        Database db = Database.NewDataDatabase();
        GridView gvMain;
        private DataCustomFormControl _data;
        private InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);

        public void AddEvent()
        {
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;

            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnImport = new SimpleButton();
            btnImport.Text = "Nhập từ excel";
            btnImport.Name = "btnImport";
            btnImport.Click += new EventHandler(btnImport_Click);
            LayoutControlItem lci3 = lcMain.AddItem("", btnImport);
            lci3.Name = "cusImport";
        }

        void btnImport_Click(object sender, EventArgs e)
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
                    OleDbDataAdapter myCommand = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", cnn);
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
                    XtraMessageBox.Show("Không tìm thấy dữ liệu nhập kho", Config.GetValue("PackageName").ToString());
                    Cursor.Current = Cursors.Default;
                    return;
                }

                foreach (DataRow row in dtSource.Rows)
                {
                    var maNL = XuLyMaNL(row);
                    gvMain.AddNewRow();
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MaNL"], maNL);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MaCuon"], row[1]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["NgaySX"], row[2]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Ca"], row[3]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLuong"], row[6]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["GhiChu"], row[8]);

                    var tbKH = db.GetDataTable("select MaKH from DMKH where TenTat = N'" + row[7] + "'");
                    if (tbKH.Rows.Count > 0)
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["MaKH"], tbKH.Rows[0][0]);

                    gvMain.UpdateCurrentRow();
                }

                Cursor.Current = Cursors.Default;
            }
        }

        private string XuLyMaNL(DataRow row)
        {
            var loai = row[0].ToString();
            var kho = row[4].ToString();
            var dl = row[5].ToString();
            var maNL = loai + "." + kho + "." + dl;
            var sql = @"if not exists (select * from DMNL where Ma = '{0}')
                        insert into DMNL(Ma, Ten, MaNHOM, Kho, DL)
                        values('{0}', '{1}', '{1}', {2}, {3})";
            db.UpdateByNonQuery(string.Format(sql, maNL, loai, kho, dl));
            return maNL;
        }

        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public InfoCustomControl Info
        {
            get { return _info; }
        }
    }
}
