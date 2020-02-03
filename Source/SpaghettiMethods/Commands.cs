using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;

namespace CursedSpaghetti
{
    class Commands
    {
        // Change directory
        public string ChangeLocation(string cmd, string location, string url)
        {
            string bkp = location;
            cmd = cmd.Substring(3);
            cmd.Replace("\"", "");

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
                    location = String.Concat(bkp, "\\", cmd);
                }
            }

            if (Directory.Exists(location))
            {
                return location;
            }
            else
            {
                Utilities.Post(url, "#!#Directory does not exist");
                return bkp;
            }
        }

        // Run CMD Command
        public (string, string) RunCMD(string cmd, string location)
        {
            if (Utilities.CliProtection(cmd))
            {
                Process process = new Process();

                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = Utilities.IsRoot(location);
                process.StartInfo.Arguments = string.Concat("/C ", cmd);
                process.Start();
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.Close();

                return (output, error);
            }
            return ("!#!WARNING: Interactive Command Detected! CliProtection Triggered", "");
        }

        // Run PSH Commands
        public (string, string) RunPSH(string cmd, string location)
        {
            cmd = cmd.Substring(11);
            if (Utilities.CliProtection(cmd))
            {
                Runspace runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                Pipeline pipeline = runspace.CreatePipeline();
                string scriptCommand = String.Format("Set-Location -Path {0}; {1}", location, cmd);
                pipeline.Commands.AddScript(scriptCommand);
                pipeline.Commands.Add("Out-String");

                StringBuilder output = new StringBuilder();
                string error = "";

                try
                {
                    Collection<PSObject> objects = pipeline.Invoke();

                    foreach (PSObject obj in objects)
                    {
                        output.AppendLine(obj.ToString());
                    }
                }
                catch (Exception e)
                {
                    error = e.Message;
                }

                runspace.Close();
                return (output.ToString(), error);
            }
            return ("!#!WARNING: Interactive Command Detected! CliProtection Triggered", "");
        }
    }
}