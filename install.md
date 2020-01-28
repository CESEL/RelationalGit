# Dependencies

Before installing RelationalGit, you need to install the following dependencies.

## 1) .NET Core

You need to get the latests bits of [.NET Core](https://www.microsoft.com/net/download).

## 2) Microsoft SQL Server

There are two ways for installing Microsoft SQL Server.

1. **Docker Image**: Download the [SQL Server Docker image](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-2017) to run it natively on your system.

2. **Windows**: You can download Sql Server - [LocalDb, Express, and Developer Editions](https://www.microsoft.com/en-ca/sql-server/sql-server-downloads).

After installing Microsoft SQL Server, you need to install [Sql Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) to query the database.

## 3) PowerShell Core

You need to get the latest version of [PowerShell Core](https://github.com/PowerShell/PowerShell/releases). RelationalGit uses PowerShell for extracting git blame information.

# Install

RelationalGit is a [dotnet Global tool](https://www.nuget.org/packages/RelationalGit). You can use it seamlessly with your favorite command-line application. The following line installs our tool on your system. After installation, all interactions will be go through the command-line interface. Our tool has been downloaded over 11000 times.

```PowerShell
dotnet tool install --global RelationalGit
```
