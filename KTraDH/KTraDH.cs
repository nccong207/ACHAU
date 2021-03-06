using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;
using System.Windows.Forms;

namespace KTraDH
{
    public class KTraDH : ICData
    {
        DataTable dtNL;
        //List<string> lstNL = new List<string>(new string[] { "Mat_", "SB_", "MB_", "SC_", "MC_", "SE_", "ME_" });
        DataRow drCur;
        string tableName;
        List<string> lstTB = new List<string>(new string[] { "MTDonHang", "MTLSX" });
        List<string> lstPk = new List<string>(new string[] { "MTDHID", "MTLSXID" });
        Database db = Database.NewDataDatabase();
        DataCustomData _data;
        InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);
        DataTable dtDMNL;
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
            tableName = _data.DrTableMaster["TableName"].ToString();
            if (!lstTB.Contains(tableName))
                return;
            
            drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            DataTable dtM = _data.DsData.Tables[0];
            
            if (drCur.RowState == DataRowState.Deleted)
                return;

            if (tableName == "MTDonHang")
            {
                if (drCur.RowState == DataRowState.Modified
                    && Boolean.Parse(drCur["Duyet", DataRowVersion.Current].ToString()) == false
                    && Boolean.Parse(drCur["Duyet", DataRowVersion.Original].ToString()) == true)   //trường hợp bỏ duyệt -> không cảnh báo
                    return;
                _info.Result = KTNhapSoLieu(drCur);
                if (_info.Result == false) return;
            }
            if (tableName == "MTDonHang")
            {
                //Truong hop them moi
                //if (drCur.RowState == DataRowState.Added)
                //{
                //    DataRow drDetail = _data.DsData.Tables[1].Rows[0];
                //    foreach (DataColumn i in _data.DsData.Tables[1].Columns)
                //    {
                //        drCur[i.ColumnName] = drDetail[i.ColumnName];
                //    }
                //}
                //thong bao ve don hang chua san xuat
                string makh = drCur["MaKH"].ToString();
                string s = @"select mt.SoDH, mt.NgayCT, dt.* 
                        from MTDonHang mt inner join DTDonHang dt on mt.MTDHID = dt.MTDHID
                        where mt.LSX is null and mt.MaKH = '" + makh + "' and mt.MTDHID <> '" + drCur["MTDHID"].ToString() + "'";
                DataTable dtDSDH = db.GetDataTable(s);
                if (dtDSDH.Rows.Count > 0)
                {
                    if (XtraMessageBox.Show(string.Format("Khách hàng này có {0} đơn hàng chưa sản xuất\n" +
                        "Bạn có muốn xem không?", dtDSDH.Rows.Count), Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        FrmDSDH frm = new FrmDSDH(dtDSDH);
                        frm.ShowDialog();
                    }
                }
                //thong bao ve 4 thong so don hang
                string msg = "Giá trị đơn hàng: {0}\nCông nợ hiện tại: {1}\nHạn mức công nợ: {2}\nTrị giá tồn kho: {3}";
                object m0 = _data.DsData.Tables[1].Compute("sum(ThanhTien)", "MTDHID = '" + drCur["MTDHID"].ToString() + "'");
                decimal d0 = m0 == DBNull.Value ? 0 : decimal.Parse(m0.ToString());
                //cong no khach hang
                s = @"select sum(PhatSinh + Thue - ThanhToan - HangTra) from wCNPThu where MaKH = '" + makh + "'";
                object m1 = db.GetValue(s);
                decimal d1 = m1 == DBNull.Value ? 0 : decimal.Parse(m1.ToString());
                //han muc cong no
                object m2 = db.GetValue("select HMNo from DMKH where MaKH = '" + makh + "'");
                decimal d2 = m2 == DBNull.Value ? 0 : decimal.Parse(m2.ToString());
                //tri gia ton kho
                s = @"select sum(PsNo - PsCo) from BLVT where MaKH = '" + makh + "'";
                object m3 = db.GetValue(s);
                decimal d3 = m3 == DBNull.Value ? 0 : decimal.Parse(m3.ToString());
                XtraMessageBox.Show(string.Format(msg, d0.ToString("###,###,###,##0"), d1.ToString("###,###,###,##0"),
                    d2.ToString("###,###,###,##0"), d3.ToString("###,###,###,##0")),
                    Config.GetValue("PackageName").ToString());
            } 
            //if (tableName == "MTDonHang")
            //    CapNhatNL(drCur);
            if (!_info.Result)      //khong chon duoc nguyen lieu voi kho giay phu hop
                return;
            //thong bao ve danh sach nguyen lieu su dung va ton kho
            if (XtraMessageBox.Show("Bạn có muốn xem nguyên liệu sử dụng không?",
                Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //LẤY SỐ TỒN TRONG VIEW wDMNL2
                dtDMNL = db.GetDataTable("Select * from wDMNL2");
                DataTable dtDSNL = LayDSNL(drCur);
                FrmDSNL frmNL = new FrmDSNL(dtDSNL);
                frmNL.ShowDialog();
            }
            string dienGiai = tableName == "MTDonHang" ? "đơn hàng" : "lệnh sản xuất";
            _info.Result = (XtraMessageBox.Show("Bạn có muốn lưu " + dienGiai + " này không?",
                Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        private bool KTNhapSoLieu(DataRow drCur)
        {
            if (string.IsNullOrEmpty(drCur["SoPO"].ToString()))
            {
                return XtraMessageBox.Show("Đơn hàng chưa có số PO, bạn có chắc muốn lưu không?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes;
            }
            var tbChange = _data.DsData.Tables[1].GetChanges();
            DataRow[] drChanges = tbChange == null ? null : tbChange.Select("LotNo = '' OR LotNo is null");
            if (drChanges != null && drChanges.Length > 0)
            {
                return XtraMessageBox.Show("Có mặt hàng chưa nhập Lot No, bạn có chắc muốn lưu không?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes;
            }
            return true;
        }

        private DataTable LayDSNL(DataRow drCur)
        {
            if (dtNL == null)
            {
                dtNL = db.GetDataTable("select Ma, Ten, MaPhu, Kho from wDMNL");
                dtNL.PrimaryKey = new DataColumn[] { dtNL.Columns["Ma"] };
            }

            DataTable dtDSNL = new DataTable();
            dtDSNL.Columns.Add("Ma", typeof(String));
            dtDSNL.Columns.Add("Ten", typeof(String));
            dtDSNL.Columns.Add("SLSD", typeof(Decimal));
            dtDSNL.Columns.Add("SLTK", typeof(Decimal));
            dtDSNL.Columns.Add("SLCL", typeof(Decimal), "SLTK - SLSD");
            dtDSNL.PrimaryKey = new DataColumn[] { dtDSNL.Columns["Ma"] };

            string pk = lstPk[lstTB.IndexOf(tableName)];
            string pkValue = drCur[pk].ToString();
            DataTable dt = _data.DsData.Tables[1];
            DataRow[] drs = dt.Select(pk + " = '" + pkValue + "'");

            List<string> lstMa = new List<string>();
            foreach (DataRow dr in drs)
            {
                string m = dr["Mat_Giay"].ToString();
                string sb = dr["SB_Giay"].ToString();
                string mb = dr["MB_Giay"].ToString();
                string sc = dr["SC_Giay"].ToString();
                string mc = dr["MC_Giay"].ToString();
                string se = dr["SE_Giay"].ToString();
                string me = dr["ME_Giay"].ToString();
                string mdl = dr["Mat_DL"].ToString();
                string sbdl = dr["SB_DL"].ToString();
                string mbdl = dr["MB_DL"].ToString();
                string scdl = dr["SC_DL"].ToString();
                string mcdl = dr["MC_DL"].ToString();
                string sedl = dr["SE_DL"].ToString();
                string medl = dr["ME_DL"].ToString();
                ThemNL(dtDSNL, dtNL, lstMa, m, mdl, dr, 1);
                ThemNL(dtDSNL, dtNL, lstMa, sb, sbdl, dr, 1.5M);
                ThemNL(dtDSNL, dtNL, lstMa, mb, mbdl, dr, 1);
                ThemNL(dtDSNL, dtNL, lstMa, sc, scdl, dr, 1.5M);
                ThemNL(dtDSNL, dtNL, lstMa, mc, mcdl, dr, 1);
                ThemNL(dtDSNL, dtNL, lstMa, se, sedl, dr, 1.5M);
                ThemNL(dtDSNL, dtNL, lstMa, me, medl, dr, 1);
            }

            return dtDSNL;
        }

        private void ThemNL(DataTable dtDSNL, DataTable dtNL, List<string> lstMa, string maNL, string dl, DataRow dr, decimal hs)
        {
            if (maNL == "" || dl == "")
                return;
            //Bổ sung điều kiện a = khổ max / giấy rộng
             string strSL = "SLPhoi";
             decimal a = Convert.ToDecimal(dr["KhoMax"]) / Convert.ToDecimal(dr["RongG"]);
             a = decimal.ToInt32(a);
             if (a == Convert.ToDecimal(dr["Dao"]))
                 strSL = "SLSX";

            string sl = (tableName == "MTDonHang") ? dr["SoLuong"].ToString() : dr[strSL].ToString();
            decimal dt = 0;
            if (dr["Loai"].ToString() == "Thùng")
                dt = decimal.Parse(dr["DienTich"].ToString());
            else
                dt = decimal.Parse(dr["Dai"].ToString()) * decimal.Parse(dr["Rong"].ToString()) / 10000;
            decimal d = 0;
            //Bổ sung điều kiện rộng + cao
            if (dr["Loai"].ToString() == "Thùng")
            {
                if (Convert.ToDecimal(dr["Rong"]) + (dr["Cao"]== DBNull.Value?0: Convert.ToDecimal(dr["Cao"])) <= Convert.ToDecimal(dr["RongG"]))
                {
                    if (a == Convert.ToDecimal(dr["Dao"])
                        || (a != Convert.ToDecimal(dr["Dao"]) && Convert.ToDecimal(dr["KhoMax"]) < Convert.ToDecimal(dr["KhoTT"])))
                        d = hs * decimal.Parse(dl) * dt * (sl == "" ? 0 : decimal.Parse(sl)) / 1000;
                    else
                        d = hs * decimal.Parse(dl) * dt * (sl == "" ? 0 : decimal.Parse(sl)) * a / 1000;
                }
                else
                {
                    decimal y = Math.Round(((Convert.ToDecimal(dr["Rong"]) + (dr["Cao"] == DBNull.Value?0: Convert.ToDecimal(dr["Cao"]))) /
                           (Convert.ToDecimal(dr["RongG"]) == 0 ? 1 : Convert.ToDecimal(dr["RongG"]))), 0);
                    if (y == 1)
                    {
                        if (a == Convert.ToDecimal(dr["Dao"])
                          || (a != Convert.ToDecimal(dr["Dao"]) && Convert.ToDecimal(dr["KhoMax"]) < Convert.ToDecimal(dr["KhoTT"])))
                            d = hs * decimal.Parse(dl) * dt * (sl == "" ? 0 : decimal.Parse(sl)) / 1000;
                        else
                            d = hs * decimal.Parse(dl) * dt * (sl == "" ? 0 : decimal.Parse(sl)) * a / 1000;
                    }
                    else
                        d = hs * decimal.Parse(dl) * dt * (tableName == "MTDonHang" ? decimal.Parse(dr["SoLuong"].ToString()) : decimal.Parse(dr["SLSX"].ToString())) * y / 1000;
                }
            }
            else
            {
                sl = (tableName == "MTDonHang") ? dr["SoLuong"].ToString() : dr["SLSX"].ToString();
                d = hs * decimal.Parse(dl) * dt * (sl == "" ? 0 : decimal.Parse(sl)) / 1000;
            }
            if (!lstMa.Contains(maNL))
            {
                DataRow[] row = dtDMNL.Select("Ma='" + maNL.ToString() + "'");
                decimal ton =0;
                if(row[0]["Ton"] != DBNull.Value)
                    ton = Convert.ToDecimal(row[0]["Ton"].ToString());
                string ten = dtNL.Rows.Find(maNL)["Ten"].ToString();
                dtDSNL.Rows.Add(new object[] { maNL, ten, d, ton });
                lstMa.Add(maNL);
            }
            else
            {
                DataRow drNL = dtDSNL.Rows.Find(maNL);
                drNL["SLSD"] = decimal.Parse(drNL["SLSD"].ToString()) + d;
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
