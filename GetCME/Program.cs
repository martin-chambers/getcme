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
                FTPClientRunner runner = new FTPClientRunner(Path.Combine(approot, configFile));
                runner.Run();
            }
            // just in case anything is not handled in program code
            catch (Exception ex)
            {
                Console.WriteLine("Unlogged error:" + ex.Message);
            }
        }
    }
}
