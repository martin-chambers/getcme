using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;


namespace GetCME
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var CMEConfig = ConfigurationManager.GetSection("CMEConfig") as NameValueCollection;
                string approot = CMEConfig["AppRoot"];
                string configFile = CMEConfig["ConfigFile"];
                string configFilePath = Path.Combine(approot, configFile);
                if(!File.Exists(configFilePath))
                {
                    throw new FileNotFoundException("File not found in " + approot + ": " + configFile);
                }
                FTPClientRunner runner = new FTPClientRunner(configFilePath);
                runner.Run();
            }
            // just in case anything is not handled in program code
            catch (Exception ex)
            {
                Console.WriteLine("Unlogged error: " + ex.Message);
                Console.WriteLine("Press any key to continue ...");
                Console.ReadKey();
            }
        }
    }
}
