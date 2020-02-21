# Replication Package

## Install The Tool

1) [Install](https://github.com/CESEL/RelationalGit/blob/master/install.md) the tool and its dependencies.

## Preparation 

1) Restore the backup of the data into MS Sql Server. For each studied project there is a separate database. 
a) You can use the [DOI](https://doi.org/10.5281/zenodo.3678551)
b) You can select individual files from the [googledrive backup](https://drive.google.com/drive/folders/1nc7Hu7kbPpavYrCMmCU5SEBlLlZTo5Fv)
2) Copy the **configuration files** and **simulation.ps1** which are provided in the [replication package](https://github.com/CESEL/RelationalGit/tree/master/ReplicationPackage).
3) Open and modify each configuration file to set the connection string. You need to provide the server address along with the credentials. Following snippet shows a sample of how connection string should be set.

```json
 {
	"ConnectionStrings": {
	  "RelationalGit": "Server=ip_db_server;User Id=user_name;Password=pass_word;Database=coreclr"
	},
	"Mining":{
 		
  	}
 }
```

4) Open **simulations.ps1** using an editor and make sure the config variables defined at the top of the file are reffering to the correct location of the downloaded config files. 

```powershell
# Each of the following variables contains the absolute path of the corresponding configuation file.
$corefx_conf = "absolute/path/to/corefx_conf.json"
$coreclr_conf = "absolute/path/to/coreclr_conf.json"
$roslyn_conf = "absolute/path/to/roslyn_conf.json"
$rust_conf = "absolute/path/to/rust_conf.json"
$kubernetes_conf = "absolute/path/to/kubernetes_conf.json"
```

## Run Simulations

1) Run the **simulations.ps1** script. Open PowerShell and run the following command in the directory of the file

``` powershell
./simulations.ps1
```

This scripts runs all the defined reviewer recommendation algorithms accross all projects. Each run is called a simulation because for each pull request one of the actual reviewers is randomly selected to be replaced by the top recommended reviewer.

**Note**: Make sure you have set the PowerShell [execution policy](https://superuser.com/questions/106360/how-to-enable-execution-of-powershell-scripts) to **Unrestricted** or **RemoteAssigned**.

## Research Questions

In following sections, we show which simulations are used for which research questions. For each simulation, a sample is provided that illustrates how the simulation can be run using the tool.

### RQ1, Review and Turnover: What is the reduction in files at risk to turnover when both authors and reviewers are considered knowledgeable?


```PowerShell
# committers only
dotnet-rgit --cmd simulate-recommender --recommendation-strategy NoReviews --conf-path <path_to_config_file>
# committers + reviewers = what happended in "Reality"
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path <path_to_config_file>
```

Log into the database and run

```SQL
-- Get the Id of the simulation 
select Id, KnowledgeShareStrategyType, StartDateTime from LossSimulations
```

Using the Id returned from above, compare the knowlege loss with and without considering reviewers knowledgable run the following: 

```PowerShell
dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --no-reviews-simulation <no_reviews_sim_id> --reality-simulation <reality_sim_id>  --conf-path <path_to_config_file>
```

---

### RQ2, Ownership: Does recommending reviewers based on code ownership reduce the number of files at risk to turnover?

```PowerShell
# AuthorshipRec Recommender
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AuthorshipRec --conf-path <path_to_config_file>
# RevOwnRec Recommender
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RecOwnRec  --conf-path <path_to_config_file>
```

---

### RQ3, cHRev: Does a state-of-the-art recommender reduce the number of files at risk to turnover?


```PowerShell
# cHRev Recommender
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --conf-path <path_to_config_file>
```

---

### RQ4, Learning and Retention: Can we reduce the number of files at risk to turnover by developing learning and retention aware review recommenders?

```PowerShell
# LearnRec Recommender
dotnet-rgit --cmd simulate-recommender --recommendation-strategy LearnRec  --conf-path <path_to_config_file>
# RetentionRec Recommender
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec  --conf-path <path_to_config_file>
# TurnoverRec Recommender
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --conf-path <path_to_config_file>
```

---

### RQ5, Sofia: Can we combine recommenders to balance Expertise, CoreWorkload, and FaR? 

```PowerShell
# Sofia Recommender
dotnet-rgit --cmd simulate-recommender --recommendation-strategy sofia  --conf-path <path_to_config_file>
```

# Results

Log into the database and run

```SQL
-- Get the Id of the simulation 
select Id, KnowledgeShareStrategyType, StartDateTime from LossSimulations
```

Using the Id returned above, compare the recommenders with the actual values, ie "reality id"

```PowerShell
dotnet-rgit --cmd analyze-simulations --analyze-result-path "path_to_result" --recommender-simulation <rec_sim_id> --reality-simulation <reality_id>  --conf-path <path_to_config_file>
```


### Interpretation of Results

The tool creates three csv files, **expertise.csv**, **workload.csv** , and **far.csv** respectively. The first column always shows the project's periods (quarters). Each column corresponds to one of the simulations. Each cell shows the percentage change between the actual outcome and the simulated outcome in that period. The last row of a column shows the median of its values.

The following table illustrates how a csv file of a project with 5 periods is formatted, assuming that only cHRev, TurnoverRec, and Sofia got compared with reality.

| Periods       | cHRev         | cHRev         | TurnoverRec   | Sofia         |
| ------------- | ------------- | ------------- | ------------- |-------------- |
| 1  | 9.12  | 20 | 15  | 10  |
| 2  | 45.87  | 30  | 20  | 25  |
| 3  | 25.10  | 40  | 25  | 42  |
| 4  | 32.10  | 50  | 30  | 90  |
| 5  | 10.10  | 60  | 35  | 34.78  |
| Median  | 25.10  | 40  | 25  | 25  |

**Note**: During simulations, for each pull request, one reviewer is randomly selected to be replaced by the top recommended reviewer. Therefore, the results may vary by up to 2.5 percentage points in our 215 simulations runs (see details in thesis and paper).

**Note**: The raw results of cHRev, TurnoverRec, and Sofia can be found [here](https://drive.google.com/drive/folders/1nc7Hu7kbPpavYrCMmCU5SEBlLlZTo5Fv). (minimum 215 simulation)

### Sofia

Our Sofia tool can be run on any GitHub repo simply by adding our [Sofia plugin](https://github.com/CESEL/Sofia). 
