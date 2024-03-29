using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace KTraXuatAm
{
    public class KTraXuatAm : ICData
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
            string sql = @"select sum(soluong - soluong_x) from BLVT 
                        where Loi = {3} and DTDHID = '{0}' and NgayCT <= '{1}' and MTIDDT <> '{2}' and ViTri {4}";
            foreach (DataRowView drv in dv)
            {
                string dtid = drv["DT32ID"].ToString();
                string dtdhid = drv["DTDHID"].ToString();
                string mahh = drv["MaHH"].ToString();
                string ngayct = drCur["NgayCT"].ToString();
                string oVT = drv["ViTri"].ToString();
                string vitri = oVT == string.Empty ? "is null" : string.Format("= N'{0}'", oVT);
                int loi = Boolean.Parse(drv["Loi"].ToString()) ? 1 : 0;
                object o = _data.DbData.GetValue(string.Format(sql, dtdhid, ngayct, dtid, loi, vitri));
                decimal slt = o == DBNull.Value ? 0 : decimal.Parse(o.ToString());
                decimal slx = decimal.Parse(drv["SoLuong"].ToString());
                if (slx > slt)
                {
                    XtraMessageBox.Show("Không được xuất vượt quá số lượng tồn (theo vị trí)\n" +
                        "Vị trí = " + oVT + "; Số lượng xuất = " + slx.ToString("###,##0") + "; Số lượng tồn = " + slt.ToString("###,##0"),
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
