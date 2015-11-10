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
        private NameValueCollection CMEConfig = ConfigurationManager.GetSection("CMEConfig") as NameValueCollection;
        public FTPClientRunner(string inputFile)
        {
            InputFile = inputFile;
        }
        public void Run()
        {
            // initialise FTPClient from config
            string host = CMEConfig["Host"];
            string approot = CMEConfig["AppRoot"];
            string downloadPath = Path.Combine(approot, CMEConfig["DownloadPath"]);
            string dataPath = Path.Combine(approot, CMEConfig["WorkingFolder"]);
            string logfolder = Path.Combine(approot, CMEConfig["LogFolder"]);
            string logfile = CMEConfig["LogFile"];
            string logpath = Path.Combine(logfolder, logfile);
            string user = CMEConfig["User"];
            string password = CMEConfig["Password"];
            List<string> lines = File.ReadAllLines(InputFile).ToList();
            List<string> urls = lines.Select(x => x.Substring(0, x.IndexOf(","))).ToList();
            List<string> distinctUrls = urls.Distinct().ToList();
            foreach (string url in distinctUrls)
            {
                FTPClient client = new FTPClient(url, user, password, logpath);
                List<string> availableFiles = client.DirectoryListing();
                foreach (string line in lines.Where(x => x.Substring(0, x.IndexOf(",")) == url))
                {
                    int comma1 = line.IndexOf(",");
                    int comma2 = line.LastIndexOf(",");
                    string basedate = line.Substring(comma1 + 1, (comma2 - comma1) - 1);
                    string downloadDestination = Path.Combine(downloadPath, basedate);
                    string dataDestination = Path.Combine(dataPath, basedate);
                    string filename = line.Substring(comma2 + 1);
                    // FTP download
                    try
                    {
                        if (availableFiles.Contains(filename))
                        {
                            client.Log(client.DownloadingSummary(filename, url, downloadDestination));
                            client.Download(downloadDestination, filename);
                            client.Log(client.DownloadedSummary(filename, url, downloadDestination));
                        }
                        else
                        {
                            client.Log("Input error: " + filename + " was not found in " + url);
                        }
                    }
                    catch (Exception ex)
                    {
                        client.Log(client.DownloadErrorSummary(filename, host, downloadDestination, ex.Message));
                    }
                    // Unzip
                    try
                    {
                        bool deleteZips = Convert.ToBoolean(CMEConfig["DeleteZips"]);
                        client.Unzip(filename, downloadDestination, dataDestination, deleteZips);
                        client.Log(client.UnzipSummary(filename, dataDestination));
                    }
                    catch (Exception ex)
                    {
                        client.Log(client.UnzipErrorSummary(filename, host, dataDestination, ex.Message));
                    }

                }
            }            
            Console.WriteLine("Program execution complete");
        }
    }
}