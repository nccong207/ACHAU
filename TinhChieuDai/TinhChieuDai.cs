using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using CDTDatabase;

namespace TinhChieuDai
{
    public class TinhChieuDai : ICControl
    {
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        DataTable _dtNL;


        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public InfoCustomControl Info
        {
            get { return _info; }
        }

        public void AddEvent()
        {
            var tables = new List<string>(new string[] { "DT40", "DT42", "DT43", "DT44" });
            if (!tables.Contains(_data.DrTable["TableName"].ToString())) return;

            Database db = Database.NewDataDatabase();
            _dtNL = db.GetDataTable("select * from DMNL");

            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);

            if (_data.BsMain.DataSource != null) BsMain_DataSourceChanged(_data.BsMain, new EventArgs());
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataTable dtDetail = (_data.BsMain.DataSource as DataSet).Tables[1];
            dtDetail.ColumnChanged += new DataColumnChangeEventHandler(dtDetail_ColumnChanged);

            if (_data.DrTable["TableName"].ToString() == "DT43" || _data.DrTable["TableName"].ToString() == "DT44")
                dtDetail.ColumnChanged += new DataColumnChangeEventHandler(dtDetail_ColumnChanged1);
        }

        void dtDetail_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName != "MaNL" && e.Column.ColumnName != "SoLuong"
                || e.Row["MaNL"] == DBNull.Value || e.Row["SoLuong"] == DBNull.Value) return;

            var maNL = e.Row["MaNL"].ToString();
            var sl = Convert.ToDouble(e.Row["SoLuong"]);

            var rowNLs = _dtNL.Select("Ma = '" + maNL + "'");
            if (rowNLs.Length == 0) return;

            var khoGoc = Convert.ToDouble(rowNLs[0]["Kho"]);
            //khoGoc <= 220: don vi cm; con lai la don vi mm
            var kho = khoGoc <= 220 ? (khoGoc / 100) : (khoGoc / 1000);
            var dl = Convert.ToDouble(rowNLs[0]["DL"]) / 1000;
            if (kho == 0 || dl == 0) return;

            e.Row["ChieuDai"] = Math.Round(sl / (kho * dl), 0);
        }

        void dtDetail_ColumnChanged1(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName != "MaNL" && e.Column.ColumnName != "SLNhap"
                || e.Row["MaNL"] == DBNull.Value || e.Row["SLNhap"] == DBNull.Value) return;

            var maNL = e.Row["MaNL"].ToString();
            var sl = Convert.ToDouble(e.Row["SLNhap"]);

            var rowNLs = _dtNL.Select("Ma = '" + maNL + "'");
            if (rowNLs.Length == 0) return;

            var khoGoc = Convert.ToDouble(rowNLs[0]["Kho"]);
            //khoGoc <= 220: don vi cm; con lai la don vi mm
            var kho = khoGoc <= 220 ? (khoGoc / 100) : (khoGoc / 1000);
            var dl = Convert.ToDouble(rowNLs[0]["DL"]) / 1000;
            if (kho == 0 || dl == 0) return;

            e.Row["CDNhap"] = Math.Round(sl / (kho * dl), 0);
        }
    }
}
