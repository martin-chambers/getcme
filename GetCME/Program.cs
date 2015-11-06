using System;
using System.Collections.Specialized;
using System.Configuration;


namespace GetCME
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var CMEConfig = ConfigurationManager.GetSection("CMEConfig") as NameValueCollection;
            string host = CMEConfig["Host"];
            string downloadDestination = CMEConfig["DownloadsFolder"];
            string logfolder = CMEConfig["LogFolder"];
            string unzipDestinationFolder = CMEConfig["UnzipDestinationFolder"];
            string logfile = CMEConfig["LogFile"];
            //string downloadFile = args[0];
            // for testing:
            //string downloadFile = "cme.a.pa2.zip";
            string downloadFile = "no_such_file";
            FTPClient client = new FTPClient(host, "anonymous", "", downloadDestination, logfolder);
            try
            {
                client.Download(downloadFile);
                client.Log("Downloaded " + downloadFile + " from " + host + " to " + downloadDestination, logfile);
            }
            catch (Exception ex)
            {
                client.Log("ERROR downloading " + downloadFile + " from " + host + " to " + downloadDestination + ". (" + ex.Message + ")", logfile);
            }
            try
            {
                client.Unzip(downloadFile, downloadDestination, unzipDestinationFolder);
                client.Log("Unzipped " + downloadFile + " to " + unzipDestinationFolder, logfile);
                Console.WriteLine("Actions completed");
            }
            catch (Exception ex)
            {
                client.Log("ERROR unzipping " + downloadFile + " from " + host + " to " + downloadDestination + ". (" + ex.Message + ")", logfile);
            }

        }
    }
}
