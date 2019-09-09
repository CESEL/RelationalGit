# Replication Package

## Install the package

1) You need to install [RelationalGit](https://github.com/CESEL/RelationalGit) with all the required dependencies.
2) You need to [restore](https://www.janbasktraining.com/blog/restore-a-database-backup-from-sql/) the [backup of data](https://drive.google.com/drive/folders/1nc7Hu7kbPpavYrCMmCU5SEBlLlZTo5Fv) into Sql Server. For each studied project there is a separate database. 
3) For each project, you need to have a configuration file. It is a json file that has the connection string to the databases and the required parameters for running the simulation. You can name this file whatever you like.

```JSON
{
"ConnectionStrings": {
   "RelationalGit": "Server=IP;User Id=user;Password=123;Database=db"
},
"Mining": {
  "Extensions": [ ".cs",".java",".scala", ".vb",".rs",".go",".s",".proto",".coffee", ".sql",".rb",".ruby",".ts", ".js", ".jsx", ".sh", ".tsx", ".py", ".c", ".h", ".cpp", ".il", ".make", ".cmake", ".ps1", ".r", ".cmd"],
  "GitBranch": "master",
  "RepositoryPath": "PATH_TO_REPO",
  "GitHubRepo": "REPO_NAME",
  "GitHubOwner": "REPO_OWNER",
  "GitHubToken": "GITHUB_TOKEN",
  "PeriodLength": 3,
  "PeriodType": "month",
  "MegaDevelopers": [ "dotnetbot","dotnetmaestro","dotnetmaestrobot","bors","dotnet bot","dotnetgitsyncbot","k8scirobot","k8smergerobot","dotnetautomergebot" ],
  "MegaCommitSize": 100,
  "ExtractBlames":true,
  "FilesAtRiksOwnersThreshold": 1,
  "FilesAtRiksOwnershipThreshold": 0.09999,
  "LeaversOfPeriodExtendedAbsence": 1,
  "MegaPullRequestSize": 100,
  "CoreDeveloperThreshold": 5000,
  "CoreDeveloperCalculationType": "ownership-lines",
  "KnowledgeSaveStrategyType": "reviewers-expertise-review",
  "KnowledgeSaveReviewerReplacementType": "one-of-actuals",
  "KnowledgeSaveReviewerFirstPeriod": "1",
  "SelectedReviewersType":"core",
  "LeaversType": "all",
  "BlamePeriods": [],
  "BlamePeriodsRange":[10,20],
  "ExcludedBlamePaths": ["*\\lib\\*"],
  "LgtmTerms":["%lgtm%","%looks good%","%look good%","%seems good%","%seem good%","%sounds good%","%sound good%","%its good%","%its good%","%r+%","%good job%"],
  "MinimumActualReviewersLength":"0",
  "PullRequestReviewerSelectionStrategy" : "0:nothing-nothing,-:replacerandom-1",
  "AddOnlyToUnsafePullrequests" : true,
  "NumberOfPeriodsForCalculatingProbabilityOfStay":4,
  "RecommenderOption": "alpha-1,beta-1,risk-3,hoarder_ratio-1",
  "ChangePast":true
  }
}
```

## Research Questions

### RQ1, Review and Turnover: What is the reduction in files atrisk to turnover when both authors and reviewers are considered knowledgeable?



```PowerShell

# simulations without considering reviewers
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy nothing --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy nothing --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy nothing --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy nothing --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy nothing --conf-path "PATH_TO_CONF_Kubernetes"

# simulations of reality
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy reviewers-actual --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy reviewers-actual --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy reviewers-actual --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy reviewers-actual --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy reviewers-actual --conf-path "PATH_TO_CONF_Kubernetes"
```

### RQ2, Ownership: Does recommending reviewers based on code ownership reduce the number of files at risk to turnover?

```PowerShell

# AuthorshipRec

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy commit --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy commit --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy commit --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy commit --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy commit --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes"

# RevOwnRec

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy review --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy review --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy review --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy review --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy review --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes"
```

### RQ3, cHRev: Does a state-of-the-art recommender reduce the number of files at risk to turnover?

```PowerShell

# cHRev

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes"
```

### RQ4, Learning and Retention: Can we reduce the number of files at risk to turnover by developing learning and retention aware review recommenders?


```PowerShell

# LearnRec

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes"

# RetentionRec

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes"

# TurnoverRec

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist-spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist-spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist-spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist-spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy persist-spreading --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
```

### RQ5, Sofia: Can we combine recommenders to balance Expertise, CoreWorkload, and FaR? 

```PowerShell

# Sofia

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
```
