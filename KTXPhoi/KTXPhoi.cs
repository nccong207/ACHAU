using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace KTXPhoi
{
    public class KTXPhoi:ICData
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
            CapNhatDaSX();
            CapNhatDaSX_LSX();
        }
        public void ExecuteBefore()
        {
            //Không được xóa khi đã nhập slht trong khsx
            if (_data.CurMasterIndex < 0)
                return;
            string sql  = @"select	isnull(sum(kh.slsx),0) [flag]
                            from	dtkh kh inner join dtlsx l on kh.dtlsxid = l.dtlsxid
				                            inner join mtlsx m on m.mtlsxid = l.mtlsxid
                            where	l.dtdhid = '{0}' and m.solsx = '{1}'";
            DataTable dt = _data.DsData.Tables[1].GetChanges(DataRowState.Deleted);
            if (dt == null)
                return;
            foreach (DataRow dr in dt.Rows)
            {
                object obj = _data.DbData.GetValue(string.Format(sql, dr["DTDHID", DataRowVersion.Original], dr["SoLSX", DataRowVersion.Original]));
                if (Convert.ToInt32(obj) > 0)
                {
                    XtraMessageBox.Show(string.Format("Mặt hàng '{0}' đã nhập SLHT, không được xóa!", dr["TenHang", DataRowVersion.Original])
                             , Config.GetValue("PackageName").ToString());
                    _info.Result = false;
                    break;
                }
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion

        //Update cho đơn hàng
        private void CapNhatDaSX()
        {
            string sqldh = "";
            DataTable dt = _data.DsData.Tables[1];

            //DataRow[] dtcopy = _data.DsDataCopy.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            //DataRow[] dtChange = _data.DsData.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            DataTable dtChange = _data.DsData.Tables[1].GetChanges();
            if (dtChange == null)
                return;
            foreach (DataRow dr in dtChange.Rows)
            {
                switch (dr.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                        if (dr["Loai"].ToString().Equals("Thùng"))
                            continue;
                        object oSoLuong = _data.DbData.GetValue("select sum(SLDat) from dtlsx where dtdhid = '" + dr["DTDHID"] + "'");
                        object oSLXuat = _data.DbData.GetValue("select sum(soluong) from dtxphoi where dtdhid = '" + dr["DTDHID"] + "'");
                        //object oSLDat = dt.Compute("SLDat", "DTDHID = '" + dr["DTDHID"] + "'");
                        if (oSoLuong == DBNull.Value || oSLXuat == DBNull.Value)
                            continue;
                        if (Convert.ToDecimal(oSLXuat) >= Convert.ToDecimal(oSoLuong))
                            sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 1, dr["DTDHID"]);
                        else
                            sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 0, dr["DTDHID"]);
                        break;
                    case DataRowState.Deleted:
                        if (dr["Loai", DataRowVersion.Original].ToString().Equals("Thùng"))
                            continue;
                        object oSoLuong1 = _data.DbData.GetValue("select sum(SLDat) from dtlsx where dtdhid = '" + dr["DTDHID", DataRowVersion.Original] + "'");
                        object oSLXuat1 = _data.DbData.GetValue("select sum(soluong) from dtxphoi where dtdhid = '" + dr["DTDHID", DataRowVersion.Original] + "'");
                        if (Convert.ToDecimal(oSLXuat1 == DBNull.Value ? 0 : oSLXuat1) >= Convert.ToDecimal(oSoLuong1 == DBNull.Value ? 0 : oSoLuong1) && oSLXuat1 != DBNull.Value)
                            sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 1, dr["DTDHID", DataRowVersion.Original]);
                        else
                            sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 0, dr["DTDHID", DataRowVersion.Original]);
                        break;
                }
            }

            if (sqldh != "")
                _data.DbData.UpdateByNonQuery(sqldh);
        }

        //Cập nhập lệnh sản xuất
        private void CapNhatDaSX_LSX()
        {
            string sqldh = "";
            DataTable dt = _data.DsData.Tables[1];

            //DataRow[] dtcopy = _data.DsDataCopy.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            //DataRow[] dtChange = _data.DsData.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            DataTable dtChange = _data.DsData.Tables[1].GetChanges();
            if (dtChange == null)
                return;
            foreach (DataRow dr in dtChange.Rows)
            {
                switch (dr.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                        if (dr["Loai"].ToString().Equals("Thùng") || string.IsNullOrEmpty(dr["SoLSX"].ToString()))
                            continue;
                        object oSLPhoi = _data.DbData.GetValue(@"select sum(d.SLDat) from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                                 where d.dtdhid = '" + dr["DTDHID"] + "' and m.solsx ='" + dr["solsx"] + "'");
                        object oSoLuong = _data.DbData.GetValue(@"select sum(soluong) from dtxphoi
                                                                  where dtdhid = '" + dr["DTDHID"] + "' and solsx ='" + dr["solsx"] + "'");
                        if (oSoLuong == DBNull.Value || oSLPhoi == DBNull.Value)
                            continue;
                        if (Convert.ToDecimal(oSoLuong) >= Convert.ToDecimal(oSLPhoi))
                            sqldh += string.Format(@";update dtlsx set dasx = {0} 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid 
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", 1, dr["solsx"],dr["dtdhid"]);
                        else
                            sqldh += string.Format(@";update dtlsx set dasx = {0} 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", 0, dr["solsx"],dr["DTDHID"]);
                        break;
                    case DataRowState.Deleted:
                        if (dr["Loai", DataRowVersion.Original].ToString().Equals("Thùng") || string.IsNullOrEmpty(dr["SoLSX", DataRowVersion.Original].ToString()))
                            continue;
                        object oSLPhoi1 = _data.DbData.GetValue(@"select sum(d.SLDat) from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                                 where d.dtdhid = '" + dr["DTDHID",DataRowVersion.Original] + "' and m.solsx ='" + dr["solsx",DataRowVersion.Original] + "'");
                        object oSoLuong1 = _data.DbData.GetValue(@"select sum(soluong) from dtxphoi
                                                                  where dtdhid = '" + dr["DTDHID",DataRowVersion.Original] + "' and solsx ='" + dr["solsx",DataRowVersion.Original] + "'");
                        if (Convert.ToDecimal(oSoLuong1 == DBNull.Value ? 0 : oSoLuong1) >= Convert.ToDecimal(oSLPhoi1 == DBNull.Value ? 0 : oSLPhoi1))
                            sqldh += string.Format(@";update dtlsx set dasx = {0} 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid 
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", 1, dr["solsx",DataRowVersion.Original], dr["dtdhid",DataRowVersion.Original]);
                        else
                            sqldh += string.Format(@";update dtlsx set dasx = {0} 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", 0, dr["solsx",DataRowVersion.Original], dr["DTDHID",DataRowVersion.Original]);
                        break;
                }
            }
            if (sqldh != "")
                _data.DbData.UpdateByNonQuery(sqldh);
        }
    }
}
