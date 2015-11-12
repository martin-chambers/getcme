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
                // look in all subdirectories recursively
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

        public class TDateSet
        {
            public DateTime T00 { get; set; }
            public DateTime T01 { get; set; }
            public DateTime T03 { get; set; }
            public DateTime T06 { get; set; }
            public DateTime T12 { get; set; }

            public TDateSet(DateTime t0)
            {

                // days to add must n
                T00 = t0.EndOfMonth();
                // T01 = T00 plus 29 days and go to end of month
                T01 = T00.AddDays(29).EndOfMonth();
                // T03 = T01 plus 90 days and go to end of month
                T03 = T01.AddDays(90).EndOfMonth();
                
            }

            private DateTime endOfMonth(DateTime d)
            {
                return new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
            }
        }

        private List<DateTime> getTDates(DateTime T0)
        {
            List<DateTime> tdates = new List<DateTime>();

        }


        public void Run()
        {
            // initialise FTPClient from config
            NameValueCollection CMEConfig = ConfigurationManager.GetSection("CMEConfig") as NameValueCollection;
            string host = CMEConfig["Host"];
            string approot = CMEConfig["AppRoot"];
            string workingpath = CMEConfig["WorkFolder"];
            string downloadfolder = Path.Combine(workingpath, CMEConfig["DownloadFolder"]);
            string datafolder = CMEConfig["DataFolder"];
            string logfolder = Path.Combine(workingpath, CMEConfig["LogFolder"]);
            string logfile = CMEConfig["LogFile"];
            string logpath = Path.Combine(logfolder, logfile);
            string user = CMEConfig["User"];
            string password = CMEConfig["Password"];
            List<string> folderlist = new List<string>(CMEConfig["NewFolderList"].Split(new char[] { ';' }));
            FTPClient client = new FTPClient(user, password, downloadfolder, logpath);
            List<string> lines = File.ReadAllLines(InputFile).ToList();
            foreach (string line in lines)
            {
                int comma = line.IndexOf(",");
                string basedate = line.Substring(0, comma);
                string downloadDestination = Path.Combine(downloadfolder, basedate);
                string dataDestination = Path.Combine(datafolder, basedate);
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
                        client.Unzip(filename, downloadDestination, dataDestination, deleteZips, folderlist);
                        client.Log(client.UnzipSummary(filename, dataDestination));
                    }
                    catch (Exception ex)
                    {
                        client.Log(client.UnzipErrorSummary(filename, downloadDestination, dataDestination, ex.Message));
                    }

                }
            }
            if()
            Console.WriteLine("Program execution complete");
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}