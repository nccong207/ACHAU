using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;
using System.Threading;

namespace CDT
{
    public partial class ExpireMsg : DevExpress.XtraEditors.XtraForm
    {
        double remain;
        public ExpireMsg(string msg, double d)
        {
            InitializeComponent();
            this.Text = Config.GetValue("PackageName").ToString();
            memoEdit1.Text = msg;
            remain = d;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (remain <= 0)
            {
                AppCon ac = new AppCon();
                ac.SetValue("Expire", Security.EnCode(DateTime.MinValue.ToString("MM/dd/yyyy")));
                Environment.Exit(0);
            }
            else
                this.Close();
        }
    }
}