using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace TaoMaHH
{
    public class TaoMaHH : ICData
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
            //Kiểm  tra đơn hàng tấm để đánh dấu đã sx
            //CapNhatDaSX();
            //CapNhatDaSX_LSX();
            //Tạo mã hàng hóa
            DataRow drCur = _data.DsDataCopy.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;
            using (DataView dv = new DataView(_data.DsData.Tables[1]))
            {
//                dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
//                string sql = @"if not exists (select MaHH from DMHH where MaHH = '{0}')
//                                insert into DMHH(MaHH, TenHH, DVT, QuyCach, GiaBan)
//                                values('{0}', N'{1}', N'{2}', '{3}', {4})";
//                foreach (DataRowView drv in dv) 
//                {
//                    string qc = float.Parse(drv["Dai"].ToString()).ToString() + "*" + float.Parse(drv["Rong"].ToString()).ToString() +
//                        (drv["Cao"].ToString() == "" ? "" : "*" + float.Parse(drv["Cao"].ToString()).ToString()) + "_" + drv["Lop"].ToString() + "L";
//                    string mahh = drv["MaKH"].ToString() + "_" + qc;
//                    if (!_data.DbData.UpdateByNonQuery(string.Format(sql, mahh, drv["TenHang"], drv["DVT"], qc, drv["GiaBan"])))
//                        return;
//                }
                dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
                //Insert có giá bán 
                string sql = @"if not exists (select MaHH from DMHH where MaHH = @@MaHH)
                                                insert into DMHH(MaHH, TenHH, DVT, QuyCach, GiaBan)
                                                values(@@MaHH, @@TenHH, @@DVT, @@QuyCach, @@GiaBan)";
                //Không insert giá bán
                string sql1 = @"if not exists (select MaHH from DMHH where MaHH = @@MaHH)
                                                insert into DMHH(MaHH, TenHH, DVT, QuyCach)
                                                values(@@MaHH, @@TenHH, @@DVT, @@QuyCach)";

                string[] paraNames = new string[]{"@@MaHH","@@TenHH","@@DVT","@@QuyCach","@@GiaBan"};
                string[] paraNames1 = new string[]{"@@MaHH","@@TenHH","@@DVT","@@QuyCach"};
                foreach (DataRowView drv in dv)
                {
                    string qc = float.Parse(drv["Dai"].ToString()).ToString() + "*" + float.Parse(drv["Rong"].ToString()).ToString() +
                        (drv["Cao"].ToString() == "" ? "" : "*" + float.Parse(drv["Cao"].ToString()).ToString()) + "_" + drv["Lop"].ToString() + "L";
                    string mahh = drv["MaKH"].ToString() + "_" + qc;
                    if (drv["GiaBan"] != DBNull.Value)
                    {
                        object[] obj = new object[] { mahh, drv["TenHang"], drv["DVT"], qc, drv["GiaBan"] };
                        if (!_data.DbData.UpdateDatabyPara(sql, paraNames, obj))
                            return;
                    }
                    else
                    {
                        object[] obj1 = new object[] { mahh, drv["TenHang"], drv["DVT"], qc};
                        if (!_data.DbData.UpdateDatabyPara(sql1, paraNames1, obj1))
                            return;
                    }
                }
            }

        }

        public void ExecuteBefore()
        {
            //Nếu đã nhập phôi sóng không cho sửa lệnh sản xuất
            if(_data.CurMasterIndex < 0)
                return;
            DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            string sql = @"select isnull(sum(soluong),0) from dtnphoi where dtdhid = '{0}' and solsx = '{1}'";
            DataTable dt = _data.DsData.Tables[1].GetChanges(DataRowState.Modified | DataRowState.Deleted);
            if (dt == null)
                return;
            foreach(DataRow dr in dt.Rows)
            {
                string dtdhid = dr.RowState == DataRowState.Modified ? dr["DTDHID"].ToString() : dr["DTDHID", DataRowVersion.Original].ToString();
                string solsx = drCur.RowState == DataRowState.Modified ? drCur["SoLSX"].ToString() : drCur["SoLSX", DataRowVersion.Original].ToString();
                string tenhang = drCur.RowState == DataRowState.Modified ? dr["TenHang"].ToString() : dr["TenHang", DataRowVersion.Original].ToString();
                object obj = _data.DbData.GetValue(string.Format(sql, dtdhid, solsx));
                if (Convert.ToInt32(obj) > 0)
                {
                    XtraMessageBox.Show(string.Format("Mặt hàng '{0}' đã lập phiếu nhập phôi sóng, không được sửa!", tenhang)
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

        private void CapNhatDaSX()
        {
            string sqldh = "";
            DataTable dt = _data.DsData.Tables[1];
            
            //DataRow[] dtcopy = _data.DsDataCopy.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            //DataRow[] dtChange = _data.DsData.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            DataTable dtChange = _data.DsData.Tables[1].GetChanges();
            foreach (DataRow dr in dtChange.Rows)
            {
               switch(dr.RowState)
               {
                   case DataRowState.Added:
                   case DataRowState.Modified:
                       if (dr["Loai"].ToString().Equals("Thùng"))
                           continue;
                       object oSoLuong = _data.DbData.GetValue("select sum(slphoi) from dtlsx where dtdhid = '" + dr["DTDHID"] + "'");
                       object oSLDat = _data.DbData.GetValue("select sum(sldat) from dtlsx where dtdhid = '" + dr["DTDHID"] + "'");
                       //object oSLDat = dt.Compute("SLDat", "DTDHID = '" + dr["DTDHID"] + "'");
                       if (oSoLuong == DBNull.Value || oSLDat == DBNull.Value)
                           continue;
                       if(Convert.ToDecimal(oSLDat) >= Convert.ToDecimal(oSoLuong))
                           sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 1,dr["DTDHID"]);
                       else
                           sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 0, dr["DTDHID"]);
                       break;
                   case DataRowState.Deleted:
                       if (dr["Loai", DataRowVersion.Original].ToString().Equals("Thùng"))
                           continue;
                       object oSoLuong1 = _data.DbData.GetValue("select sum(slphoi) from dtlsx where dtdhid = '" + dr["DTDHID",DataRowVersion.Original] + "'");
                       object oSLDat1 = _data.DbData.GetValue("select sum(sldat) from dtlsx where dtdhid = '" + dr["DTDHID",DataRowVersion.Original] + "'");
                       if (Convert.ToDecimal(oSLDat1 == DBNull.Value?0:oSLDat1) >= Convert.ToDecimal(oSoLuong1==DBNull.Value?0:oSoLuong1) && oSoLuong1 != DBNull.Value)
                           sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 1, dr["DTDHID",DataRowVersion.Original]);
                       else
                           sqldh += string.Format(";update dtdonhang set dasx = {0} where dtdhid = '{1}'", 0, dr["DTDHID",DataRowVersion.Original]);
                       break;
                       //trường hợp xóa
                       break;
                }
            }

            if (sqldh != "")
                _data.DbData.UpdateByNonQuery(sqldh);
        }

        private void CapNhatDaSX_LSX()
        { 
            string sqldh = "";
            DataTable dt = _data.DsData.Tables[1];
            
            //DataRow[] dtcopy = _data.DsDataCopy.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            //DataRow[] dtChange = _data.DsData.Tables[1].GetChanges().Select("Loai = 'Tấm'");
            DataTable dtChange = _data.DsData.Tables[1].GetChanges();
            foreach (DataRow dr in dtChange.Rows)
            {
                switch (dr.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                        if (dr["Loai"].ToString().Equals("Thùng"))
                            continue;
                        //object oSoLuong = _data.DbData.GetValue("select soluong from dtdonhang where dtdhid = '" + dr["DTDHID"] + "'");
                        //object oSLDat = _data.DbData.GetValue("select sum(sldat) from dtlsx where dtdhid = '" + dr["DTDHID"] + "'");
                        //object oSLDat = dt.Compute("SLDat", "DTDHID = '" + dr["DTDHID"] + "'");
                        object oSLDat = dr["SLDat"];
                        object oSoLuong = dr["SLPhoi"];
                        if (oSoLuong == DBNull.Value || oSLDat == DBNull.Value)
                            continue;
                        if (Convert.ToDecimal(oSLDat) >= Convert.ToDecimal(oSoLuong))
                            sqldh += string.Format(";update dtlsx set dasx = {0} where dtlsxid = '{1}'", 1, dr["DTLSXID"]);
                        else
                            sqldh += string.Format(";update dtlsx set dasx = {0} where dtlsxid = '{1}'", 0, dr["DTLSXID"]);
                        break;
                    case DataRowState.Deleted:
                        //if (dr["Loai", DataRowVersion.Original].ToString().Equals("Thùng"))
                        //    continue;
                        //object oSLDat1 = dr["SLDat", DataRowVersion.Original];
                        //object oSoLuong1 = dr["SLPhoi", DataRowVersion.Original];
                        //if (Convert.ToDecimal(oSLDat1 == DBNull.Value ? 0 : oSLDat1) >= Convert.ToDecimal(oSoLuong1 == DBNull.Value ? 0 : oSoLuong1))
                        //    sqldh += string.Format(";update dtlsx set dasx = {0} where dtlsxid = '{1}'", 1, dr["DTLSXID", DataRowVersion.Original]);
                        //else
                        //    sqldh += string.Format(";update dtlsx set dasx = {0} where dtlsxid = '{1}'", 0, dr["DTLSXID", DataRowVersion.Original]);
                        //break;
                        //trường hợp xóa
                        break;
                }
            }
            if (sqldh != "")
                _data.DbData.UpdateByNonQuery(sqldh);
        }
    }
}
