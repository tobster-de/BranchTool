using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BranchTool
{
    class Git
    {

        internal static bool StartGit(string arguments, out string output)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "git.exe";
                startInfo.Arguments = arguments;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                using (Process process = Process.Start(startInfo))
                {
                    // Read in all the text from the process with the StreamReader.
                    using (StreamReader reader = process.StandardOutput)
                    {
                        output = reader.ReadToEnd();
                    }
                    using (StreamReader reader = process.StandardError)
                    {
                        string result = reader.ReadToEnd();
                        output += result;
                    }

                    process.WaitForExit();// Waits here for the process to exit.            

                    return process.ExitCode == 0;
                }

            }
            catch (Exception e)
            {
                output = e.Message;
                return false;
            }
        }
    }
}
