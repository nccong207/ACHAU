using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace DaSX
{
    public class DaSX : ICData
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
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.Deleted | DataViewRowState.ModifiedCurrent;
            string slsx = "update DTLSX set DaSX = {0} where DTLSXID = '{1}'";
            string sdh = "update DTDonHang set DaSX = {0} where DTDHID = (select DTDHID from DTLSX where DTLSXID = '{1}')";
            string sqldh = @"   declare @dtdh nvarchar(128)
                                declare @soluong decimal
                                declare @slsx decimal
                                select @dtdh = dtdhid from dtlsx where dtlsxid = '{1}'
                                select @soluong = soluong from dtdonhang where dtdhid = @dtdh
                                select @slsx = sum(slsx) from dtkh where dtlsxid in (select dtlsxid from dtlsx where dtdhid = @dtdh)
                                update dtdonhang set dasx = case when @slsx >= @soluong then 1 else 0 end where dtdhid = @dtdh
                                ";

            foreach (DataRowView drv in dv)
            {
                string lsxid = drv["DTLSXID"].ToString();
                bool ht = Boolean.Parse(drv["HT"].ToString());
                switch (drv.Row.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                        int t = ht ? 1 : 0;
                        _data.DbData.UpdateByNonQuery(string.Format(slsx, t, lsxid));
                        //đơn hàng hoàn thành khi slsx >= sl đặt
                        _data.DbData.EndMultiTrans();
                        _data.DbData.UpdateByNonQuery(string.Format(sqldh, t, lsxid));
                        //_data.DbData.UpdateByNonQuery(string.Format(sdh, t, lsxid));
                        break;
                    case DataRowState.Deleted:
                        if (ht)
                        {
                            _data.DbData.UpdateByNonQuery(string.Format(slsx, 0, lsxid));
                            _data.DbData.UpdateByNonQuery(string.Format(sdh, 0, lsxid));
                        }
                        break;
                }
            }
        }

        public void ExecuteBefore()
        {
            //Không cho xóa khi đã lập phiếu nhập thành phẩm
            KTXoa();
            KiemSoatSLHT();
            //DataView dv = new DataView(_data.DsData.Tables[1]);
            //dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.Deleted | DataViewRowState.ModifiedCurrent;
            //string slsx = "update DTLSX set DaSX = {0} where DTLSXID = '{1}'";
            //string sdh = "update DTDonHang set DaSX = {0} where DTDHID = (select DTDHID from DTLSX where DTLSXID = '{1}')";
            //foreach (DataRowView drv in dv)
            //{
            //    string lsxid = drv["DTLSXID"].ToString();
            //    bool ht = Boolean.Parse(drv["HT"].ToString());
            //    switch (drv.Row.RowState)
            //    {
            //        case DataRowState.Added:
            //        case DataRowState.Modified:
            //            int t = ht ? 1 : 0;
            //            _data.DbData.UpdateByNonQuery(string.Format(slsx, t, lsxid));
            //            //đơn hàng hoàn thành khi slsx >= sl đặt
            //            _data.DbData.UpdateByNonQuery(string.Format(sdh, t, lsxid));
            //            break;
            //        case DataRowState.Deleted:
            //            if (ht)
            //            {
            //                _data.DbData.UpdateByNonQuery(string.Format(slsx, 0, lsxid));
            //                _data.DbData.UpdateByNonQuery(string.Format(sdh, 0, lsxid));
            //            }
            //            break;
            //    }
            //}
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion

        //Không cho xóa khi đã lập phiếu nhập thành phẩm
        private void KTXoa()
        {
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.Deleted;

            if (dv == null)
                return;
//            string sql = @" select		count(*) [flag]
//                            from		dtlsx l inner join dt22 d on l.dtdhid = d.dtdhid
//                            where		l.dtlsxid = '{0}'
//                            group by	l.dtlsxid ";
            string sql = @"select	isnull(sum(d.soluong),0) [slnhap], 0 [slsx]
                            into	#tmp123
                            from	mt22 m 
		                            inner join dt22 d on m.mt22id = d.mt22id 
		                            inner join dtlsx l on l.dtdhid = d.dtdhid
                            where	l.dtlsxid = '{0}'
                            union all
                            select	0,isnull(sum(k.slsx),0)
                            from	dtkh k 
		                            inner join dtlsx l on k.dtlsxid = l.dtlsxid
                            where	l.dtdhid in (select dtdhid 
					                            from dtlsx where dtlsxid = '{0}')

                            select	sum(slsx) [slsx],sum(slnhap) [slnhap] from #tmp123

                            drop table #tmp123";
            
            foreach (DataRowView drv in dv)
            {
                DataTable dt = _data.DbData.GetDataTable(string.Format(sql, drv.Row.RowState == DataRowState.Deleted
                                                ? drv.Row["DTLSXID", DataRowVersion.Original] : drv.Row["DTLSXID"]));
                if (dt.Rows.Count == 0)
                    continue;

                decimal slcu = Convert.ToDecimal(drv.Row["SLSX", DataRowVersion.Original] == DBNull.Value ? 0 : drv.Row["SLSX", DataRowVersion.Original]);
                decimal slsx = Convert.ToDecimal(dt.Rows[0]["slsx"]) - slcu;
                decimal slnhap = Convert.ToDecimal(dt.Rows[0]["slnhap"]);
                if (slsx < slnhap)
                {
                    //THông báo lỗi
                    string strPNhap = "";
                    string sql1 = @"select  m.SoCT 
                                    from	mt22 m 
		                                    inner join dt22 d on m.mt22id = d.mt22id 
		                                    inner join dtlsx l on l.dtdhid = d.dtdhid
                                    where	l.dtlsxid = '{0}'";
                    DataTable dt1 = _data.DbData.GetDataTable(string.Format(sql1, drv.Row["DTLSXID", DataRowVersion.Original]));
                    foreach (DataRow dr in dt1.Rows)
                    {
                        strPNhap += dr["SoCT"].ToString() + ", ";
                    }
                    XtraMessageBox.Show(string.Format("Mặt hàng '{0}' đã lập phiếu nhập {1}. Không được xóa!"
                        , drv.Row.RowState == DataRowState.Deleted ? drv.Row["TenHang", DataRowVersion.Original] : drv.Row["TenHang"], strPNhap.Substring(0, strPNhap.Length - 2))
                        , Config.GetValue("PackageName").ToString());
                    _info.Result = false;
                    return;
                }
            }
        }

        //Kiểm soát số lượng hoàn thành
        private void KiemSoatSLHT()
        {
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
            
            if (dv == null)
                return;
            string sql = @"select	isnull(sum(d.soluong),0) [slnhap], 0 [slsx]
                            into	#tmp123
                            from	mt22 m 
		                            inner join dt22 d on m.mt22id = d.mt22id 
		                            inner join dtlsx l on l.dtdhid = d.dtdhid
                            where	l.dtlsxid = '{0}'
                            union all
                            select	0,isnull(sum(k.slsx),0)
                            from	dtkh k 
		                            inner join dtlsx l on k.dtlsxid = l.dtlsxid
                            where	l.dtdhid in (select dtdhid 
					                            from dtlsx where dtlsxid = '{0}')

                            select	sum(slsx) [slsx],sum(slnhap) [slnhap] from #tmp123

                            drop table #tmp123";

            foreach (DataRowView drv in dv)
            {
                decimal slmoi = Convert.ToDecimal(drv.Row["SLSX", DataRowVersion.Current] == DBNull.Value ? 0 : drv.Row["SLSX", DataRowVersion.Current]);
                decimal slcu = Convert.ToDecimal(drv.Row["SLSX", DataRowVersion.Original] == DBNull.Value ? 0 : drv.Row["SLSX", DataRowVersion.Original]);
                if (slmoi != slcu)
                {
                    decimal slTangGiam = slmoi - slcu;
                    DataTable dt = _data.DbData.GetDataTable(string.Format(sql, drv.Row["dtlsxid"]));
                    if (dt.Rows.Count == 0)
                        continue;
                    decimal slsx = Convert.ToDecimal(dt.Rows[0]["slsx"]) + slTangGiam;
                    decimal slnhap = Convert.ToDecimal(dt.Rows[0]["slnhap"]);
                    if (slsx < slnhap)
                    { 
                        //THông báo lỗi
                        string strPNhap = "";
                        string sql1 = @"select  m.SoCT 
                                        from	mt22 m 
		                                        inner join dt22 d on m.mt22id = d.mt22id 
		                                        inner join dtlsx l on l.dtdhid = d.dtdhid
                                        where	l.dtlsxid = '{0}'";
                        DataTable dt1 = _data.DbData.GetDataTable(string.Format(sql1, drv.Row["DTLSXID", DataRowVersion.Original]));
                        foreach (DataRow dr in dt1.Rows)
                        {
                            strPNhap += dr["SoCT"].ToString() + ", ";
                        }
                        XtraMessageBox.Show(string.Format("Số lượng hoàn thành của mặt hàng '{0}' nhỏ hơn số lượng nhập của phiếu nhập {1}."
                            , drv.Row.RowState == DataRowState.Deleted ? drv.Row["TenHang", DataRowVersion.Original] : drv.Row["TenHang"], strPNhap.Substring(0,strPNhap.Length - 2))
                            , Config.GetValue("PackageName").ToString());
                        _info.Result = false;
                        return;
                    }
                }
            }
        }
    }
}
