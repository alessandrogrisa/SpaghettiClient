using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

class Weapons
{
    // basic system check
    public static void ListBasicOSInfo()
    {
        // returns basic OS/host information, including:
        //      Windows version information
        //      integrity/admin levels
        //      processor count/architecture
        //      basic user and domain information
        //      whether the system is a VM
        //      etc.

        string ProductName = GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "ProductName");
        string EditionID = GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "EditionID");
        string ReleaseId = GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "ReleaseId");
        string BuildBranch = GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "BuildBranch");
        string CurrentMajorVersionNumber = GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "CurrentMajorVersionNumber");
        string CurrentVersion = GetRegValue("HKLM", "Software\\Microsoft\\Windows NT\\CurrentVersion", "CurrentVersion");

        bool isHighIntegrity = IsHighIntegrity();
        //bool isLocalAdmin = IsLocalAdmin();

        string arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
        string userName = Environment.GetEnvironmentVariable("USERNAME");
        string ProcessorCount = Environment.ProcessorCount.ToString();
        //bool isVM = IsVirtualMachine();

        DateTime now = DateTime.UtcNow;
        DateTime boot = now - TimeSpan.FromMilliseconds(Environment.TickCount);
        DateTime BootTime = boot + TimeSpan.FromMilliseconds(Environment.TickCount);

        String strHostName = Dns.GetHostName();
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        string dnsDomain = properties.DomainName;

        Console.WriteLine("\r\n\r\n=== Basic OS Information ===\r\n");
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "Hostname", strHostName));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "Domain Name", dnsDomain));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "Username", WindowsIdentity.GetCurrent().Name));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "ProductName", ProductName));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "EditionID", EditionID));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "ReleaseId", ReleaseId));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "BuildBranch", BuildBranch));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "CurrentMajorVersionNumber", CurrentMajorVersionNumber));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "CurrentVersion", CurrentVersion));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "Architecture", arch));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "ProcessorCount", ProcessorCount));
        //Console.WriteLine(String.Format("  {0,-30}:  {1}", "IsVirtualMachine", isVM));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "BootTime (approx)", BootTime));
        Console.WriteLine(String.Format("  {0,-30}:  {1}", "HighIntegrity", isHighIntegrity));
        //Console.WriteLine(String.Format("  {0,-30}:  {1}", "IsLocalAdmin", isLocalAdmin));
        /*
        if (!isHighIntegrity && isLocalAdmin)
        {
            Console.WriteLine("    [*] In medium integrity but user is a local administrator- UAC can be bypassed.");
        }
        */
    }

    private static string GetRegValue(string hive, string path, string value)
    {
        // returns a single registry value under the specified path in the specified hive (HKLM/HKCU)
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

    private static bool IsHighIntegrity()
    {
        // returns true if the current process is running with adminstrative privs in a high integrity context
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

}