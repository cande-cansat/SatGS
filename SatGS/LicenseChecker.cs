using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SatGS
{
    internal static class LicenseChecker
    {
        private static string GetXceedLicenseKey()
        {
            string[] licenseFiles =
            {
#if DEBUG
                "../../XceedLicense.cfg",
#endif
                "XceedLicense.cfg"
            };

            string LicenseKey = "";
            foreach (var licenseFile in licenseFiles)
            {
                if (!File.Exists(licenseFile)) continue;
                using (var file = new StreamReader(File.OpenRead(licenseFile)))
                {
                    LicenseKey = file.ReadLine();
                }
            }

            if (string.IsNullOrEmpty(LicenseKey))
            {
                MessageBox.Show("There is no license file for Xceed.Wpf (XceedLicense.cfg)");
                Environment.Exit(-1);
            }

            return LicenseKey;
        }

        private static void CheckXceedLicense()
        {
            Xceed.Wpf.Toolkit.Licenser.LicenseKey = GetXceedLicenseKey();
            Xceed.Wpf.Toolkit.LicenseChecker.CheckLicense();
        }

        public static void CheckLicense()
        {
            CheckXceedLicense();
        }
    }
}
