# Replication Package

## Install the package

1) You need to install [RelationalGit](https://github.com/CESEL/RelationalGit) with all the required dependencies.
2) You need to [restore](https://www.janbasktraining.com/blog/restore-a-database-backup-from-sql/) the [backup of data](https://drive.google.com/drive/folders/1nc7Hu7kbPpavYrCMmCU5SEBlLlZTo5Fv) into Sql Server. For each studied project there is a separate database. 
3) For each project, you need to have a configuration file. It is a json file that has the connection string to the databases and the required parameters for running the simulation. You can name this file whatever you like. In the following, we assume the path to these configuration files are _PATH_TO_CONF_CoreFX_, _PATH_TO_CONF_CoreCLR_, _PATH_TO_CONF_Roslyn_, _PATH_TO_CONF_Rust_, _PATH_TO_CONF_Kubernetes_.

```JSON
{
"ConnectionStrings": {
   "RelationalGit": "Server=IP_of_db_server;User Id=user_name;Password=pass_word;Database=db_name"
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
  "FilesAtRiksOwnershipThreshold": 0.02,
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
  "BlamePeriodsRange":[],
  "ExcludedBlamePaths": [],
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

**Generate Data**: Run the following commands. Make sure the _--conf-path_ has the correct path to the configuration file.

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

**Result**: 

---

### RQ2, Ownership: Does recommending reviewers based on code ownership reduce the number of files at risk to turnover?

**Generate Data**: Run the following commands. Make sure the _--conf-path_ has the correct path to the configuration file. (**Make sure you have already ran the simulations of reality: RQ1**)

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

---

### RQ3, cHRev: Does a state-of-the-art recommender reduce the number of files at risk to turnover?

**Generate Data**: Run the following commands. Make sure the _--conf-path_ has the correct path to the configuration file. (**Make sure you have already ran the simulations of reality: RQ1**)

```PowerShell

# cHRev

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy bird --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes"
```

---

### RQ4, Learning and Retention: Can we reduce the number of files at risk to turnover by developing learning and retention aware review recommenders?

**Generate Data**: Run the following commands. Make sure the _--conf-path_ has the correct path to the configuration file. (**Make sure you have already ran the simulations of reality: RQ1**)

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

---

### RQ5, Sofia: Can we combine recommenders to balance Expertise, CoreWorkload, and FaR? 

**Generate Data**: Run the following commands. Make sure the _--conf-path_ has the correct path to the configuration file. (**Make sure you have already ran the simulations of reality: RQ1**)

```PowerShell

# Sofia

dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreFX" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_CoreCLR" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Roslyn" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Rust" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
dotnet-rgit --cmd compute-loss --simulation-first-period 1 --mega-pr-size 100 --save-strategy sophia --pullRequests-reviewer-selection "0:nothing-nothing,-:replacerandom-1" --conf-path "PATH_TO_CONF_Kubernetes" --recommender-option "alpha-1,beta-1,risk-3,hoarder_ratio-1"
```

## Result (RQ2, RQ3, RQ4, RQ5) 

You need to produce the result per project. 1) Open the database of a project that you want to see its results. 2) Query the LossSimulations table. 3) Note the id of the actual simulation and all recommendation simulations. 4) run the following command to dump the result.

**Note** 1) Replace _actual_sim_id_ parameter with the id of the actual simulation. 2) replace _rec_sim_idX_ parameters with the id of the recommendation simulations. These ids are separated by a space. in these samples we have three ids for the recommendation simulation. 3) replace _path_to_result_ parameter with the path of a folder you want to store the result.

```PowerShell

dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --recommender-simulation rec_sim_id1 rec_sim_id2 rec_sim_id3 --actual-simulation actual_sim_id  --conf-path "PATH_TO_CONF_CoreFX"
dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --recommender-simulation rec_sim_id1 rec_sim_id2 rec_sim_id3 --actual-simulation actual_sim_id --conf-path "PATH_TO_CONF_CoreCLR"
dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --recommender-simulation rec_sim_id1 rec_sim_id2 rec_sim_id3 --actual-simulation actual_sim_id --conf-path "PATH_TO_CONF_Roslyn"
dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --recommender-simulation rec_sim_id1 rec_sim_id2 rec_sim_id3 --actual-simulation actual_sim_id --conf-path "PATH_TO_CONF_Rust"
dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --recommender-simulation rec_sim_id1 rec_sim_id2 rec_sim_id3 --actual-simulation actual_sim_id --conf-path "PATH_TO_CONF_Kubernetes"
```
