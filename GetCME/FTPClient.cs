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
        private string _logfolder;
        private byte[] downloadedData;
        public FTPClient(string remoteHost, string remoteUser, string remotePassword, string downloadfolder, string logfolder)
        {
            _remoteHost = remoteHost;
            _remoteUser = remoteUser;
            _remotePass = remotePassword;
            _downloadfolder = downloadfolder;
            _logfolder = logfolder;
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

            Console.WriteLine("Downloading File " + fileToDownload + " from " + _remoteHost);

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

        
        public void WriteDataToFile(string writefile)
        {
            if (downloadedData != null && downloadedData.Length != 0)
            {
                Console.WriteLine("Saving Data...");
                //Write the bytes to a file
                FileStream newFile = new FileStream(Path.Combine(_downloadfolder, writefile), FileMode.Create);
                newFile.Write(downloadedData, 0, downloadedData.Length);
                newFile.Close();
                Console.WriteLine("Zip file: '" + writefile + "' written to " + _downloadfolder);
            }
            else
            {
                throw new Exception("The server did not report that the file '" + writefile + "' does not exist, but no data was downloaded");
            }
        }

        public void Log(string logtext, string logfilename)
        {
            folderCheckAndCreate(_logfolder);
            string timeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (StreamWriter w = File.AppendText(Path.Combine(_logfolder, logfilename)))
            {
                w.WriteLine(timeString + " " + logtext);
            }
        }

        public void Unzip(string zipFileName, string zipSourceFolder, string unzipDestinationFolder)
        {
            folderCheckAndCreate(unzipDestinationFolder);
            // allow for the possibility that there are multiple files in the zip archive
            using (ZipFile zip = ZipFile.Read(Path.Combine(zipSourceFolder, zipFileName)))
            {
                foreach (ZipEntry entry in zip)
                {
                    entry.Extract(unzipDestinationFolder);
                }
            }
        }

    }
}
