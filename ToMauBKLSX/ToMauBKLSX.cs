using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.Drawing;

namespace ToMauBKLSX
{
    public class ToMauBKLSX : ICReport
    {
        private DataCustomReport _data;
        private InfoCustomReport _info = new InfoCustomReport(IDataType.Report);
        GridView gvMain;

        public DataCustomReport Data
        {
            set { _data = value; }

        }

        public InfoCustomReport Info
        {
            get { return _info; }
        }

        public void Execute()
        {
            gvMain = (_data.FrmMain.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;

            StyleFormatCondition h1 = new StyleFormatCondition();
            gvMain.FormatConditions.Add(h1);
            h1.Column = gvMain.Columns["TinhTrangNP"];
            h1.Condition = FormatConditionEnum.Equal;
            h1.Value1 = "Chưa đủ";
            h1.Appearance.BackColor = Color.Yellow;
            h1.ApplyToRow = true;

            StyleFormatCondition h2 = new StyleFormatCondition();
            gvMain.FormatConditions.Add(h2);
            h2.Column = gvMain.Columns["TinhTrangNP"];
            h2.Condition = FormatConditionEnum.Equal;
            h2.Value1 = "Nhập đủ";
            h2.Appearance.BackColor = Color.LightGreen;
            h2.ApplyToRow = true;

            StyleFormatCondition h = new StyleFormatCondition();
            gvMain.FormatConditions.Add(h);
            h.Column = gvMain.Columns["DaSX"];
            h.Condition = FormatConditionEnum.Equal;
            h.Value1 = true;
            h.Appearance.BackColor = Color.Gainsboro;
            h.ApplyToRow = true;
        }
    }
}
