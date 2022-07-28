using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.License
{
    internal static class LicenseChecker
    {
        private static string GetXceedLicenseKey()
        {
            return Properties.Resources.XceedLicense;
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
