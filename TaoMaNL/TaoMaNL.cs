using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Plugins;
using DevExpress.XtraEditors;
using CDTLib;

namespace TaoMaNL
{
    public class TaoMaNL:ICControl
    {
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.SingleDt);
        #region ICControl Members

        public void AddEvent()
        {
            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(_data.BsMain, new EventArgs());
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            //TextEdit teMaNL = _data.FrmMain.Controls.Find("MaNL", true)[0] as TextEdit;
            //teMaNL.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            if (_data.BsMain == null)
                return;
            DataTable dtNL = _data.BsMain.DataSource as DataTable;
            dtNL.ColumnChanged += new DataColumnChangeEventHandler(dtNL_ColumnChanged);
        }

        void dtNL_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            List<string> lstFields = new List<string>(new string[] { "MaNCC", "MaNHOM", "MaNL", "Kho", "DL" });
            if (lstFields.Contains(e.Column.ColumnName))
            {
                string k = e.Row["Kho"].ToString();
                if (k != "")
                    k = float.Parse(k).ToString();
                string d = e.Row["DL"].ToString();
                if (d != "")
                    d = float.Parse(d).ToString();
                //e.Row["Ma"] = e.Row["MaNCC"].ToString() + "." + e.Row["MaNhom"].ToString() + "."
                //+ e.Row["MaNL"].ToString() + "." + d + "." + k;
                e.Row["Ma"] = e.Row["MaNhom"].ToString() + "." + k + "." + d;
                if (e.Row["Ma"].ToString().Length > 20)
                {
                    XtraMessageBox.Show("Mã chính không được vượt quá 20 ký tự", Config.GetValue("PackageName").ToString());
                    e.Row["Ma"] = DBNull.Value;
                }
                e.Row.EndEdit();
            }
        }

        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public InfoCustomControl Info
        {
            get { return _info; }
        }

        #endregion
    }
}
