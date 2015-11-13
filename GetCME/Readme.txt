GetCME.exe usage
----------------

The program requires .net 4.5 installed on the computer to run. 

The 4 required files are:

getcme.exe
config.txt
getcme.exe.config
ionic.zip.dll

These are supplied in a zip file - GETCME.zip.

1. place the above unzipped files into an application (AppRoot) folder; for instance C:\GetCME.


2. create suitable entries in the configuration file. Here are some example entries:

	<add key="AppRoot" value="C:\Git\GetCME" />
    <add key="WorkFolder" value="C:\GetCME" />
    <add key="ConfigFile" value="config2.txt" />
    <add key="Host" value="ftp://ftp.cmegroup.com/span/data/cme/" />
    <add key="DownloadFolder" value="Downloads" />
    <add key="DataFolder" value="C:\GetCME\Data" />
	<add key="DataSubFolder" value="InputDataFiles" />
    <add key="LogFolder" value="Log" />
    <add key="User" value="anonymous" />
    <add key="Password" value="" />
    <add key="LogFile" value="GetCME.log" />
    <add key="DeleteZips" value="true" />
    <add key="TDates" value="0;1;3;6;12" />
    <add key="FileFormat" value="cme.;.s.pa2.zip" />
    <add key="DateDecrementLimit" value="7" />
    <add key="NewFolderList" value="InputDataFiles;InputPortfolios;Scripts;SpanResults" />

	Explanation:

	-	The config file (named config.txt above, but you can call it what you want), should be placed in the approot folder.
		It contains a list of basedates in the format yyyyMMdd, e.g.20150930.

	-	The downloads and log folder are subfolders of the work folder. The data folder is given a fully qualified path as above.

	-	The User and Password values given above are suitable for the ftp host given above.

	-	The log file as named above is placed in the log folder. The log file name can be changed here.

	-	The DeleteZips value is "true" or "false". If true, zip files downloaded from CME will be deleted after being unzipped.

	-	TDates entered here give the month increments that will be applied when searching for T01 and T01 dates etc.

	-	The FileFormat values represent the first part of a file, before the date part and the last part after the date part. 
		For instance, for a date 20150930, the above entries would result in searching for the file 'cme.20150930.s.pa2.zip'

	-	The DateDecrementLimit gives the number of times the date will be decremented when searching for a missing file. 

	-	The new folder list gives the names of subfolders that will be automatically created in the basedate folders.
		

3 Insert lines into the config.txt file, based on the files that need to be downloaded and using the following format:

	<basedate>

	Here are some example entries:

	20151022
	20151026

4. Double click on GetCME in its approot folder.

Further explanatory notes
-------------------------

- Download and unzip actions will be recorded in the log.

- If the Downloads and Log folders do not already exist, they will be created in the working folder.

- If the Data folder does not already exist, it will be created in the fully specified path.

- Subfolders based on the basedate value in config.txt will be created in the Downloads and Data folders.

- If the log file does not already exist, it will be created. If the log file exists, new log data will be appended.

- If the zip files already exist, they will be overwritten.

- If the unzipped data files already exist, they will be overwritten and a comment will be added to the log.












