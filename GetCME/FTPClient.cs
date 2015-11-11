using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Ionic.Zip;

namespace GetCME
{
    public class FTPClient
    {
        // The remote username
        private string _remoteUser;
        // Password for the remote user
        private string _remotePass;
        private string _downloadfolder;
        private string _logpath;
        private byte[] downloadedData;

        public FTPClient(string remoteUser, string remotePassword, string logpath)
            : this(remoteUser, remotePassword)
        {
            _logpath = logpath;
        }
        public FTPClient(string remoteUser, string remotePassword)
        {
            _remoteUser = remoteUser;
            _remotePass = remotePassword;
        }
        public FTPClient(string remoteUser, string remotePassword, string downloadfolder, string logpath) 
            : this(remoteUser, remotePassword, logpath)
        {
            _downloadfolder = downloadfolder;
        }

        public List<string> DirectoryListing(string url)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.ListDirectory;            
            request.Credentials = new NetworkCredential(_remoteUser, _remotePass);
            List<string> result = new List<string>();
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();              
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    while (!reader.EndOfStream)
                    {
                        result.Add(reader.ReadLine());
                    }
                }
            }
            //response.Close();
            return result;
        }

        private void folderCheckAndCreate(string path)
        {
            bool exists = Directory.Exists(path);
            if (!exists)
            {
                Directory.CreateDirectory(path);
            }
        }
        private void folderCheckAndDelete(string path)
        {
            bool exists = Directory.Exists(path);
            if (exists)
            {
                Directory.Delete(path, true);
            }
        }

        public void Download(string fileToDownload, string url)
        {
            if(_downloadfolder == "")
            {
                throw new InvalidOperationException("The download folder has not been intialised");
            }

            // check the download folder exists and if not, create it
            folderCheckAndCreate(_downloadfolder);

            FtpWebRequest request = FtpWebRequest.Create(url + fileToDownload) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(_remoteUser, _remotePass);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true; //don't close the connection

            int dataLength = (int)request.GetResponse().ContentLength;  // could use this later for progress-checking

            //Now get the actual data
            request = FtpWebRequest.Create(url + fileToDownload) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_remoteUser, _remotePass);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false; //close the connection when done

            //Streams
            FtpWebResponse response = request.GetResponse() as FtpWebResponse;
            using (Stream reader = response.GetResponseStream())
            {

                //Download to memory
                using (MemoryStream memStream = new MemoryStream())
                {
                    byte[] buffer = new byte[1024]; //downloads in chunks
                    int i = 0;
                    string comfort = "";
                    while (true)
                    {
                        //Try to read the data
                        int bytesRead = reader.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            //Nothing was read, finished downloading
                            break;
                        }
                        else
                        {
                            if (i % 200 == 0)
                            {
                                comfort = (i % 400 == 0) ? "\rWorking ...\r" : "\r           \r";
                            }
                            // animation!
                            Console.Write(comfort);
                            i++;
                            //Write the downloaded data
                            memStream.Write(buffer, 0, bytesRead);
                        }
                    }

                    //Convert the downloaded stream to a byte array
                    downloadedData = memStream.ToArray();
                    memStream.Close();
                }
                reader.Close();
            }
            WriteDataToFile(fileToDownload);
            response.Close();            
        }

        public string DownloadingSummary(string filename, string host, string downloadDestination)
        {
            return "Downloading: " + filename + " from " + host + " to " + downloadDestination;
        }
        public string DownloadedSummary(string filename, string host, string downloadDestination)
        {
            return "Completed download: " + filename + " from " + host + " to " + downloadDestination;
        }
        public string DownloadErrorSummary(string filename, string host, string downloadDestination, string message)
        {
            return "Error downloading " + filename + " from " + host + " to " + downloadDestination + ". (" + message + ")";
        }
        public string UnzipSummary(string filename, string dataDestination)
        {
            return "Unzipped " + filename + " to " + dataDestination;
        }
        public string UnzipErrorSummary(string filename, string downloadfolder, string dataDestination, string message)
        {
            return "ERROR unzipping " + filename + " from " + downloadfolder + " to " + dataDestination + ". (" + message + ")";
        }


        public void Download(string downloadfolder, string fileToDownload, string url)
        {
            _downloadfolder = downloadfolder;
            Download(fileToDownload, url);
        }

        
        public void WriteDataToFile(string writefile)
        {
            if (downloadedData != null && downloadedData.Length != 0)
            {
                //Write the bytes to a file
                using (FileStream newFile = new FileStream(Path.Combine(_downloadfolder, writefile), FileMode.Create))
                {
                    newFile.Write(downloadedData, 0, downloadedData.Length);
                }
                
            }
            else
            {
                throw new Exception("The server did not report that the file '" + writefile + "' does not exist, but no data was downloaded");
            }
        }

        public void Log(string logtext)
        {
            string logfolder = Utility.RemoveFilenameFromFilepath(_logpath);
            folderCheckAndCreate(logfolder);
            string timeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (StreamWriter w = File.AppendText(_logpath))
            {
                w.WriteLine(timeString + " " + logtext);
            }
            Console.WriteLine(logtext);
        }

        public void Unzip(string zipFileName, string zipSourceFolder, string unzipDestinationFolder, bool deleteZips)
        {
            folderCheckAndCreate(unzipDestinationFolder);
            string zipFilePath = Path.Combine(zipSourceFolder, zipFileName);
            string destinationFilePath = Path.Combine(unzipDestinationFolder, zipFileName).Replace(".zip", "");
            try
            {
                using (Stream stream = new FileStream(zipFilePath, FileMode.Open))
                {
                    // File/Stream manipulating code here
                }
            }
            catch
            {
                Log(zipFileName + " is in use");
            }
            if (File.Exists(destinationFilePath))
            {
                File.Delete(destinationFilePath);
                Log("Deleted pre-existing version of file: " + zipFileName + " in " + unzipDestinationFolder);
            }
            using (ZipFile zip = ZipFile.Read(zipFilePath))
            {
                // allow for the possibility that there are multiple files in the zip archive
                foreach (ZipEntry entry in zip)
                {
                    entry.Extract(unzipDestinationFolder);                                        
                }
            }
            if (deleteZips)
            {
                File.Delete(zipFilePath);
            }
        }
    }
}
