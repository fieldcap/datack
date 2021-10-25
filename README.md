## Datack

Datack is a simple Database backup tool supporting currently the following databases:

-   SQL Server

It is designed to scale for a large amount of databases and tasks that can run on multiple servers.

### Getting started

Datack needs a single server frontend which acts as a command and control. It controls dumb agents through SignalR websockets.

#### Installing the server

##### On a host that will run the Datack server frontend

1. Install the latest version of .NET 5: https://dotnet.microsoft.com/download.

1. Download the latest copy of `datack server.zip` from the releases page on Github.

1. Unpack the zip and run `service-install.bat`. This will install the `Datack` service.

1. If needed, change `appsettings.json` to change the path to store logs and the database file. By default they will be written to `/data/`.

1. Open your browser to http://localhost:3000.

1. On the first run setup a username and password.

1. To update the server simply run `Update.ps1`, this will download the latest release from Github and update all files, except for `appsettings.json` and `/data`.

#### Installing the agent

##### On a host that will run backup tasks

1. Install the latest version of .NET 5: https://dotnet.microsoft.com/download.

1. Download the latest copy of `datack agent.zip` from the releases page on Github.

1. Unpack the zip and run `service-install.bat`. This will install the `Datack Agent` service.

1. If needed, change `appsettings.json` to change the path to store logs. By default they will be written to `/data/`. The `token` property can be set here to a GUID value, if empty a new token will be generated on startup.
