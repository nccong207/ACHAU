using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace XoaCTLSX
{
    public class XoaCTLSX : ICData
    {
        //chỉ xử lý trường hợp xóa bớt 1 dòng đơn hàng trong chi tiết LSX -> xóa trong đơn hàng và báo giá liên quan
        DataCustomData _data;
        InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);
        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            /*DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;
            DataView dvDt = new DataView(_data.DsData.Tables[1]);
            dvDt.RowStateFilter = DataViewRowState.Deleted;
            if (dvDt.Count == 0)
                return;
            string s1 = @"delete from DTBaoGia where DTBGID in 
                                (select bg.DTBGID from DTBaoGia bg inner join DTDonHang dh
                                on bg.TenHang = dh.TenHang and bg.Dai = dh.Dai and bg.Rong = dh.Rong and bg.Cao = dh.Cao
                                where DTDHID = '{0}')";
            string s2 = "delete from DTDonHang where DTDHID = '{0}'";

            foreach (DataRowView drv in dvDt)
            {
                _data.DbData.UpdateByNonQuery(string.Format(s1, drv["DTDHID"]));
                _data.DbData.UpdateByNonQuery(string.Format(s2, drv["DTDHID"]));
            }*/
        }

        public void ExecuteBefore()
        {
            /*DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;
            DataView dvDt = new DataView(_data.DsData.Tables[1]);
            dvDt.RowStateFilter = DataViewRowState.Deleted;
            if (dvDt.Count == 0)
                return;
            if (XtraMessageBox.Show("Xóa mặt hàng tại đây sẽ đồng thời xóa mặt hàng trong đơn hàng và báo giá liên quan?",
                Config.GetValue("PackageName").ToString(), System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                _info.Result = true;
            else
                _info.Result = false;*/
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
