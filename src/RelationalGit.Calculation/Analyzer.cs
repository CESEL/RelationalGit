using CsvHelper;
using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Calculation
{
    public class Analyzer
    {
        public void AnalyzeSimulations(long actualSimulationId, long[] recommenderSimulationIds, string analyzeResultPath)
        {
            if (!Directory.Exists(analyzeResultPath))
                Directory.CreateDirectory(analyzeResultPath);

            CalculateFaRReduction(actualSimulationId, recommenderSimulationIds, analyzeResultPath);
            CalculateExpertiseLoss(actualSimulationId, recommenderSimulationIds, analyzeResultPath);
            CalculateWorkload(actualSimulationId, recommenderSimulationIds, 10, analyzeResultPath);
        }

        public void CalculateWorkload(long actualId, long[] simulationsIds, int topReviewers, string path)
        {

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var periods = dbContext.Periods.ToArray();
                var pullRequests = dbContext.PullRequests.ToDictionary(q => q.Number);
                var actualSelectedReviewers = dbContext.RecommendedPullRequestReviewers.Where(q => q.LossSimulationId == actualId).ToArray();

                var actualWorkload = new Dictionary<long, Dictionary<string, int>>();
                foreach (var actualSelectedReviewer in actualSelectedReviewers)
                {
                    var prDateTime = pullRequests[(int)actualSelectedReviewer.PullRequestNumber].CreatedAtDateTime;
                    var period = periods.Single(q => q.FromDateTime <= prDateTime && q.ToDateTime >= prDateTime);

                    if (!actualWorkload.ContainsKey(period.Id))
                        actualWorkload[period.Id] = new Dictionary<string, int>();

                    if (!actualWorkload[period.Id].ContainsKey(actualSelectedReviewer.NormalizedReviewerName))
                        actualWorkload[period.Id][actualSelectedReviewer.NormalizedReviewerName] = 0;

                    actualWorkload[period.Id][actualSelectedReviewer.NormalizedReviewerName]++;
                }

                var lastPeriod = actualWorkload.Max(q => q.Key);
                actualWorkload.Remove(lastPeriod);

                foreach (var simulationId in simulationsIds)
                {
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);
                    
                    var simulatedSelectedReviewers = dbContext.RecommendedPullRequestReviewers.Where(q => q.LossSimulationId == simulationId)
                        .Select(q=>new { q.NormalizedReviewerName, q.PullRequestNumber })
                        .AsNoTracking()
                        .ToArray();

                    var simulatedWorkload = new Dictionary<long, Dictionary<string, int>>();
                    foreach (var simulatedSelectedReviewer in simulatedSelectedReviewers)
                    {
                        var prDateTime = pullRequests[(int)simulatedSelectedReviewer.PullRequestNumber].CreatedAtDateTime;
                        var period = periods.Single(q => q.FromDateTime <= prDateTime && q.ToDateTime >= prDateTime);

                        if (!simulatedWorkload.ContainsKey(period.Id))
                            simulatedWorkload[period.Id] = new Dictionary<string, int>();

                        if (!simulatedWorkload[period.Id].ContainsKey(simulatedSelectedReviewer.NormalizedReviewerName))
                            simulatedWorkload[period.Id][simulatedSelectedReviewer.NormalizedReviewerName] = 0;

                        simulatedWorkload[period.Id][simulatedSelectedReviewer.NormalizedReviewerName]++;
                    }

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedWorkloadPeriod in simulatedWorkload)
                    {
                        var periodId = simulatedWorkloadPeriod.Key;
                        var actualWorkLoadPeriod = actualWorkload.GetValueOrDefault(periodId);

                        if (actualWorkLoadPeriod == null)
                            continue;

                        var actualTop10Workload = actualWorkLoadPeriod.OrderByDescending(q => q.Value).Take(10).Sum(q => q.Value);
                        var simulatedTop10Workload = simulatedWorkloadPeriod.Value.OrderByDescending(q => q.Value).Take(10).Sum(q => q.Value);

                        var value = CalculateIncreasePercentage(simulatedTop10Workload, actualTop10Workload);

                        simulationResult.Results.Add((periodId, value));
                    }

                    result.Add(simulationResult);
                }
            }

            Write(result, Path.Combine(path, "workload.csv"));
        }

        private static void CalculateWorkloadRaw(long[] simulationsIds, int topReviewers, string path)
        {
            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var periods = dbContext.Periods.ToArray();
                var pullRequests = dbContext.PullRequests.ToDictionary(q => q.Number);

                foreach (var simulationId in simulationsIds)
                {
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);
                    var simulatedSelectedReviewers = dbContext.RecommendedPullRequestReviewers.Where(q => q.LossSimulationId == simulationId).ToArray();

                    var simulatedWorkload = new Dictionary<long, Dictionary<string, int>>();
                    foreach (var simulatedSelectedReviewer in simulatedSelectedReviewers)
                    {
                        var prDateTime = pullRequests[(int)simulatedSelectedReviewer.PullRequestNumber].CreatedAtDateTime;
                        var period = periods.Single(q => q.FromDateTime <= prDateTime && q.ToDateTime >= prDateTime);

                        if (!simulatedWorkload.ContainsKey(period.Id))
                            simulatedWorkload[period.Id] = new Dictionary<string, int>();

                        if (!simulatedWorkload[period.Id].ContainsKey(simulatedSelectedReviewer.NormalizedReviewerName))
                            simulatedWorkload[period.Id][simulatedSelectedReviewer.NormalizedReviewerName] = 0;

                        simulatedWorkload[period.Id][simulatedSelectedReviewer.NormalizedReviewerName]++;
                    }

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedWorkloadPeriod in simulatedWorkload)
                    {
                        var periodId = simulatedWorkloadPeriod.Key;
                        var simulatedTop10Workload = simulatedWorkloadPeriod.Value.OrderByDescending(q => q.Value).Take(10).Sum(q => q.Value);

                        var value = simulatedTop10Workload;

                        simulationResult.Results.Add((periodId, value));
                    }

                    simulationResult.Results.AddRange(periods.Where(q => !simulationResult.Results.Any(r => r.PeriodId == q.Id)).Select(q => (q.Id, 0.0)));
                    simulationResult.Results = simulationResult.Results.OrderBy(q => q.PeriodId).ToList();
                    result.Add(simulationResult);
                }
            }

            result = result.OrderBy(q => q.LossSimulation.KnowledgeShareStrategyType).ToList();
            Write(result, Path.Combine(path, "workload_raw.csv"));
        }

        public void CalculateExpertiseLoss(long actualId, long[] simulationsIds, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var periods = dbContext.Periods.ToArray();
                var pullRequests = dbContext.PullRequests.ToDictionary(q => q.Number);

                var actualRecommendationResults = dbContext.PullRequestRecommendationResults.Where(q => q.LossSimulationId == actualId && q.ActualReviewersLength > 0)
                    .Select(q => new { q.Expertise,q.PullRequestNumber})
                    .ToArray();

                var actualExpertise = new Dictionary<long, List<double>>();
                foreach (var actualRecommendationResult in actualRecommendationResults)
                {
                    var prDateTime = pullRequests[(int)actualRecommendationResult.PullRequestNumber].CreatedAtDateTime;
                    var period = periods.Single(q => q.FromDateTime <= prDateTime && q.ToDateTime >= prDateTime);

                    if (!actualExpertise.ContainsKey(period.Id))
                        actualExpertise[period.Id] = new List<double>();

                    actualExpertise[period.Id].Add(actualRecommendationResult.Expertise);
                }


                var lastPeriod = actualExpertise.Max(q => q.Key);
                actualExpertise.Remove(lastPeriod);

                foreach (var simulationId in simulationsIds)
                {
                    var simulatedExpertise = new Dictionary<long, List<double>>();
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);
                    var simulatedRecommendationResults = dbContext.PullRequestRecommendationResults.Where(q => q.LossSimulationId == simulationId && q.ActualReviewersLength > 0)
                        .Select(q => new { q.Expertise,q.PullRequestNumber})
                        .ToArray();

                    foreach (var simulatedRecommendationResult in simulatedRecommendationResults)
                    {
                        var prDateTime = pullRequests[(int)simulatedRecommendationResult.PullRequestNumber].CreatedAtDateTime;
                        var period = periods.Single(q => q.FromDateTime <= prDateTime && q.ToDateTime >= prDateTime);

                        if (!simulatedExpertise.ContainsKey(period.Id))
                            simulatedExpertise[period.Id] = new List<double>();

                        simulatedExpertise[period.Id].Add(simulatedRecommendationResult.Expertise);
                    }

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedExpertisePeriod in simulatedExpertise)
                    {
                        var periodId = simulatedExpertisePeriod.Key;
                        var actualExpertises = actualExpertise.GetValueOrDefault(periodId);

                        if (actualExpertises == null)
                            continue;

                        var value = CalculateIncreasePercentage(simulatedExpertisePeriod.Value.Sum(),
                            actualExpertises.Sum());

                        simulationResult.Results.Add((periodId, value));
                    }

                    result.Add(simulationResult);
                }
            }

            Write(result, Path.Combine(path, "expertiseloss.csv"));
        }

        private static void CalculateHoardings(long actualId, long[] simulationsIds, int topReviewers, string path)
        {
            var result = new List<SimulationResult>();
            var dicActualHoarderPeriod = new Dictionary<long, int>();

            using (var dbContext = GetDbContext())
            {
                var periods = dbContext.Periods.ToArray();
                var pullRequests = dbContext.PullRequests.ToDictionary(q => q.Number);
                var actualSelectedReviewers = dbContext.RecommendedPullRequestReviewers.Where(q => q.LossSimulationId == actualId).ToArray();

                var actualWorkload = new Dictionary<long, Dictionary<string, int>>();
                foreach (var actualSelectedReviewer in actualSelectedReviewers)
                {
                    var prDateTime = pullRequests[(int)actualSelectedReviewer.PullRequestNumber].CreatedAtDateTime;
                    var period = periods.Single(q => q.FromDateTime <= prDateTime && q.ToDateTime >= prDateTime);

                    if (!actualWorkload.ContainsKey(period.Id))
                        actualWorkload[period.Id] = new Dictionary<string, int>();

                    if (!actualWorkload[period.Id].ContainsKey(actualSelectedReviewer.NormalizedReviewerName))
                        actualWorkload[period.Id][actualSelectedReviewer.NormalizedReviewerName] = 0;

                    actualWorkload[period.Id][actualSelectedReviewer.NormalizedReviewerName]++;
                }

                foreach (var actualWorkloadPeriod in actualWorkload)
                {
                    var periodId = actualWorkloadPeriod.Key;
                    var actualWorkLoadPeriod = actualWorkload.GetValueOrDefault(periodId);
                    var files = dbContext.FileKnowledgeables.Where(q => q.HasReviewed && q.LossSimulationId == actualId
                    && q.TotalKnowledgeables == 1 && q.PeriodId == periodId).ToArray();

                    if (actualWorkLoadPeriod == null)
                        continue;

                    var actualTopReviewers = actualWorkLoadPeriod.OrderByDescending(q => q.Value).Take(10).Select(q => q.Key);
                    var actualHoarders = files.Where(q => q.Knowledgeables.Split(',').All(q1 => actualTopReviewers.Contains(q1))).Count();

                    dicActualHoarderPeriod[periodId] = actualHoarders;
                }

                foreach (var simulationId in simulationsIds)
                {
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);
                    var simulatedSelectedReviewers = dbContext.RecommendedPullRequestReviewers.Where(q => q.LossSimulationId == simulationId).ToArray();

                    var simulatedWorkload = new Dictionary<long, Dictionary<string, int>>();
                    foreach (var simulatedSelectedReviewer in simulatedSelectedReviewers)
                    {
                        var prDateTime = pullRequests[(int)simulatedSelectedReviewer.PullRequestNumber].CreatedAtDateTime;
                        var period = periods.Single(q => q.FromDateTime <= prDateTime && q.ToDateTime >= prDateTime);

                        if (!simulatedWorkload.ContainsKey(period.Id))
                            simulatedWorkload[period.Id] = new Dictionary<string, int>();

                        if (!simulatedWorkload[period.Id].ContainsKey(simulatedSelectedReviewer.NormalizedReviewerName))
                            simulatedWorkload[period.Id][simulatedSelectedReviewer.NormalizedReviewerName] = 0;

                        simulatedWorkload[period.Id][simulatedSelectedReviewer.NormalizedReviewerName]++;
                    }

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedWorkloadPeriod in simulatedWorkload)
                    {
                        var periodId = simulatedWorkloadPeriod.Key;
                        var actualWorkLoadPeriod = actualWorkload.GetValueOrDefault(periodId);
                        var files = dbContext.FileKnowledgeables.Where(q => q.HasReviewed && q.LossSimulationId == lossSimulation.Id
                        && q.TotalKnowledgeables == 1 && q.PeriodId == periodId).ToArray();


                        if (actualWorkLoadPeriod == null)
                            continue;

                        var simulatedTopReviewers = simulatedWorkloadPeriod.Value.OrderByDescending(q => q.Value).Take(10).Select(q => q.Key); ;

                        var simulatedHoarders = files.Where(q => q.Knowledgeables.Split(',').All(q1 => simulatedTopReviewers.Contains(q1))).Count();

                        var value = CalculateIncreasePercentage(simulatedHoarders, dicActualHoarderPeriod[periodId]);

                        simulationResult.Results.Add((periodId, value));
                    }

                    result.Add(simulationResult);
                }
            }

            Write(result, Path.Combine(path, "hoardings.csv"));
        }

        private static void CalculateExpertiseRaw(long[] simulationsIds, string path)
        {
            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var pullRequests = dbContext.PullRequests.ToDictionary(q => q.Number);

                foreach (var simulationId in simulationsIds)
                {
                    var simulatedExpertise = new Dictionary<long, List<double>>();
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);

                    var simulatedRecommendationResults = dbContext.PullRequestRecommendationResults.Where(q => q.LossSimulationId == simulationId && q.ActualReviewersLength > 0)
                        .OrderBy(q => q.PullRequestNumber).ToArray();

                    foreach (var simulatedRecommendationResult in simulatedRecommendationResults)
                    {
                        if (!simulatedExpertise.ContainsKey(simulatedRecommendationResult.PullRequestNumber))
                            simulatedExpertise[simulatedRecommendationResult.PullRequestNumber] = new List<double>();

                        simulatedExpertise[simulatedRecommendationResult.PullRequestNumber].Add(simulatedRecommendationResult.Expertise);
                    }

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedExpertisePeriod in simulatedExpertise)
                    {
                        var value = simulatedExpertisePeriod.Value[0];
                        simulationResult.Results.Add((simulatedExpertisePeriod.Key, value));
                    }

                    result.Add(simulationResult);
                }
            }

            result = result.OrderBy(q => q.LossSimulation.KnowledgeShareStrategyType).ToList();
            Write(result, Path.Combine(path, "expertise_raw.csv"));
        }

        public void CalculateFaRReduction(long actualId, long[] simulationsIds, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var actualFaR = dbContext.FileKnowledgeables.Where(q => q.HasReviewed && q.TotalKnowledgeables < 2 && q.LossSimulationId == actualId).
                    GroupBy(q => q.PeriodId).
                    Select(q => new { Count = q.Count(), PeriodId = q.Key })
                    .ToArray();

                var lastPeriod = actualFaR.Max(q => q.PeriodId);
                actualFaR = actualFaR.Where(q => q.PeriodId != lastPeriod)
                    .ToArray();

                foreach (var simulationId in simulationsIds)
                {
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);

                    var simulatedFaR = dbContext.FileKnowledgeables.Where(q => q.HasReviewed && q.TotalKnowledgeables < 2 && q.LossSimulationId == simulationId).
                        GroupBy(q => q.PeriodId).
                        Select(q => new { Count = q.Count(), PeriodId = q.Key })
                        .ToArray();

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedFaRPeriod in simulatedFaR)
                    {
                        var actualValue = actualFaR.SingleOrDefault(q => q.PeriodId == simulatedFaRPeriod.PeriodId);

                        if (actualValue == null)
                            continue;

                        var value = CalculateIncreasePercentage(simulatedFaRPeriod.Count, actualValue.Count);
                        simulationResult.Results.Add((simulatedFaRPeriod.PeriodId, value));
                    }

                    simulationResult.Results.AddRange(actualFaR.Where(q => !simulationResult.Results.Any(r => r.PeriodId == q.PeriodId)).Select(q => (q.PeriodId, 100.0)));

                    result.Add(simulationResult);
                }
            }

            Write(result, Path.Combine(path, "far.csv"));

        }

        public void CalculateFaRReductionBetweenRealityAndNoReviews(long actualId, long noReviewId, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var actualFaR = dbContext.FileKnowledgeables.Where(q => q.TotalKnowledgeables < 2 && q.LossSimulationId == actualId).
                    GroupBy(q => q.PeriodId).
                    Select(q => new { Count = q.Count(), PeriodId = q.Key })
                    .ToArray();

                var actualProportations = dbContext.FileKnowledgeables.Where(q => q.LossSimulationId == actualId)
                    .GroupBy(q => q.PeriodId)
                    .ToArray()
                    .Select(q => new { Count = actualFaR.Single(f => f.PeriodId == q.Key).Count / (double)q.Count(), PeriodId = q.Key })
                    .ToArray();


                var lastPeriod = actualFaR.Max(q => q.PeriodId);
                actualFaR = actualFaR.Where(q => q.PeriodId != lastPeriod).ToArray();

                var realityLossSimulation = dbContext.LossSimulations.Single(q => q.Id == actualId);


                var actualResult = new SimulationResult()
                {
                    LossSimulation = realityLossSimulation
                };

                foreach (var simulatedFaRPeriod in actualProportations)
                {
                    actualResult.Results.Add((simulatedFaRPeriod.PeriodId, simulatedFaRPeriod.Count));
                }

                var noreviewFaR = dbContext.FileKnowledgeables.Where(q => q.TotalKnowledgeables < 2 && q.LossSimulationId == noReviewId).
                    GroupBy(q => q.PeriodId).
                    Select(q => new { Count = q.Count(), PeriodId = q.Key })
                    .ToArray();

                var noReviewProportations = dbContext.FileKnowledgeables.Where(q => q.LossSimulationId == noReviewId)
                    .GroupBy(q => q.PeriodId)
                    .ToArray()
                    .Select(q => new { Count = noreviewFaR.Single(f => f.PeriodId == q.Key).Count / (double)q.Count(), PeriodId = q.Key })
                    .ToArray();

                var noReviewsLossSimulation = dbContext.LossSimulations.Single(q => q.Id == noReviewId);

                var noReviewsResult = new SimulationResult()
                {
                    LossSimulation = noReviewsLossSimulation
                };

                foreach (var simulatedFaRPeriod in noReviewProportations)
                {
                    noReviewsResult.Results.Add((simulatedFaRPeriod.PeriodId, simulatedFaRPeriod.Count));
                }

                Write(new[] { actualResult, noReviewsResult }, Path.Combine(path, "far_between_reality_noreviews.csv"));

            }
        }

        private static void CalculateFaRRaw(long[] simulationsIds, string path)
        {
            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var periods = dbContext.Periods.ToArray();
                foreach (var simulationId in simulationsIds)
                {
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);

                    var simulatedFaR = dbContext.FileKnowledgeables.Where(q => q.HasReviewed && q.TotalKnowledgeables < 2 && q.LossSimulationId == simulationId).
                        GroupBy(q => q.PeriodId).
                        Select(q => new { Count = q.Count(), PeriodId = q.Key })
                        .ToArray();

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedFaRPeriod in simulatedFaR)
                    {
                        simulationResult.Results.Add((simulatedFaRPeriod.PeriodId, simulatedFaRPeriod.Count));
                    }

                    simulationResult.Results.AddRange(periods.Where(q => !simulationResult.Results.Any(r => r.PeriodId == q.Id)).Select(q => (q.Id, 0.0)));
                    simulationResult.Results = simulationResult.Results.OrderBy(q => q.PeriodId).ToList();

                    result.Add(simulationResult);
                }
            }

            result = result.OrderBy(q => q.LossSimulation.KnowledgeShareStrategyType).ToList();
            Write(result, Path.Combine(path, "far_raw.csv"));

        }

        private static void CalculateTotalFaRRaw(long[] simulationsIds, string path)
        {
            var result = new List<SimulationResult>();

            using (var dbContext = GetDbContext())
            {
                var periods = dbContext.Periods.ToArray();
                foreach (var simulationId in simulationsIds)
                {
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == simulationId);

                    var simulatedFaR = dbContext.FileKnowledgeables.Where(q => q.TotalKnowledgeables < 2 && q.LossSimulationId == simulationId).
                        GroupBy(q => q.PeriodId).
                        Select(q => new { Count = q.Count(), PeriodId = q.Key })
                        .ToArray();

                    var simulationResult = new SimulationResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    foreach (var simulatedFaRPeriod in simulatedFaR)
                    {
                        simulationResult.Results.Add((simulatedFaRPeriod.PeriodId, simulatedFaRPeriod.Count));
                    }

                    simulationResult.Results.AddRange(periods.Where(q => !simulationResult.Results.Any(r => r.PeriodId == q.Id)).Select(q => (q.Id, 0.0)));
                    simulationResult.Results = simulationResult.Results.OrderBy(q => q.PeriodId).ToList();

                    result.Add(simulationResult);
                }
            }

            result = result.OrderBy(q => q.LossSimulation.KnowledgeShareStrategyType).ToList();
            Write(result, Path.Combine(path, "far_total_raw.csv"));

        }

        private static void Write(IEnumerable<SimulationResult> simulationResults, string path)
        {
            using (var dt = new DataTable())
            {
                dt.Columns.Add("PeriodId", typeof(string));

                foreach (var simulationResult in simulationResults)
                {
                    dt.Columns.Add(simulationResult.LossSimulation.KnowledgeShareStrategyType + "-" + simulationResult.LossSimulation.Id, typeof(double));
                }

                var rows = simulationResults.ElementAt(0).Results
                    .Select(q => q.PeriodId)
                    .OrderBy(q=>q)
                    .Select(q =>
                {
                    var row = dt.NewRow();
                    row[0] = q;
                    return row;
                }).ToArray();


                for (int j = 0; j < rows.Length - 1; j++)
                {
                    for (int i = 0; i < simulationResults.Count(); i++)
                    {
                        rows[j][i + 1] = simulationResults.ElementAt(i).Results.SingleOrDefault(q => q.PeriodId == int.Parse(rows[j][0].ToString())).Value;
                    }
                    dt.Rows.Add(rows[j]);
                }

                for (int j = dt.Rows.Count - 1; j>=0; j--)
                {
                    var isRowConstant = IsRowConstant(dt.Rows[j]);

                    if (isRowConstant)
                    {
                        dt.Rows.RemoveAt(j);
                    }
                }

                var rowMedian = dt.NewRow();
                rowMedian[0] = "Median";
                var medians = new List<double>();

                for (int columnIndex = 1; columnIndex < dt.Columns.Count; columnIndex++)
                {
                    var values = new List<double>();

                    for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                    {
                        values.Add((double) dt.Rows[rowIndex][columnIndex]);
                    }

                    medians.Add(values.Median());
                    rowMedian[columnIndex] =  values.Median();
                }

                var rowAverage = dt.NewRow();
                rowAverage[0] = "Average";
                rowAverage[1] = medians.Average();
                dt.Rows.Add(rowMedian);
                dt.Rows.Add(rowAverage);

                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer))
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }

        private static bool IsRowConstant(DataRow row)
        {
            if (row.Table.Columns.Count <= 2)
                return false;

            var lastCheckedValue = row[1];

            for (int i = 2; i < row.Table.Columns.Count; i++)
            {
                if(lastCheckedValue.ToString() != row[i].ToString())
                {
                    return false;
                }
            }

            return true;
        }

        private static GitRepositoryDbContext GetDbContext()
        {
            return new GitRepositoryDbContext(autoDetectChangesEnabled: false);
        }

        public class SimulationResult
        {
            public LossSimulation LossSimulation { get; set; }

            public List<(long PeriodId, double Value)> Results { get; set; } = new List<(long PeriodId, double Value)>();

            public double Median => Results.Select(q => q.Value).OrderBy(q => q).Take(Results.Count - 1).Median();

            public double Average => Results.Select(q => q.Value).Average();
        }

        private static double CalculateIncreasePercentage(double first, double second)
        {
            return ((first / second) - 1) * 100;
        }
    }
}
