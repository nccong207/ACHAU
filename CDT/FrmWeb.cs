using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace CDT
{
    public partial class FrmWeb : DevExpress.XtraEditors.XtraForm
    {
        private string _curUrl = "";

        public string CurUrl
        {
            get { return _curUrl; }
            set 
            {
                if (_curUrl != value)
                {
                    _curUrl = value;
                    axWebBrowser1.Navigate(_curUrl);
                }
            }
        }
        public FrmWeb()
        {
            InitializeComponent();
        }

        private void FrmWeb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}