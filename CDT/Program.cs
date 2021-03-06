using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using CDTLib;
using CDTSystem;
using ErrorManager;
using CDTControl;
using System.IO;
using System.Diagnostics;

namespace CDT
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //tuy theo moi soft co productName khac nhau
                string siteCode = "HTC"; //giá trị mặc định
                if (args.Length > 0)
                    siteCode = args[0];
                Config.NewKeyValue("SiteCode", siteCode);

                InitApp();
                SetEnvironment(siteCode);
                Login frmLogin = new Login();
                frmLogin.ShowDialog();

                //dang nhap thanh cong, bat dau su dung chuong trinh
                if (frmLogin.DialogResult != DialogResult.Cancel)
                    Application.Run(new Main(frmLogin.drUser));
            }
            catch (Exception ex)
            {
                LogFile.UnknowError(ex);
            }
        }

        private static void InitApp()
        {
            //lay style mac dinh cho form
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.UserSkins.OfficeSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();
            DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeelMain = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            defaultLookAndFeelMain.LookAndFeel.SetSkinStyle("Money Twins");
        }

        private static void SetEnvironment(string siteCode)
        {
            System.Globalization.CultureInfo CultureInfo = System.Windows.Forms.Application.CurrentCulture.Clone() as System.Globalization.CultureInfo;
            CultureInfo = new CultureInfo("en-US");
            DateTimeFormatInfo dtInfo = new DateTimeFormatInfo();
            dtInfo.LongDatePattern = "MM/dd/yyyy h:mm:ss tt";
            dtInfo.ShortDatePattern = "MM/dd/yyyy";
            CultureInfo.DateTimeFormat = dtInfo;
            System.Windows.Forms.Application.CurrentCulture = CultureInfo;

            //lay chuoi ket noi
            AppCon ac = new AppCon();
            string StructConnection = ac.GetValue("StructDb");
            if (StructConnection == "" && File.Exists("InstallerMng.exe"))
            {
                ProcessStartInfo psi = new ProcessStartInfo("InstallerMng.exe", siteCode);
                Process.Start(psi);
                Environment.Exit(0);
            }
            StructConnection = Security.DeCode(StructConnection);
            string structDb = "CDT" + ac.GetValue("ShortName");
            Config.NewKeyValue("StructDb", structDb);
            Config.NewKeyValue("StructConnection", StructConnection);
        }
    }
}