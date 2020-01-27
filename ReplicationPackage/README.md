# Replication Package

## Install The Tool

1) You need to install [RelationalGit](https://github.com/CESEL/RelationalGit) with all the required dependencies.

## Prepare Data 

1) You need to [restore](https://www.janbasktraining.com/blog/restore-a-database-backup-from-sql/) the [backup of data](https://drive.google.com/drive/folders/1nc7Hu7kbPpavYrCMmCU5SEBlLlZTo5Fv) into Sql Server. For each studied project there is a separate database. 
2) Copy the contents of the replication package into your system.
3) Modify the conf file of each project and set the correct connection string.

## Run Simulations

1) Run the [simulations.ps1](https://github.com/CESEL/RelationalGit/blob/master/ReplicationPackage/simulations.ps1).

## Research Questions

### RQ1, Review and Turnover: What is the reduction in files atrisk to turnover when both authors and reviewers are considered knowledgeable?

**Generate Data**: The following commands run two simulations on the CoreFX project. First, without considering reviewers. Second, with considering reviews that have happened in reality.

```PowerShell

# Using the NoReviews parameter for recommendation-strategy, we perform a simulation in which no review has been conducted in the project.

# simulations without considering reviewers
dotnet-rgit --cmd simulate-recommender --recommendation-strategy NoReviews --conf-path $corefx_conf

# Using the Reality parameter for recommendation-strategy, we perform a simulation which reflects exactly what has been happened in reallity during code reviews.

# simulations of reality
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path $corefx_conf
```

---

### RQ2, Ownership: Does recommending reviewers based on code ownership reduce the number of files at risk to turnover?

Following commands are samples to show how AuthorshipRec and RevOwnRec affect Expertise, CoreWorkload, and FaR of CoreFX. See the full list of simulations in [simulations.ps1](https://github.com/CESEL/RelationalGit/blob/master/ReplicationPackage/simulations.ps1).

```PowerShell

# AuthorshipRec
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AuthorshipRec --conf-path $corefx_conf


# RevOwnRec
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RecOwnRec  --conf-path $corefx_conf

```

---

### RQ3, cHRev: Does a state-of-the-art recommender reduce the number of files at risk to turnover?

Following commands are samples to show how cHRev affects Expertise, CoreWorkload, and FaR of CoreFX on CoreFX. See the full list of simulations in [simulations.ps1](https://github.com/CESEL/RelationalGit/blob/master/ReplicationPackage/simulations.ps1).

```PowerShell

# cHRev
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --conf-path $corefx_conf
```

---

### RQ4, Learning and Retention: Can we reduce the number of files at risk to turnover by developing learning and retention aware review recommenders?

Following commands are samples to show how LearnRec, RetentionRec, TurnoverRec affect Expertise, CoreWorkload, and FaR of CoreFX. See the full list of simulations in [simulations.ps1](https://github.com/CESEL/RelationalGit/blob/master/ReplicationPackage/simulations.ps1).

```PowerShell

# LearnRec
dotnet-rgit --cmd simulate-recommender --recommendation-strategy LearnRec  --conf-path $corefx_conf

# RetentionRec
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec  --conf-path $corefx_conf

# TurnoverRec
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --conf-path $corefx_conf
```

---

### RQ5, Sofia: Can we combine recommenders to balance Expertise, CoreWorkload, and FaR? 

Following commands are samples to show how Sofia affects Expertise, CoreWorkload, and FaR of CoreFX. See the full list of simulations in [simulations.ps1](https://github.com/CESEL/RelationalGit/blob/master/ReplicationPackage/simulations.ps1).

```PowerShell

# Sofia
dotnet-rgit --cmd simulate-recommender --recommendation-strategy sofia  --conf-path $corefx_conf

```

## Results RQ1

## Results RQ2, RQ3, RQ4, and RQ5

You need to produce the result per project. 1) Open the database of a project that you want to see its results. 2) Query the LossSimulations table. 3) Note the id of the actual simulation and all recommendation simulations. 4) run the following command to dump the result of quartely percentage change of Expertise, CoreWorkload, and Files at Risk.

**Note** 1) Replace _actual_sim_id_ parameter with the id of the actual simulation. 2) replace _rec_sim_idX_ parameters with the id of the recommendation simulations. These ids are separated by a space. in these samples we have three ids for the recommendation simulation. 3) replace _path_to_result_ parameter with the path of a folder you want to store the result.

```PowerShell

dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --recommender-simulation rec_sim_id1 rec_sim_id2 rec_sim_id3 --actual-simulation actual_sim_id  --conf-path "PATH_TO_CONF_CoreFX"
```
