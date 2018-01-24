# RelationalGit
RelationalGit extracts valuable information about commits, blames, changes, devs, and pull requests from git data structure and import them to a relational structure. These data can be a ground for further source code mining analysis.

For example you can find answers to following questions by running simple Sql queries:

* Which developer has the most commits?
* Which developer has the most knowledge about a file or project?
* Which files are changing constantly? maybe they are bug-prone.

RelationalGit imports the extracted data to a relational database. So, You can easily query the database and find answers to many interesing questions. Todays source code mining is one of the hottest topics in academia and RelationalGit wants to helps researchers to start their investigations more conveniently.

:raised_hand: Currently, RelationalGit has been tested on `SQL Server`. Support for other popular databases (Sqllite, PostgreSQL) will be added.

:heart_eyes: You can download awesome Sql Server - LocalDb, Express, and Developer Editions - and Sql Server Management Studio free of charge. Sql Server 2017 is cross platform and also docker images are available.

# RelationalGit :cupid: .NET

RelationalGit has been implemented in awsesome C# (dotnet core 2.0) and it's cross platform. So you can easily run it on your platform of choice.

# RelationalGit :cupid: Open Source
RelationalGit has been built on top of the most popular Git Libraries. It uses libgit2Sharp and Octokit.Net in order to extract data from git data structure and Github respectively.

# :star: Setup

To be able to use RelationalGit you need to go through some preliminary steps.

0. Install free and awesome dotnet core 2
1. Extract RelationalGit on a folder on your system
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

### Committed Changes

```
dotnet .\RelationalGit.dll -get-commitsChanges {repo_path} {branch_name}
```

### Blames

```
dotnet .\RelationalGit.dll -get-blobsblames {repo_path} {branch_name} {commit_sha} {file_extensions_seperated_by_comma}
```

# :star: Github Pull Requests Exctraction

RelationalGit wants to help us to have a better insight about code review practices in our project. So, It fetches and saves `Pull Requests`, `Reviewers`, `Reviewer Comments`, and also `Pull Request's Files`.

