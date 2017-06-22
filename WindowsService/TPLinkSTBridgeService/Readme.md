# TP-Link SmartThings Bridge Windows Service

Version 0.1.

06-22-2017 - Initial Release.

This is a Windows service that replaces the *TP-Link_Server.js* Node script, and removes the need to setup an item in the Windows schedule to run when the machine is started.  Instead, it installs as a standard Windows service, which is automatically started as soon as Windows starts.

## Pre-Requisites ##

The application uses .NET 4.6.2 so this must be installed on the machine.  It is already installed as part of Windows 10.  Older versions of Windows can be updated using the .NET installer: [https://www.microsoft.com/en-us/download/details.aspx?id=53345](https://www.microsoft.com/en-us/download/details.aspx?id=53345).

## Building the solution ##

The solution requires Visual Studio 2017 to be installed.  To build, execute the *\_build.cmd* batch file.

## Installing the service ##

To install the service, download the TPSLinkToSmartThingsBridgeService.zip file from the releases tab.  Unzip this to a folder, like *c:\Utils\TPLinkSmartThingsBridgeServer*, and then execute the *\_install.cmd* batch file to install and start the Windows Service.

Note, the batch file must be executed using elevated privileges.

## Removing the service ##

To remove the service, go to the folder where the service was unzipped - eg. *c:\Utils\TPLinkSmartThingsBridgeServer*, and execute the *\_uninstall.cmd* batch file.

Note, the batch file must be executed using elevated privileges.