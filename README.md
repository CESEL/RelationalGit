# RelationalGit
RelationalGit extracts valuable information about commits, blames, changes, devs, and pull requests from git data structure and import them to a relational structure. These data can be a ground for further source code mining analysis.

For example you can find answers to following questions by running simple Sql queries:

* Which developer has the most commits?
* What files usually are changed together? this way you can detect and document your hidden dependencies.
* Which developer has the most knowledge about a file or project? This idea is based on [Rigby's paper](http://ieeexplore.ieee.org/document/7886975/).
* Which files are changing constantly? maybe they are bug-prone.

RelationalGit imports the extracted data to a relational database. So, You can easily query the database and find answers to many interesing questions. Todays source code mining is one of the hottest topics in academia and RelationalGit wants to helps researchers to start their investigations more conveniently.

:raised_hand: Currently, RelationalGit has been tested on `SQL Server`. Support for other popular databases (Sqllite, PostgreSQL) will be added.

:heart_eyes: You can download awesome Sql Server - [LocalDb, Express, and Developer Editions](https://www.microsoft.com/en-ca/sql-server/sql-server-downloads) - and [Sql Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) free of charge. Sql Server 2017 is cross platform and also docker images are available.

# RelationalGit :cupid: .NET

RelationalGit has been implemented in awsesome C# (dotnet core 2.0) and it's cross platform. So you can easily run it on your platform of choice.

# RelationalGit :cupid: Open Source
RelationalGit has been built on top of the most popular Git Libraries. It uses libgit2Sharp and Octokit.Net in order to extract data from git data structure and Github respectively.

# :star: Setup

To be able to use RelationalGit you need to go through some preliminary steps.

0. Install free and awesome [dotnet core](https://www.microsoft.com/net/download/windows) 2
1. Extract [RelationalGit](https://github.com/mirsaeedi/RelationalGit/releases) on a folder on your system
2. Create a database on your favorite RDBMS
3. The Collation of your database should enforce case sensitivity. For example in SQL Server it could be Latin1_General_CS_AS.
4. Introduce the connection string of your database to RelationGit. You can do that by editing the appsettings.json file.

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

