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

# :star: Install (dotnet Global Tool)

RelationalGit is a [dotnet Global tool](https://www.nuget.org/packages/RelationalGit). You can use it seamlessly with your favorite command-line application.

```PowerShell
dotnet tool install --global RelationalGit
```

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
    "LeaversType": "all",
    "BlamePeriods": [1],
    "BlamePeriodsRange":[10,20],
    "ExcludedBlamePaths": ["*\\lib\\*"]
  }
}
```

You need to tell relational git where's your config file. If you don't, it assumes there is a configuration file in the user directory names _relationalgit.json_.

# :star: Commands

RelationalGit has several built-in commands for extracting git information and computing various knowledge loss scenarios. You can override the values you set in the configuration file by passing explicit arguments for each command. 

For example, the following lines execute the _get-github-pullrequest-reviewer-comments_ command to gather all the PR's comments of the GitHub repository which is defined via *_GitHubOwner_* and *_GitHubRepo_* values of the setting file.

```PowerShell

dotnet-rgit --conf-path "C:\Users\Ehsan Mirsaeedi\Documents\relationalgit.json"  --cmd get-github-pullrequest-reviewer-comments 
dotnet-rgit --cmd get-github-pullrequest-reviewer-comments // it gets the setting file from the default location

```

Below is the complete list of commands.

### get-git-commits
  
Extract the commits from the repository refrenced by _RepositoryPath_ parameter. And Fill the _Commits_ table.
  
### get-git-commits-changes

Extract the introduced changes of each commit from the repository refrenced by _RepositoryPath_ parameter. And Fill the _CommittedChanges_ and _CommitRelationship_ tables. It detects rename operations and assign a canonical path to the files to make it possible to track renames.

### alias-git-names

Try to resolve multiple developer names confusion by finding unique users from the repository refrenced by _RepositoryPath_ parameter. It fills the _AliasedDeveloperNames_ table. You can manually edit this table' data to do the final touches.

### apply-git-aliased

Fills the _NormalizedAuthorName_ of -Commits_ table and _NormalizedDeveloperIdentity_ of _CommittedChanges_ table with normalized name computed by _alias-git-names_ command.

### ignore-mega-commits
### periodize-git-commits
### extract-dev-info
### get-git-commit-blames-for-periods
### get-git-commit-blames
### get-github-pullrequests
### get-github-pullrequest-reviewers
### get-merge-events
### get-github-pullrequest-reviewer-comments
### get-github-pullrequests-files
### get-pullrequest-issue-comments
### map-git-github-names
### compute-loss

# Complete Data Gathering Sample

for a complete data gathering one can run a following script, assuming the setting file is located at the default location (User Directory \ relationalgit.json) and all the required setting values are set.

```PowerShell
dotnet-rgit --cmd get-git-commits
dotnet-rgit --cmd get-git-commits-changes
dotnet-rgit --cmd ignore-mega-commits
dotnet-rgit --cmd periodize-git-commits
dotnet-rgit --cmd get-git-commit-blames-for-periods
dotnet-rgit --cmd ignore-mega-commits
dotnet-rgit --cmd alias-git-names
dotnet-rgit --cmd apply-git-aliased
dotnet-rgit --cmd extract-dev-info
dotnet-rgit --cmd get-github-pullrequests
dotnet-rgit --cmd get-github-pullrequest-reviewers
dotnet-rgit --cmd get-github-pullrequest-reviewer-comments
dotnet-rgit --cmd get-github-pullrequests-files
dotnet-rgit --cmd get-merge-events
dotnet-rgit --cmd get-pullrequest-issue-comments
dotnet-rgit --cmd map-git-github-names
dotnet-rgit --cmd compute-loss
```
