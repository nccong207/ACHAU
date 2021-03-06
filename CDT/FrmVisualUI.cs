using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraBars;
using DevExpress.XtraNavBar;
using CDTSystem;
using CDTControl;
using FormFactory;
using ErrorManager;
using CDTLib;
using System.IO;
using System.Reflection;
using DevExpress.XtraBars.Controls;
using DevExpress.XtraLayout.Utils;

namespace CDT
{
    public partial class FrmVisualUI : DevExpress.XtraEditors.XtraForm
    {
        private DataTable dtMenu;

        public DataTable DtMenu
        {
            get { return dtMenu; }
        }
        private int _sysMenuParent;
        private int _delayPopup = 0;

        public int SysMenuParent
        {
            get { return _sysMenuParent; }
            set 
            { 
                _sysMenuParent = value;
                bpNV._sysMenuParent = value;
            }
        }
        private SysMenu _sysMenu;
        private PluginManager _pm;
        private Command _cmd;

        public Command Cmd
        {
            get { return _cmd; }
            set 
            { 
                _cmd = value;
                if (bpNV != null)
                    bpNV._cmd = value;
            }
        }
        public DataRow DrCurrent;
        private BarButtonItem bbiThem;
        private BarButtonItem bbiTatCa;
        public PopupMenu PMenu
        {
            get { return pMenu; }
        }
        private Rectangle _popupRect;

        public Rectangle PopupRect
        {
            get { return _popupRect; }
        }

        public FrmVisualUI(SysMenu sysMenu, PluginManager pm)
        {
            InitializeComponent();
            AddItemToPopup();
            _sysMenu = sysMenu;
            bpNV._sysMenu = _sysMenu;
            _pm = pm;
        }

        private void AddItemToPopup()
        {
            bool v = Config.GetValue("Language").ToString() == "0";
            bbiThem = new BarButtonItem(bmMenu, v ? "Thêm mới" : "Add new");
            bbiThem.Tag = FormAction.New;
            BarButtonItem bbiTim = new BarButtonItem(bmMenu, v ? "Tìm kiếm" : "Search data");
            bbiTim.Tag = FormAction.View;
            bbiTatCa = new BarButtonItem(bmMenu, v ? "Xem tất cả" : "View all data");
            bbiTatCa.Tag = FormAction.All;
            //BarButtonItem bbiSua = new BarButtonItem(bmMenu, v ? "Cập nhật" : "Update");
            //bbiSua.Tag = FormAction.Edit;
            //bbiNLNhanh = new BarButtonItem(bmMenu, v ? "Nhập liệu nhanh" : "Quick update");
            //bbiNLNhanh.Tag = FormAction.GridEdit;
            pMenu.AddItems(new BarItem[] { bbiTatCa, bbiThem, bbiTim });
            bmMenu.ItemClick += new ItemClickEventHandler(bmMenu_ItemClick);
            pMenu.Popup += new EventHandler(PMenu_Popup);
            bmMenu.HighlightedLinkChanged += new HighlightedLinkChangedEventHandler(bmMenu_HighlightedLinkChanged);
        }

        void PMenu_Popup(object sender, EventArgs e)
        {
            FieldInfo fi = typeof(PopupMenu).GetField("subControl", BindingFlags.Instance | BindingFlags.NonPublic);
            PopupMenuBarControl p = (PopupMenuBarControl)fi.GetValue(sender);
            Point m = Control.MousePosition;
            int x = m.X;
            int y = m.Y;
            if (x + p.Form.Size.Width > Screen.PrimaryScreen.Bounds.Width)
                x = x - p.Form.Size.Width;
            if (y + p.Form.Size.Height > Screen.PrimaryScreen.Bounds.Height)
                y = y - p.Form.Size.Height;
            _popupRect = new Rectangle(new Point(x,y), p.Form.Size);
        }

        void bmMenu_HighlightedLinkChanged(object sender, HighlightedLinkChangedEventArgs e)
        {
            if (e.PrevLink != null && e.PrevLink.Bar == null
                && e.Link == null && pMenu.Opened)
                pMenu.HidePopup();
        }
        
        private void VisualUI_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F12)
                bpNV.ChangeImageStatus();
        }

        public void RefreshData(DataTable dtModule)
        {
            //dtMenu = _sysMenu.GetMenuForModule(_sysMenuParent);
            dtMenu = dtModule;
            bpNV.dtMenu = dtModule;
            bpNV.BusinessProcess_Load(bpNV, new EventArgs());
        }

        private void AddItem(DataRow dr, NavBarGroup nbg, NavBarControl nbc)
        {
            string exe = Boolean.Parse(Config.GetValue("Admin").ToString()) ? "" : dr["Executable"].ToString();
            NavBarItem nbi = new NavBarItem(Config.GetValue("Language").ToString() == "0" ? dr["MenuName"].ToString() : dr["MenuName2"].ToString());
            nbi.Tag = dr;
            nbc.Items.Add(nbi);
            nbg.ItemLinks.Add(nbi);
            nbi.Enabled = (exe == "" || Boolean.Parse(exe));
            if (nbi.Caption.Length > 22)
                nbi.Hint = nbi.Caption;
        }
 
        private void Button_Click(object sender, EventArgs e)
        {
            SimpleButton btn = sender as SimpleButton;
            if (btn == null)
                return;
            DataRow dr = btn.Tag as DataRow;
            if (dr == null)
                return;
            DrCurrent = dr;
            ButtonAction(btn.PointToScreen(new Point(btn.Width / 2, btn.Height / 2)), true, null);
        }

        private void bmMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            Config.NewKeyValue("sysMenuID", DrCurrent["SysMenuID"]);
            Config.NewKeyValue("MenuName", DrCurrent["MenuName"]);
            try
            {
                _cmd.ShowTable(DrCurrent, (FormAction)e.Item.Tag);
            }
            catch (Exception ex)
            {
                LogFile.UnknowError(ex);
            }
        }

        private void navBarControl_LinkClicked(object sender, NavBarLinkEventArgs e)
        {
            DataRow dr = e.Link.Item.Tag as DataRow;
            if (dr == null)
                return;
            DrCurrent = dr;
            ButtonAction(Control.MousePosition, true, null);
        }

        public void ButtonAction(Point p, bool isClick, DataTable dtModule)
        {
            if (!isClick && DrCurrent["sysTableID"].ToString() != "")
                //&& Int32.Parse(DrCurrent["Type"].ToString()) != 1
                //&& Int32.Parse(DrCurrent["Type"].ToString()) != 4)
            {
                bool v = Config.GetValue("Language").ToString() == "0";
                string c1 = v ? "Xem trong kỳ" : "View in this period";
                string c2 = v ? "Xem tất cả" : "View all data";
                int type = Int32.Parse(DrCurrent["Type"].ToString());
                bbiTatCa.Caption = (type == 3 || type == 7) ? c1 : c2;
                bbiThem.Visibility = (type == 5) ? BarItemVisibility.Never : BarItemVisibility.Always;
                p.X += 5;
                p.Y += 5;
                pMenu.ShowPopup(p);
            }
            else
                if (isClick)
                {
                    pMenu.HidePopup();
                    _cmd.ExecuteCommand(DrCurrent, dtModule);
                }
        }

        private void navBarControl_HotTrackedLinkChanged(object sender, NavBarLinkEventArgs e)
        {
            if (e.Link != null)
            {
                DataRow dr = e.Link.Item.Tag as DataRow;
                if (dr == null)
                    return;
                DrCurrent = dr;
                _delayPopup = 0;
                timer1.Tag = e.Link;
                timer1.Enabled = true;
            }
            else
            {
                timer1.Tag = null;
                if (pMenu.Opened && !_popupRect.Contains(Control.MousePosition))
                    pMenu.HidePopup();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            _delayPopup += 1;
            if (_delayPopup == 5)
            {
                if (timer1.Tag != null)
                {
                    ButtonAction(Control.MousePosition, false, null);
                }
                _delayPopup = 0;
                timer1.Enabled = false;
            }
        }

        private void navBarControl2_MouseUp(object sender, MouseEventArgs e)
        {
            NavBarHitInfo hInfo = ((NavBarControl)sender).CalcHitInfo(e.Location);
            if (hInfo.InGroupCaption && !hInfo.InGroupButton)
                hInfo.Group.Expanded = !hInfo.Group.Expanded;
        }
    }
}