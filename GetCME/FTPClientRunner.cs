using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace GetCME
{
    public class FTPClientRunner
    {
        public string InputFile { get; set; }
        public FTPClientRunner(string inputFile)
        {
            InputFile = inputFile;
        }
        public void Run()
        {
            // initialise FTPClient from config
            var CMEConfig = ConfigurationManager.GetSection("CMEConfig") as NameValueCollection;
            string host = CMEConfig["Host"];
            string approot = CMEConfig["AppRoot"];
            string downloadPath = Path.Combine(approot, CMEConfig["DownloadPath"]);
            string dataPath = Path.Combine(approot, CMEConfig["WorkingFolder"]);
            string logfolder = Path.Combine(approot, CMEConfig["LogFolder"]);
            string logfile = CMEConfig["LogFile"];
            string logpath = Path.Combine(logfolder, logfile);
            string user = CMEConfig["User"];
            string password = CMEConfig["Password"];

            FTPClient client = new FTPClient(host, user, password, logpath);

            List<string> lines = File.ReadAllLines(InputFile).ToList();
            List<string> availableFiles = client.DirectoryListing();
            foreach (string line in lines)
            {
                int commaPosition = line.IndexOf(",");
                string basedate = line.Substring(0, commaPosition);
                string downloadDestination = Path.Combine(downloadPath, basedate);
                string dataDestination = Path.Combine(dataPath, basedate);
                string filename = line.Substring(commaPosition+1);
                // FTP download
                try
                {
                    if (availableFiles.Contains(filename))
                    {
                        client.Log(client.DownloadingSummary(filename, host, downloadDestination));
                        client.Download(downloadDestination, filename);                        
                        client.Log(client.DownloadedSummary(filename, host, downloadDestination));                        
                    }
                    else
                    {
                        client.Log("Input error: " + filename + " was not found in " + downloadDestination);
                    }
                }
                catch (Exception ex)
                {
                    client.Log(client.DownloadErrorSummary(filename, host, downloadDestination, ex.Message));
                }
                // Unzip
                try
                {
                    client.Unzip(filename, downloadDestination, dataDestination);
                    client.Log(client.UnzipSummary(filename, dataDestination));
                }
                catch (Exception ex)
                {
                    client.Log(client.UnzipErrorSummary(filename, host, dataDestination, ex.Message));
                }
            }
            client.Log("Program execution complete");
        }
    }
}
