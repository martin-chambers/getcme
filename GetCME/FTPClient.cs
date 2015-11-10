using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Ionic.Zip;

namespace GetCME
{
    public class FTPClient
    {
        // The hostname or IP address of the FTP server
        private string _remoteHost;
        // The remote username
        private string _remoteUser;
        // Password for the remote user
        private string _remotePass;
        private string _downloadfolder;
        private string _logpath;
        private byte[] downloadedData;

        public FTPClient(string remoteHost, string remoteUser, string remotePassword, string logpath)
        {
            _remoteHost = remoteHost;
            _remoteUser = remoteUser;
            _remotePass = remotePassword;
            _logpath = logpath;
        }
        public FTPClient(string remoteHost, string remoteUser, string remotePassword, string downloadfolder, string logpath) 
            : this(remoteHost, remoteUser, remotePassword, logpath)
        {
            _downloadfolder = downloadfolder;
        }

        public List<string> DirectoryListing()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_remoteHost);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(_remoteUser, _remotePass);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            List<string> result = new List<string>();
            while (!reader.EndOfStream)
            {
                result.Add(reader.ReadLine());
            }
            reader.Close();
            response.Close();
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

        public void Download(string fileToDownload)
        {
            // check the download folder exists and if not, create it
            folderCheckAndCreate(_downloadfolder);

            //Get the file size first (for progress tracking)
            FtpWebRequest request = FtpWebRequest.Create(_remoteHost + fileToDownload) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(_remoteUser, _remotePass);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true; //don't close the connection

            int dataLength = (int)request.GetResponse().ContentLength;

            //Now get the actual data
            request = FtpWebRequest.Create(_remoteHost + fileToDownload) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_remoteUser, _remotePass);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false; //close the connection when done

            //Streams
            FtpWebResponse response = request.GetResponse() as FtpWebResponse;
            Stream reader = response.GetResponseStream();

            //Download to memory
            MemoryStream memStream = new MemoryStream();
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

            WriteDataToFile(fileToDownload);

            //Clean up
            reader.Close();
            memStream.Close();
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
        public string UnzipErrorSummary(string filename, string host, string dataDestination, string message)
        {
            return "ERROR unzipping " + filename + " from " + host + " to " + dataDestination + ". (" + message + ")";
        }


        public void Download(string downloadfolder, string fileToDownload)
        {
            _downloadfolder = downloadfolder;
            Download(fileToDownload);
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
            // allow for the possibility that there are multiple files in the zip archive
            using (ZipFile zip = ZipFile.Read(zipFilePath))
            {
                foreach (ZipEntry entry in zip)
                {
                    string filePath = Path.Combine(unzipDestinationFolder, entry.FileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Log("Deleted pre-existing version of file: " + entry.FileName + " in " + unzipDestinationFolder);
                    }
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
