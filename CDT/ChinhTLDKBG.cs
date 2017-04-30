using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;

namespace ChinhTLDKBG
{
    public class ChinhTLDKBG : ICControl
    {
        private DataCustomFormControl _data;
        private InfoCustomControl _info = new InfoCustomControl(IDataType.Single);
        #region ICControl Members

        public void AddEvent()
        {
            GridView gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gvMain.OptionsView.RowAutoHeight = true;
            gvMain.OptionsView.NewItemRowPosition = NewItemRowPosition.None;
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
