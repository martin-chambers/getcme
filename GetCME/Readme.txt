GetCME.exe usage
----------------

The 4 required files are:

getcme.exe
config.txt
getcme.exe.config
ionic.zip.dll

These are supplied in a zip file - GETCME.zip.

1. place the above unzipped files into an application (AppRoot) folder; for instance C:\GetCME.

2. create suitable entries in the configuration file. Here are some example entries:

    <add key="AppRoot" value="C:\GetCME" />
    <add key="ConfigFile" value="config.txt" />
    <add key="Host" value="ftp://ftp.cmegroup.com/span/data/cme/" />
    <add key="DownloadPath" value="Downloads" />
    <add key="WorkingFolder" value="Data" />
    <add key="LogFolder" value="Log" />
    <add key="User" value="anonymous" />
    <add key="Password" value="" />
    <add key="LogFile" value="GetCME.log" />

	The only entries which you might need to change are the AppRoot, Host, User and Password entries. The current user and password settings
	work for the host name given above. If in future, CME provides an FTP connection which does not use the 'anonymous' user, the above 
	settings would need to be changed.

3 Insert lines into the config.txt file, based on the files that need to be downloaded and using the following format:

	<basedate>,<filename>

	Here are some example entries:

	20151022,cme.20151022.e.pa2.zip
	20151026,cme.20151026.e.pa2.zip

4. Open a command window in the AppRoot folder.

5. Type GetCME


Further explanatory notes
-------------------------

- Download and unzip actions will be recorded in the log.

- If the Data, Downloads and Log folders do not already exist, they will be created in the AppRoot folder.

- Subfolders based on the basedate value in config.txt will be created in the Downloads and Data folders.

- If the log file does not already exist, it will be created. If the log file exists, new log data will be appended.

- If the zip files already exist, they will be overwritten.

- If the unzipped data files already exist, they will be overwritten and a comment will be added to the log.












