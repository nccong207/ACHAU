using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace KTNPhoi
{
    public class KTNPhoi:ICData
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
            CapNhatDaNhapPhoi_LSX();
        }
        public void ExecuteBefore()
        {

        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion

        //Cập nhập lệnh sản xuất
        private void CapNhatDaNhapPhoi_LSX()
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
                object slPNhap = 0;
                object dtdhid = "", solsx = "";
                switch (dr.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                        if (dr["Loai"].ToString().Equals("Tấm"))
                            continue;
                        dtdhid = dr["DTDHID"];
                        solsx = dr["solsx"];
                        object oSLDat = _data.DbData.GetValue(@"select sum(d.SLDat) from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                                 where d.dtdhid = '" + dtdhid + "' and m.solsx ='" + solsx + "'");
                        object oSLNhap = _data.DbData.GetValue(@"select sum(soluong) from dtnphoi
                                                                  where dtdhid = '" + dtdhid + "' and solsx ='" + solsx + "'");
                        if (oSLNhap == DBNull.Value || oSLDat == DBNull.Value)
                            continue;
                        slPNhap = oSLNhap;
                        if (Convert.ToDecimal(oSLDat) > Convert.ToDecimal(oSLNhap))
                            sqldh += string.Format(@";update dtlsx set TinhTrangNP = N'{0}' 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid 
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", "Chưa đủ", solsx, dtdhid);
                        else
                            sqldh += string.Format(@";update dtlsx set TinhTrangNP = N'{0}' 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", "Nhập đủ", solsx, dtdhid);
                        break;
                    case DataRowState.Deleted:
                        if (dr["Loai", DataRowVersion.Original].ToString().Equals("Tấm"))
                            continue;
                        dtdhid = dr["DTDHID", DataRowVersion.Original];
                        solsx = dr["solsx", DataRowVersion.Original];
                        object oSLDat1 = _data.DbData.GetValue(@"select sum(d.SLDat) from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                                 where d.dtdhid = '" + dtdhid + "' and m.solsx ='" + solsx + "'");
                        object oSLNhap1 = _data.DbData.GetValue(@"select sum(soluong) from dtnphoi
                                                                  where dtdhid = '" + dtdhid + "' and solsx ='" + solsx + "'");
                        slPNhap = oSLNhap1 == null || oSLNhap1 == DBNull.Value ? 0 : oSLNhap1;
                        if (Convert.ToDecimal(oSLDat1 == DBNull.Value ? 0 : oSLDat1) > Convert.ToDecimal(oSLNhap1 == DBNull.Value ? 0 : oSLNhap1))
                            sqldh += string.Format(@";update dtlsx set TinhTrangNP = N'{0}' 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid 
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'",
                                                                                                 Convert.ToDecimal(oSLNhap1 == DBNull.Value ? 0 : oSLNhap1) == 0 ? string.Empty : "Chưa đủ",
                                                                                                 solsx, dtdhid);
                        else
                            sqldh += string.Format(@";update dtlsx set TinhTrangNP = N'{0}' 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", "Nhập đủ", solsx, dtdhid);
                        break;
                }
                sqldh += string.Format(@";update dtlsx set SLNhap = {0} 
                                                    from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid 
                                                    where m.solsx = '{1}' and d.dtdhid = '{2}'", slPNhap, solsx, dtdhid);
                sqldh += string.Format(@";update DTKH set SLPNhap = {0} 
                                        from DTKH inner join DTLSX on DTKH.DTLSXID = DTLSX.DTLSXID
                                        where DTLSX.DTDHID = '{1}'", slPNhap, dtdhid);
            }
            if (sqldh != "")
                _data.DbData.UpdateByNonQuery(sqldh);
        }
    }
}
