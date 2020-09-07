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
                switch (dr.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                        if (dr["Loai"].ToString().Equals("Tấm"))
                            continue;
                        object oSLPhoi = _data.DbData.GetValue(@"select sum(d.slsx) from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                                 where d.dtdhid = '" + dr["DTDHID"] + "' and m.solsx ='" + dr["solsx"] + "'");
                        object oSoLuong = _data.DbData.GetValue(@"select sum(soluong) from dtnphoi
                                                                  where dtdhid = '" + dr["DTDHID"] + "' and solsx ='" + dr["solsx"] + "'");
                        if (oSoLuong == DBNull.Value || oSLPhoi == DBNull.Value)
                            continue;
                        if (Convert.ToDecimal(oSLPhoi) >= Convert.ToDecimal(oSoLuong))
                            sqldh += string.Format(@";update dtlsx set DaNhapPhoi = {0} 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid 
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", 1, dr["solsx"],dr["dtdhid"]);
                        else
                            sqldh += string.Format(@";update dtlsx set DaNhapPhoi = {0} 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", 0, dr["solsx"],dr["DTDHID"]);
                        break;
                    case DataRowState.Deleted:
                        if (dr["Loai", DataRowVersion.Original].ToString().Equals("Tấm"))
                            continue;
                        object oSLPhoi1 = _data.DbData.GetValue(@"select sum(d.slsx) from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid
                                                                 where d.dtdhid = '" + dr["DTDHID",DataRowVersion.Original] + "' and m.solsx ='" + dr["solsx",DataRowVersion.Original] + "'");
                        object oSoLuong1 = _data.DbData.GetValue(@"select sum(soluong) from dtnphoi
                                                                  where dtdhid = '" + dr["DTDHID",DataRowVersion.Original] + "' and solsx ='" + dr["solsx",DataRowVersion.Original] + "'");
                        if (Convert.ToDecimal(oSoLuong1 == DBNull.Value ? 0 : oSoLuong1) <= Convert.ToDecimal(oSLPhoi1 == DBNull.Value ? 0 : oSLPhoi1))
                            sqldh += string.Format(@";update dtlsx set DaNhapPhoi = {0} 
                                                      from dtlsx d inner join mtlsx m on d.mtlsxid = m.mtlsxid 
                                                      where m.solsx = '{1}' and d.dtdhid = '{2}'", 1, dr["solsx",DataRowVersion.Original], dr["dtdhid",DataRowVersion.Original]);
                        else
                            sqldh += string.Format(@";update dtlsx set DaNhapPhoi = {0} 
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
