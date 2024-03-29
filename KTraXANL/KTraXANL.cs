using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace KTraXANL
{
    public class KTraXANL : ICData
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
            string tb = _data.DrTableMaster["TableName"].ToString();
            if (tb != "MT43" && tb != "MT44")
                return;
            if (Config.GetValue("XuatAmNL").ToString() == "1") return;
            DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return; 
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
            string sql = @"select sum(soluong - soluong_x) from BLNL
                        where DT42ID = '{0}' and NgayCT <= '{1}' and MTIDDT <> '{2}'";
            string pk = _data.DrTable["Pk"].ToString();
            foreach (DataRowView drv in dv)
            {
                string dtid = drv[pk].ToString();
                string dt42id = drv["DT42ID"].ToString();
                if (dt42id == "")
                    continue;
                string ngayct = drCur["NgayCT"].ToString();
                object o = _data.DbData.GetValue(string.Format(sql, dt42id, ngayct, dtid));
                decimal slt = o == DBNull.Value ? 0 : decimal.Parse(o.ToString());
                decimal slx = decimal.Parse(drv["SoLuong"].ToString());
                if (slx > slt)
                {
                    XtraMessageBox.Show("Không được xuất vượt quá số lượng tồn\n" +
                        drv["MaNL"].ToString() + ": Số lượng xuất = " + slx.ToString("#,##0") + "; Số lượng tồn = " + slt.ToString("#,##0"),
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
