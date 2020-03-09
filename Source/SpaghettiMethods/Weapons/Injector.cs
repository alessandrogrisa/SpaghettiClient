using System;
using System.Reflection;

namespace Weapons
{
    // In-Memory Injection
    class Injector
    {
        public static string run(string target)
        {
            byte[] bytes = W_Utils.GetDLLBytes(target);

            if (bytes.Length > 0)
            {
                try
                {
                    // Get PID

                    // inject
                }
                catch { }
                
                return "#!#Injection Failed";
            }
            return "#!#404 - File Not Found";
        }
    }
}
