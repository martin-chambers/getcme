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

        private static int stringToInt(string i)
        {
            return Convert.ToInt32(i);
        }



        public void Run()
        {
            // get FTPClient config values
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

            // get details of fileformat. Must have length 2, to give a pre-date and post-date part 
            string[] fileformat = CMEConfig["FileFormat"].Split(new char[] { ';' });
            if (fileformat.Length != 2)
            {
                throw new InvalidOperationException("Configuration problem: file format is unexpected");
            }
            string firstFilePart = fileformat[0];
            string lastFilePart = fileformat[1];

            // get date search logic values 
            int[] months =
                Array.ConvertAll(
                    CMEConfig["TDates"].Split(new char[] { ';' }),
                    new Converter<string, int>(stringToInt));
            int decrementLimit = Convert.ToInt32(CMEConfig["DateDecrementLimit"]);
            // get subfolders to create in basedate folder
            List<string> folderlist = new List<string>(CMEConfig["NewFolderList"].Split(new char[] { ';' }));

            // create FTP client
            FTPClient client = new FTPClient(user, password, downloadfolder, logpath);

            // read the config file
            List<string> lines = File.ReadAllLines(InputFile).ToList();
            foreach (string line in lines)
            {
                string basedate = line;
                DateTime startdate = new DateTime(
                    Convert.ToInt32(basedate.Substring(0, 4)),
                    Convert.ToInt32(basedate.Substring(4, 2)),
                    Convert.ToInt32(basedate.Substring(6, 2))
                );
                TDateSet tdates = new TDateSet(startdate, months);
                for (int i = 0; i < tdates.Length; i++)
                {
                    string url = "";
                    string downloadDestination = Path.Combine(downloadfolder, basedate);
                    string dataDestination = Path.Combine(datafolder, basedate);
                    DateTime searchDate = tdates.Dates[i];
                    string filename = "";
                    int d = 0;
                    while (url == "" && d <= decrementLimit)
                    {
                        filename = firstFilePart + searchDate.ToString("yyyyMMdd") + lastFilePart;
                        url = getUrlForFile(filename, host, user, password, client);
                        if(url == "")
                        {
                            client.Log("Could not find file " + filename + ". Decrementing date ...");
                        }
                        searchDate = searchDate.Decrement();
                        d++;
                    }
                    // still not found ?
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
            }
            Console.WriteLine("Program execution complete");
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}