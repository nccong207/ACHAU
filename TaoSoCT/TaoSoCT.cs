using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace TaoSoCT
{
    public class TaoSoCT:ICData
    {
        List<string> lstTable = new List<string>(new string[] { "MT11", "MT12", "MT22", "MT23", "MT32", "MT33", "MT34", "MT40", "MT41", "MT42", "MT43", "MT44", "MT45", "MT46", "MTBaoGia", "MTDonHang", "MTKH", "MTLSX", "MTNPhoi", "MTXPhoi" });
        List<string> lstSoCT = new List<string>(new string[] { "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoCT", "SoBG", "SoDH", "SoKH", "SoLSX", "SoCT", "SoCT" });
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        Database dbCDT = Database.NewStructDatabase();

        #region ICData Members
  
        public TaoSoCT()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {

        }

        private bool KTSoCuon()
        {
            if (_data.CurMasterIndex < 0)
                return true;
            var maKho = _data.DsData.Tables[0].Rows[_data.CurMasterIndex]["MaKho"].ToString();
            using (DataTable dt = _data.DsData.Tables[1].GetChanges())
            {
                if (dt == null)
                    return true;
                string msg = "";
                int i =1;
                DataTable dtBLNL = db.GetDataTable(string.Format(@"Select macuon, soct from blnl where MaKho = '{0}' and SoLuong > 0 and macuon in
                                (select macuon from blnl where MaKho = '{0}' group by macuon having sum(soluong) - sum(soluong_x) > 0)", maKho));
                
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.RowState == DataRowState.Added || dr.RowState == DataRowState.Modified)
                    {
                        if (dr.RowState == DataRowState.Modified
                            && (dr["MaCuon"].ToString() == dr["MaCuon", DataRowVersion.Original].ToString()))
                            continue;
                        DataRow[] drs = dtBLNL.Select("MaCuon='"+ dr["MaCuon"].ToString() +"'");
                        if(drs.Length != 0)
                        {
                            msg += string.Format("Mã cuộn {0} dòng {1} đã có trong phiếu {2}.\n", drs[0]["MaCuon"], i, drs[0]["SoCT"]); 
                        }
                        if (dt.Select("MaCuon='" + dr["MaCuon"].ToString() + "'").Length > 1)
                        {
                            msg += string.Format("Mã cuộn {0} dòng {1} đã có trong phiếu.\n", dr["MaCuon"], i); 
                        }
                    }
                    i++;
                }
                if(msg=="")
                    return true;
                else
                {
                    XtraMessageBox.Show(msg, Config.GetValue("PackageName").ToString());
                    return false;
                }
            }
        }

        private bool KTSuaNgay(DataRow drMaster)
        {
            DateTime dt1 = DateTime.Parse(drMaster["NgayCT", DataRowVersion.Current].ToString());
            DateTime dt2 = DateTime.Parse(drMaster["NgayCT", DataRowVersion.Original].ToString());
            return (dt1.Month != dt2.Month || dt1.Year != dt2.Year);
        }

        void CreateCT()
        {
            if (_data.CurMasterIndex < 0)
                return;
            string mact = _data.DrTable["MaCT"].ToString();
            if (mact == "")
                return;
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (!drMaster.Table.Columns.Contains("NgayCT"))
                return;
            string tb = _data.DrTableMaster["TableName"].ToString();
            string soct = lstSoCT[lstTable.IndexOf(tb)];
            if (drMaster.RowState == DataRowState.Added
                || (drMaster.RowState == DataRowState.Modified && KTSuaNgay(drMaster)))
            {
                string sql = "", soctNew = "", Thang = "", Nam = "";
                DateTime NgayCT = (DateTime)drMaster["NgayCT"];
                // Tháng: 2 chữ số
                // Năm: 2 số cuối của năm
                Thang = NgayCT.Month.ToString();
                Nam = NgayCT.Year.ToString();

                if (Thang.Length == 1)
                    Thang = "0" + Thang;
                Nam = Nam.Substring(2, 2);

                string suffix = "-" + Thang + Nam;
                
                sql = string.Format(@" SELECT   Top 1 {2}  
                                       FROM     {0}
                                       WHERE    {2} LIKE '{1}%{3}'
                                       ORDER BY {2} DESC", tb, mact, soct, suffix);
                DataTable dt = db.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    string soctOld = dt.Rows[0][soct].ToString();
                    soctNew = GetNewValue(soctOld.Substring(0, soctOld.Length - suffix.Length));
                    //MessageBox.Show(soctNew);
                }
                else
                    soctNew = mact + "-" + "0001";
                if (soctNew != "")
                    drMaster[soct] = soctNew + suffix;
            }
        }

        void CreateSoPhieu()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (!drMaster.Table.Columns.Contains("NgayCT"))
                return;
            if (drMaster.RowState == DataRowState.Added
                || (drMaster.RowState == DataRowState.Modified && KTSuaNgay(drMaster)))
            {
                string sql = "", soctNew = "", Thang = "", Nam = "";
                DateTime NgayCT = (DateTime)drMaster["NgayCT"];
                // Tháng: 2 chữ số
                // Năm: 2 số cuối của năm
                Thang = NgayCT.Month.ToString();
                Nam = NgayCT.Year.ToString();

                if (Thang.Length == 1)
                    Thang = "0" + Thang;

                string suffix = "/" + Thang;

                sql = string.Format(@" SELECT SoPhieu from MT44 where year(NgayCT) = {0} and month(NgayCT) = {1} and SoPhieu like '%{2}' order by SoPhieu desc"
                    , Nam, Thang, suffix);
                DataTable dt = db.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    string soctOld = dt.Rows[0]["SoPhieu"].ToString();
                    soctNew = GetNewValue(soctOld.Substring(0, soctOld.Length - suffix.Length));
                    //MessageBox.Show(soctNew);
                }
                else
                    soctNew = "001";
                if (soctNew != "")
                    drMaster["SoPhieu"] = soctNew + suffix;
            }
        }

        private string GetNewValue(string OldValue)
        {
            try
            {
                int i = OldValue.Length - 1;
                for (; i > 0; i--)
                    if (!Char.IsNumber(OldValue, i))
                        break;
                if (i == OldValue.Length - 1)
                {
                    int NewValue = Int32.Parse(OldValue) + 1;
                    return NewValue.ToString();
                }
                string PreValue = OldValue.Substring(0, i + 1);
                string SufValue = OldValue.Substring(i + 1);
                int intNewSuff = Int32.Parse(SufValue) + 1;
                string NewSuff = intNewSuff.ToString().PadLeft(SufValue.Length, '0');
                return (PreValue + NewSuff);
            }
            catch
            {
                return string.Empty;
            }
        }

        public void ExecuteBefore()
        {
            if (!lstTable.Contains(_data.DrTableMaster["TableName"].ToString()))
                return;
            CreateCT();
            if (_data.DrTableMaster["TableName"].ToString().Equals("MT44")) CreateSoPhieu();
            if (_data.DrTableMaster["TableName"].ToString().Equals("MT42"))
                if (!KTSoCuon())
                    _info.Result = false;
                    
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
