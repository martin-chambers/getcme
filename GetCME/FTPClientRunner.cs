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

        private string getUrlForFile(string fileToSearch, string url, string user, string password, FTPClient client)
        {
            string foundUrl = "";
            List<string> fileEntries = client.DirectoryListing(url);
            if (fileEntries.Contains(fileToSearch))
            {
                return url;
            }
            else
            {
                // look in all subdirectories
                foreach (string fileEntry in fileEntries.Where(x => x.Contains(".") == false))
                {
                    if (foundUrl == "")
                    {
                        foundUrl = getUrlForFile(fileToSearch, url + fileEntry + "/", user, password, client);
                    }
                }

            }
            return foundUrl;
        }


        public void Run()
        {
            // initialise FTPClient from config
            NameValueCollection CMEConfig = ConfigurationManager.GetSection("CMEConfig") as NameValueCollection;
            string host = CMEConfig["Host"];
            string approot = CMEConfig["AppRoot"];
            string downloadPath = Path.Combine(approot, CMEConfig["DownloadPath"]);
            string dataPath = Path.Combine(approot, CMEConfig["WorkingFolder"]);
            string logfolder = Path.Combine(approot, CMEConfig["LogFolder"]);
            string logfile = CMEConfig["LogFile"];
            string logpath = Path.Combine(logfolder, logfile);
            string user = CMEConfig["User"];
            string password = CMEConfig["Password"];
            FTPClient client = new FTPClient(user, password, downloadPath, logpath);
            List<string> lines = File.ReadAllLines(InputFile).ToList();
            foreach (string line in lines)
            {
                int comma = line.IndexOf(",");
                string basedate = line.Substring(0, comma);
                string downloadDestination = Path.Combine(downloadPath, basedate);
                string dataDestination = Path.Combine(dataPath, basedate);
                string filename = line.Substring(comma + 1);
                string url = getUrlForFile(filename, host, user, password, client);
                if (url == "")
                {
                    client.Log("Input error: " + filename + " was not found in " + url + " or any of the subfolders");
                }
                else
                {
                    // FTP download
                    try
                    {
                        client.Log(client.DownloadingSummary(filename, url, downloadDestination));
                        client.Download(downloadDestination, filename, url);
                        client.Log(client.DownloadedSummary(filename, url, downloadDestination));
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
                        client.Log(client.UnzipErrorSummary(filename, downloadDestination, dataDestination, ex.Message));
                    }

                }
            }
            Console.WriteLine("Program execution complete");
        }
    }
}