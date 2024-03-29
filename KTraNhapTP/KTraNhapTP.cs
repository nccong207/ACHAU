using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace KTraNhapTP
{
    public class KTraNhapTP : ICData
    {
        DataCustomData _data;
        InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);
        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            
        }

        public void ExecuteBefore()
        {
            DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
            string sql1 = @"select sum(soluong) from dtxphoi where dtdhid = '{0}'";
            string sql = @"select sum(SLSX) from DTKH where DTLSXID in (select DTLSXID from DTLSX where DTDHID = '{0}')";
            string sql2 = @"select sum(SoLuong) from DT22 where DTDHID = '{0}'";
            string sql3 = @"select sum(SoLuong) from DT22 where DTDHID = '{0}' and DT22ID <> '{1}'";
            string sql21 = @"select sum(SoLuong) from DT22 where DTDHID = '{0}' and ViTri {1}";
            string sql31 = @"select sum(SoLuong) from DT22 where DTDHID = '{0}' and ViTri {2} and DT22ID <> '{1}'";
            string sql4 = @"select sum(SoLuong) from DT32 where DTDHID = '{0}' and ViTri {1}";
            foreach (DataRowView drv in dv)
            {
                string dtdhid = drv["DTDHID"].ToString();
                string mahh = drv["MaHH"].ToString();
                string oVT = drv.Row.RowState == DataRowState.Added ? drv["ViTri"].ToString() : drv.Row["ViTri", DataRowVersion.Original].ToString();
                string vitri = (oVT == string.Empty) ? "is null" : string.Format(" = N'{0}'", oVT);
                object o;
                if(drv["Loai"].ToString().Equals("Thùng")) 
                    o = _data.DbData.GetValue(string.Format(sql, dtdhid));
                else
                    o = _data.DbData.GetValue(string.Format(sql1, dtdhid));
                decimal slt = o == DBNull.Value ? 0 : decimal.Parse(o.ToString());
                decimal tsln = decimal.Parse(drv["SoLuong"].ToString());
                decimal sln = (drv.Row.RowState == DataRowState.Modified && drv.Row["ViTri"].ToString() != drv.Row["ViTri", DataRowVersion.Original].ToString()) ? 0 : tsln;
                object o2, o21;
                if (drv.Row.RowState == DataRowState.Added)
                {
                    o2 = _data.DbData.GetValue(string.Format(sql2, dtdhid));
                    o21 = _data.DbData.GetValue(string.Format(sql21, dtdhid, vitri));
                }
                else
                {
                    o2 = _data.DbData.GetValue(string.Format(sql3, dtdhid, drv["DT22ID"]));
                    o21 = _data.DbData.GetValue(string.Format(sql31, dtdhid, drv["DT22ID"], vitri));
                }
                tsln = tsln + (o2 == DBNull.Value ? 0 : decimal.Parse(o2.ToString()));
                sln = sln + (o21 == DBNull.Value ? 0 : decimal.Parse(o21.ToString()));
                if (tsln > slt)
                {
                    XtraMessageBox.Show("Không được nhập vượt quá số lượng hoàn thành\n" +
                        mahh + ": Số lượng nhập = " + tsln.ToString("###,##0") + "; Số lượng hoàn thành = " + slt.ToString("###,##0"),
                        Config.GetValue("PackageName").ToString());
                    _info.Result = false;
                    return;
                }

                var o4 = _data.DbData.GetValue(string.Format(sql4, dtdhid, vitri));
                var slx = o4 == DBNull.Value ? 0 : Convert.ToDecimal(o4);
                if (sln < slx)
                {
                    XtraMessageBox.Show("Số lượng nhập không thể nhỏ hơn số lượng đã xuất (tồn theo vị trí)\n" +
                        "Vị trí = " + oVT + "; Số lượng nhập = " + sln.ToString("###,##0") + "; Số lượng xuất = " + slx.ToString("###,##0"),
                        Config.GetValue("PackageName").ToString());
                    _info.Result = false;
                    return;
                }
            }
            _info.Result = true;
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
