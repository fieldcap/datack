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

1. Unpack the zip to a directory of your choice.

1. Open `appsettings.json` and set the `ServerUrl` setting to the URL of the server. Optionally you can change the path to store logs. By default they will be written to `/data/`. The `token` property can be set here to a GUID value, if empty a new token will be generated on startup.

1. Run `service-install.bat`. This will install the `Datack Agent` service.

#### First steps

To get started, first add your agent to the server. Open `appsettings.json` of the agent and copy the `Token` property (this is automatically set when ran for the first time).

On the server click `Agents` and `Add Agent`. Make sure to add the token here. After saving restart the agent.

Click `Jobs` to start adding jobs.

### Jobs

A job is defined as a series of tasks. The job is not connected to an agent but can have multiple agents (i.e. backup a database and restore it on another server).

### Job runs

When a job is executed it will use the very first task to create a list of items. Normally this is the `Create Backup` task and will generate a list of all the databases that are defined in the task. This list of items will be used throughout the job run. The output of a task, called an artifact, can be used as the input for another task.

### Parallel tasks

When the `Parallel` parameter is set on a task it will attempt to start the task multiple times per item. When a task is completed for an item it will proceed to the next task for that item, but it will never start more than `Parallel` amount of items for the task.

### Tasks

Some tasks store sensitive information like access keys or passwords. These secrets are encrypted with a key that lives on the agent for the task. If you change the agent for a task or re-install an agent, the secret will need to be re-entered again. The agent also needs to be online to be able to store the settings.

Datack supports the following tasks:

#### Create backup

This task will create a backup of a database. Use the settings to define which databases need to be backed up. The list of databases that are not striked through will be backed up.
