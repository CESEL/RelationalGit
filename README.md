# RelationalGit
RelationalGit extracts valuable information about commits, blames, changes, devs, and pull requests out of git's data structure and imports them to a relational database such as Microsoft SQL Server. These data can be a ground for further source code mining analysis.

So, You can easily query the database and find answers to many interesing questions. Since, source code mining is one of the hottest topics in academia and industry, RelationalGit wants to help researchers to start their investigations more conveniently.
For example you can find answers to the following questions by running a simple SQL query over extracted data.

* What are the files that are recently changed by a given developer?
* Who is the author of a specific line in a specific file? (Git Blame)
* Which developer has the most commits?
* What files usually are changed together? this way you can detect and document your hidden dependencies.
* Which developer has the most knowledge about a file or project? This idea is based on [Rigby's paper](http://ieeexplore.ieee.org/document/7886975/).
* Which files are changing constantly? maybe they are bug-prone.
* Who is the most appropriate developer to work on a given file?

# Dependencies

:raised_hand: Currently, RelationalGit has been tested on `SQL Server`. Support for other popular databases (Sqllite, PostgreSQL) will be added in near future.

##  :cupid: .NET Core

You need to get the lastes bits of [.NET Core](https://www.microsoft.com/net/download).

##  :cupid: SQL Server
On Windows, you can download awesome Sql Server - [LocalDb, Express, and Developer Editions](https://www.microsoft.com/en-ca/sql-server/sql-server-downloads) - and [Sql Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) free of charge.

If you are using Linux or Mac you can download the [SQL Server Docker image](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-2017) to run it natively on your system. Then you need to install [Microsoft SQL Operation Studio](https://docs.microsoft.com/en-us/sql/sql-operations-studio/download) or [SQLCMD](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup-tools?view=sql-server-linux-2017) to query the database.

##  :cupid: PowerShell Core

You need to get the lastes version of [PowerShell Core](https://github.com/PowerShell/PowerShell/releases). RelationalGit uses PowerShell for extracting blames.


# RelationalGit :cupid: Open Source
RelationalGit has been built on top of the most popular Git Libraries. It uses [libgit2Sharp](https://github.com/libgit2/libgit2sharp), [Octokit.Net](https://github.com/octokit/octokit.net), and [Octokit.Extensions](https://github.com/mirsaeedi/octokit.net.extensions) in order to extract data from git data structure and Github respectively.

# :star: Configuration File

You need to create a configuration file with the following format. The configuration file at least needs to have _ConnectionStrings:RelationalGit_ and _Mining_ (empty) sections.

```JSON
{
  "ConnectionStrings": {
    "RelationalGit": "Server=IP;User Id=user;Password=123;Database=db"
  },
  "Mining": {
    "Extensions": [ ".cs", ".vb", ".ts", ".js", ".jsx", ".sh", ".yml", ".tsx", ".css", ".json", ".py", ".c", ".h", ".cpp", ".il", ".make", ".cmake", ".ps1", ".r", ".cmd", ".html", ".conf" ],
    "GitBranch": "master",
    "RepositoryPath": "",
    "GitHubRepo": "corefx",
    "GitHubOwner": "dotnet",
    "GitHubToken": "",
    "PeriodLength": 3,
    "PeriodType": "month",
    "MegaDevelopers": [ "dotnet-bot" ],
    "MegaCommitSize": 200,
    "FilesAtRiksOwnersThreshold": 1,
    "FilesAtRiksOwnershipThreshold": 0.90,
    "LeaversOfPeriodExtendedAbsence": 4,
    "MegaPullRequestSize": 100,
    "CoreDeveloperThreshold": 0.1,
    "CoreDeveloperCalculationType": "ownership-percentage",
    "KnowledgeSaveStrategyType": "nothing",
    "LeaversType": "all"
  }
}
```

You need to tell relational git where's your config file. If you don't, it assumes there is a configuration file in the user directory names _relationalgit.json_.

# :star: Commands

RelationalGit has several built-in commands for extracting git information and computing various knowledge loss scenarios. You can override the values you set in the configuration file by passing explicit arguments for each command. 

For example, the following lines execute the _get-github-pullrequest-reviewer-comments_ command to gather all the PR's comments of the GitHub repository which is defined via *_GitHubOwner_* and *_GitHubRepo_* values of the setting file.

```PowerShell

rgit --conf-path "C:\Users\Ehsan Mirsaeedi\Documents\relationalgit.json"  --cmd get-github-pullrequest-reviewer-comments 
rgit --cmd get-github-pullrequest-reviewer-comments // it gets the setting file from the default location

```

# :star: Git Exctraction

RelationalGit extract Commits and Blames from the git structure (the .git folder). In Addition, RelationalGit shows you detailed information about changes that happend in each commit.

Before going with the commands, let's assume that we have cloned the repository we want to examine to a folder with `{repo_path}` path. `{repo_path}` can be a reference to any cloned folder on your system.



### Commit

```
dotnet .\RelationalGit.dll -get-commits {repo_path} {branch_name}
```

This command saves the extracted data in `Commits` and `CommitRelationship` tables.

### Committed Changes

```
dotnet .\RelationalGit.dll -get-commitsChanges {repo_path} {branch_name}
```

This command saves the extracted data in `CommitChanges` table.

### Blames

```
dotnet .\RelationalGit.dll -get-blobsblames {repo_path} {branch_name} {commit_sha} {file_extensions_seperated_by_comma}
```

This command saves the extracted data in `CommittedBlob` and `CommitBlobBlames` tables.

# :star: Github Pull Requests Exctraction

RelationalGit wants to help us to have a better insight about code review practices in our project. So if you have hosted your project on Github, It can fetche and save `Pull Requests`, `Reviewers`, `Reviewer Comments`, and also `Pull Request's Files`. To use Github APIs you need to generate a token. Bear in mind that each token is allowed to sent 5000 requests per hour to Github's servers. So, for larger projects you need to wait a bit more.

### Pull Requests

```
dotnet .\RelationalGit.dll -get-pullrequests {github_token} {owner_name} {repo_name} {branch_name}
```

### Reviewers

```
dotnet .\RelationalGit.dll -get-pullrequest-reviewers {github_token} {owner_name} {repo_name} {branch_name}
```

### Reviewers' Comments

```
dotnet .\RelationalGit.dll -get-pullrequest-reviewer-comments {github_token} {owner_name} {repo_name} {branch_name}
```

### Pull Requests' Files

```
dotnet .\RelationalGit.dll get-pullrequests-files {github_token} {owner_name} {repo_name} {branch_name}
```

