using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EFCore.BulkExtensions;
using RelationalGit.Data.Models;

namespace RelationalGit.Commands
{
    public class ShareKnowledgeCommand
    {
        #region Fields
        private GitRepositoryDbContext _dbContext;
        private Commit[] _commits;
        private CommitBlobBlame[] _commitBlobBlames;
        private CommittedChange[] _committedChanges;
        private PullRequest[] _pullRequests;
        private PullRequestFile[] _pullRequestFiles;
        private PullRequestReviewer[] _pullRequestReviewers;
        private PullRequestReviewerComment[] _pullRequestReviewComments;
        private IssueComment[] _issueComments;
        private Developer[] _developers;
        private DeveloperContribution[] _developersContributions;
        private Dictionary<string, DeveloperContribution> _developersContributionsDic;
        private Dictionary<string, string> _canononicalPathMapper;
        private GitHubGitUser[] _GitHubGitUsernameMapper;
        private Period[] _periods;
        private ILogger _logger;


        public ShareKnowledgeCommand(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        internal Task Execute(LossSimulationOption lossSimulationOption)
        {
            _dbContext = new GitRepositoryDbContext(false);

            var lossSimulation = CreateLossSimulation(lossSimulationOption);

            var timeMachine = CreateTimeMachine(lossSimulation);

            var knowledgeDistributioneMap = timeMachine.FlyInTime();

            var leavers = GetLeavers(lossSimulation);

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                //SaveLeaversAndFilesAtRisk(lossSimulation, knowledgeDistributioneMap, leavers);
                //_logger.LogInformation("{datetime}: Leavers and FilesAtRisk are saved successfully.", DateTime.Now);

                SavePullRequestReviewes(knowledgeDistributioneMap, lossSimulation);
                _logger.LogInformation("{datetime}: RecommendedPullRequestReviewes are saved successfully.", DateTime.Now);

                //SaveFileTouches(knowledgeDistributioneMap, lossSimulation);
                //_logger.LogInformation("{datetime}: FileTouches are saved successfully.", DateTime.Now);

                SaveOwnershipDistribution(knowledgeDistributioneMap, lossSimulation, leavers);
                _logger.LogInformation("{datetime}: Ownership Distribution is saved Successfully.", DateTime.Now);

                SavePullRequestSimulatedRecommendationResults(knowledgeDistributioneMap, lossSimulation);
                _logger.LogInformation("{datetime}: Pull Requests Recommendation Results are saved Successfully.", DateTime.Now);

                transaction.Commit();
                _logger.LogInformation("{datetime}: Transaction is committed.", DateTime.Now);
            }

            _logger.LogInformation("{datetime}: trying to save results into database", DateTime.Now);
            _dbContext.SaveChanges();

            lossSimulation.EndDateTime = DateTime.Now;
            _dbContext.Entry(lossSimulation).State = EntityState.Modified;

            _dbContext.SaveChanges();
            _logger.LogInformation("{datetime}: results have been saved", DateTime.Now);
            _dbContext.Dispose();

            return Task.CompletedTask;
        }

        private void SavePullRequestSimulatedRecommendationResults(KnowledgeDistributionMap knowledgeDistributioneMap, LossSimulation lossSimulation)
        {
            var results = knowledgeDistributioneMap.PullRequestSimulatedRecommendationMap.Values;
            var bulkPullRequestSimulatedRecommendationResults = new List<PullRequestRecommendationResult>();

            foreach (var result in results)
            {
                if (!result.IsSimulated)
                    continue;

                bulkPullRequestSimulatedRecommendationResults.Add(new PullRequestRecommendationResult()
                {
                    ActualReviewers = result.ActualReviewers?.Count() > 0 ? result.ActualReviewers?.Aggregate((a, b) => a + ", " + b) : null,
                    SelectedReviewers = result.SelectedReviewers?.Count() > 0 ? result.SelectedReviewers?.Aggregate((a, b) => a + ", " + b) : null,
                    SortedCandidates = result.SortedCandidates?.Count() > 0 ? result.SortedCandidates?.Aggregate((a, b) => a + ", " + b) : null,
                    ActualReviewersLength = result.ActualReviewers.Length,
                    SelectedReviewersLength = result.SelectedReviewers.Length,
                    SortedCandidatesLength = result.SortedCandidates?.Length,
                    PullRequestNumber=result.PullRequestNumber,
                    MeanReciprocalRank=result.MeanReciprocalRank,
                    TopFiveIsAccurate=result.TopFiveIsAccurate,
                    TopTenIsAccurate=result.TopTenIsAccurate,
                    LossSimulationId=lossSimulation.Id
                });
            }


            _dbContext.BulkInsert(bulkPullRequestSimulatedRecommendationResults, new BulkConfig { BatchSize = 50000 });


        }

        private void SaveOwnershipDistribution(KnowledgeDistributionMap knowledgeDistributioneMap, LossSimulation lossSimulation, Dictionary<long, IEnumerable<SimulatedLeaver>> leavers)
        {

            var bulkFileTouches = new List<FileTouch>();
            var bulkFileKnowledgeable = new List<FileKnowledgeable>();

            foreach (var period in _periods)
            {
                // get the final list of files by the end of period and also their blame information to that point of time.
                var blameSnapshot = knowledgeDistributioneMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(period.Id);

                // when we have not extracted the related blame information.
                if (blameSnapshot == null)
                    continue;

                // getting the list of people who were active in this period and also who have left the project by the end of this period
                var availableDevelopersOfPeriod = GetAvailableDevelopersOfPeriod(period).Select(q=>q.NormalizedName).ToHashSet();
                var leaversOfPeriod = leavers[period.Id].Select(q => q.NormalizedName).ToHashSet();

                foreach (var filePath in blameSnapshot.FilePaths)
                {
                    // we are counting all developers regardless of their owenership >0
                    var committers = blameSnapshot[filePath].Where(q => q.Value.OwnedPercentage > 0).Select(q => q.Value.NormalizedDeveloperName).ToHashSet();

                    var fileReviewDetails = knowledgeDistributioneMap.ReviewBasedKnowledgeMap[filePath]?.Where(q => q.Value.Periods.Any(p => p.Id <= period.Id));

                    // reviewers shouldn't be null. Just for convinience.
                    var reviewers = fileReviewDetails?.Select(q => q.Value.Developer.NormalizedName).ToHashSet()??new HashSet<string>();

                    var availableCommitters = committers.Where(q => availableDevelopersOfPeriod.Contains(q) && !leaversOfPeriod.Contains(q)).ToArray();

                    var availableReviewers = reviewers.Where(q => availableDevelopersOfPeriod.Contains(q) && !leaversOfPeriod.Contains(q)).ToArray();

                    var knowledgeables = availableReviewers.Union(availableCommitters).ToArray();

                    var totalPullRequests = fileReviewDetails?.SelectMany(q => q.Value.PullRequests).Select(q => q.Number).Distinct().Count();

                    bulkFileKnowledgeable.Add(new FileKnowledgeable()
                    {
                        CanonicalPath = filePath,
                        PeriodId = period.Id,
                        TotalAvailableCommitters = availableCommitters.Count(),
                        TotalAvailableReviewers = availableReviewers.Count(),
                        TotalAvailableReviewOnly = availableReviewers.Where(q => !availableCommitters.Contains(q)).Count(),
                        TotalAvailableCommitOnly = availableCommitters.Where(q => !availableReviewers.Contains(q)).Count(),
                        TotalKnowledgeables = availableReviewers.Union(availableCommitters).Count(),
                        Knowledgeables = knowledgeables.Count() > 0 ? knowledgeables.Aggregate((a, b) => a + "," + b) : null,
                        AvailableCommitters = availableCommitters.Count() > 0 ? availableCommitters.Aggregate((a, b) => a + "," + b) : null,
                        AvailableReviewers = availableReviewers.Count() > 0 ? availableReviewers.Aggregate((a, b) => a + "," + b) : null,
                        LossSimulationId = lossSimulation.Id,
                        HasReviewed = reviewers.Count > 0,
                        TotalReviewers = reviewers.Count(),
                        TotalCommitters = committers.Count(),
                        TotalPullRequests = totalPullRequests.GetValueOrDefault(0)
                    });

                    bulkFileTouches.AddRange(availableCommitters.Select(q => new FileTouch()
                    {
                        CanonicalPath=filePath,
                        LossSimulationId=lossSimulation.Id,
                        NormalizeDeveloperName=q,
                        PeriodId=period.Id,
                        TouchType="commit",
                    }));

                    bulkFileTouches.AddRange(availableReviewers.Select(q => new FileTouch()
                    {
                        CanonicalPath = filePath,
                        LossSimulationId = lossSimulation.Id,
                        NormalizeDeveloperName = q,
                        PeriodId = period.Id,
                        TouchType = "review",
                    }));

                }
            }

            _dbContext.BulkInsert(bulkFileTouches, new BulkConfig { BatchSize = 50000, BulkCopyTimeout = int.MaxValue });
            _dbContext.BulkInsert(bulkFileKnowledgeable, new BulkConfig { BatchSize = 50000,BulkCopyTimeout=int.MaxValue });
        }

        private void SaveLeaversAndFilesAtRisk(LossSimulation lossSimulation, KnowledgeDistributionMap knowledgeDistributioneMap, Dictionary<long, IEnumerable<SimulatedLeaver>> leavers)
        {
            var bulkEntities = new List<SimulatedAbondonedFile>();

            foreach (var period in _periods)
            {
                _logger.LogInformation("{datetime}: computing knowledge loss for period {pid}.", DateTime.Now, period.Id);

                var availableDevelopers = GetAvailableDevelopersOfPeriod(period);

                _dbContext.AddRange(leavers[period.Id]);

                var abandonedFiles = GetAbandonedFiles(period, leavers[period.Id], availableDevelopers, knowledgeDistributioneMap, lossSimulation);
                bulkEntities.AddRange(abandonedFiles);

                _logger.LogInformation("{datetime}: computing knowledge loss for period {pid} is done.", DateTime.Now, period.Id);
            }

            _dbContext.BulkInsert(bulkEntities);
        }

        private Dictionary<long,IEnumerable<SimulatedLeaver>> GetLeavers(LossSimulation lossSimulation)
        {
            var result = new Dictionary<long, IEnumerable<SimulatedLeaver>>();

            foreach (var period in _periods)
            {
                var availableDevelopers = GetAvailableDevelopersOfPeriod(period);
                var leavers = GetLeaversOfPeriod(lossSimulation, period, availableDevelopers);
                result[period.Id] = leavers;
            }

            return result;
        }

        private IEnumerable<SimulatedLeaver> GetLeaversOfPeriod(LossSimulation lossSimulation, Period period, IEnumerable<Developer> availableDevelopers)
        {
            return GetLeavers(period, availableDevelopers, _developersContributionsDic, lossSimulation);
        }

        private IEnumerable<Developer> GetAvailableDevelopersOfPeriod(Period period)
        {
            return _developers.Where(q => q.LastParticipationPeriodId >= period.Id && q.FirstParticipationPeriodId <= period.Id);
        }

        private void SaveFileTouches(KnowledgeDistributionMap knowledgeMap,LossSimulation lossSimulation)
        {
            var bulkEntities = new List<FileTouch>();

            var developerFileCommitDetails = knowledgeMap.CommitBasedKnowledgeMap.Details;

            foreach(var detail in developerFileCommitDetails)
            {
                foreach(var period in detail.Periods)
                {
                    bulkEntities.Add(new FileTouch()
                    {
                        NormalizeDeveloperName = detail.Developer.NormalizedName,
                        PeriodId = period.Id,
                        CanonicalPath=detail.FilePath,
                        TouchType="commit",
                        LossSimulationId=lossSimulation.Id
                    });
                }
            }

            var developerFileReviewDetails = knowledgeMap.ReviewBasedKnowledgeMap.Details;

            foreach(var detail in developerFileReviewDetails)
            {
                foreach(var period in detail.Periods.Distinct())
                {
                    bulkEntities.Add(new FileTouch()
                    {
                        NormalizeDeveloperName = detail.Developer.NormalizedName,
                        PeriodId = period.Id,
                        CanonicalPath=detail.FilePath,
                        TouchType="review",
                        LossSimulationId=lossSimulation.Id
                    });
                }
            }

            _dbContext.BulkInsert(bulkEntities, new BulkConfig { BatchSize=50000});
        }

        private void SavePullRequestReviewes(KnowledgeDistributionMap knowledgeMap,LossSimulation lossSimulation)
        {
            var bulkEntities = new List<RecommendedPullRequestReviewer>();

            foreach (var pullRequestReviewerItem in knowledgeMap.PullRequestSimulatedRecommendationMap)
            {
                var pullRequestNumber=pullRequestReviewerItem.Key;
                foreach(var reviewer in pullRequestReviewerItem.Value.RecommendedPullRequestReviewers)
                {
                    reviewer.LossSimulationId = lossSimulation.Id;
                    bulkEntities.Add(reviewer);
                }
            }

            _dbContext.BulkInsert(bulkEntities);
        }

        private LossSimulation CreateLossSimulation(LossSimulationOption lossSimulationOption)
        {
            var lossSimulation = new LossSimulation()
            {
                StartDateTime = DateTime.Now,
                MegaPullRequestSize = lossSimulationOption.MegaPullRequestSize,
                KnowledgeShareStrategyType = lossSimulationOption.KnowledgeShareStrategyType,
                LeaversType = lossSimulationOption.LeaversType,
                FilesAtRiksOwnershipThreshold = lossSimulationOption.FilesAtRiksOwnershipThreshold,
                FilesAtRiksOwnersThreshold = lossSimulationOption.FilesAtRiksOwnersThreshold,
                LeaversOfPeriodExtendedAbsence = lossSimulationOption.LeaversOfPeriodExtendedAbsence,
                KnowledgeSaveReviewerReplacementType = lossSimulationOption.KnowledgeSaveReviewerReplacementType,
                FirstPeriod= lossSimulationOption.KnowledgeSaveReviewerFirstPeriod,
                SelectedReviewersType=lossSimulationOption.SelectedReviewersType
            };

            _dbContext.Add(lossSimulation);
            _dbContext.SaveChanges();
            return lossSimulation;
        }

        private TimeMachine CreateTimeMachine(LossSimulation lossSimulation)
        {
            _developersContributions = _dbContext.DeveloperContributions.ToArray();


            /*var dict = new Dictionary<long, List<double>>();

            for (int i = 1; i < 14; i++)
            {
                var coreDevs = _developersContributions.Where(q => q.PeriodId == i && !(q.TotalCommits > 20 || q.TotalReviews > 5)).Select(q=>q.NormalizedName).ToHashSet();

                for (int j = 1; j < 14 ; j++)
                {
                    if (j + i > 14)
                        continue;

                    var remainedDevsCount = _developersContributions.Where(q => q.PeriodId == i+j && (q.TotalCommits>0 || q.TotalReviews>0) && coreDevs.Contains(q.NormalizedName)).Count();

                    if (!dict.ContainsKey(j))
                        dict[j] = new List<double>();

                    dict[j].Add((double)remainedDevsCount / coreDevs.Count());
                }
            }

            var list = new Dictionary<long,double>();
            foreach (var kv in dict)
            {
                list[kv.Key]= kv.Value.Average();
            }*/

            _logger.LogInformation("{datetime}: initializing the Time Machine.",DateTime.Now);

            var knowledgeShareStrategy = KnowledgeShareStrategy.Create(lossSimulation.KnowledgeShareStrategyType, lossSimulation.KnowledgeSaveReviewerReplacementType);

            var timeMachine = new TimeMachine(knowledgeShareStrategy,_logger);

            _commits = _dbContext.Commits.Where(q => !q.Ignore).ToArray();

            _logger.LogInformation("{datetime}: Commits are loaded.", DateTime.Now);

            _commitBlobBlames = _dbContext.CommitBlobBlames.Where(q=>!q.Ignore).ToArray();

            _logger.LogInformation("{datetime}: Blames are loaded.", DateTime.Now);

            var latestCommitDate = _commits.Max(q => q.AuthorDateTime);

            _committedChanges = _dbContext.CommittedChanges.ToArray();

            _logger.LogInformation("{datetime}: Committed Changes are loaded.", DateTime.Now);

            _pullRequests = _dbContext
            .PullRequests
            .FromSql($@"SELECT * FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{lossSimulation.MegaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate}")
            .ToArray();

            _logger.LogInformation("{datetime}: Pull Request are loaded.", DateTime.Now);


            _pullRequestFiles = _dbContext
            .PullRequestFiles
            .FromSql($@"SELECT * From PullRequestFiles Where PullRequestNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{lossSimulation.MegaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .ToArray();

            _logger.LogInformation("{datetime}: Pull Request Files are loaded.", DateTime.Now);

            _pullRequestReviewers = _dbContext
            .PullRequestReviewers
            .FromSql($@"SELECT * From PullRequestReviewers Where PullRequestNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{lossSimulation.MegaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .Where(q => q.State != "DISMISSED")
            .ToArray();

            _logger.LogInformation("{datetime}: Pull Request Reviewers are loaded.", DateTime.Now);

            _pullRequestReviewComments = _dbContext
            .PullRequestReviewerComments
            .FromSql($@"SELECT * From PullRequestReviewerComments Where PullRequestNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{lossSimulation.MegaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .ToArray();

            _issueComments = _dbContext
            .IssueComments
            .FromSql($@"SELECT * From IssueComments Where IssueNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{lossSimulation.MegaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate}) and ((Body LIKE '%lgtm%') OR
                                                   (Body LIKE '%looks good%') OR
                                                   (Body LIKE '%its good%') OR
                                                   (Body LIKE '%look good%') OR
                                                   (Body LIKE '%good job%'))")
            .ToArray();

            _logger.LogInformation("{datetime}: Pull Request Reviewer Comments are loaded.", DateTime.Now);

            _developers = _dbContext.Developers.ToArray();

            _logger.LogInformation("{datetime}: Developers are loaded.", DateTime.Now);

            


            _developersContributionsDic = _developersContributions.ToDictionary(q => q.PeriodId+ "-" + q.NormalizedName);

            _logger.LogInformation("{datetime}: Developers Contributions are loaded.", DateTime.Now);

            _canononicalPathMapper = _dbContext.GetCanonicalPaths();

            _logger.LogInformation("{datetime}: Canonical Paths are loaded.", DateTime.Now);

            _GitHubGitUsernameMapper = _dbContext.GitHubGitUsers.Where(q => q.GitUsername != null).ToArray();

            _periods = _dbContext.Periods.ToArray();

            timeMachine.Initiate(
            _commits,
            _commitBlobBlames,
            _developers,
            _developersContributions,
            _committedChanges,
            _pullRequests,
            _pullRequestFiles,
            _issueComments,
            _pullRequestReviewers,
            _pullRequestReviewComments,
            _canononicalPathMapper,
            _GitHubGitUsernameMapper,
            _periods,
            lossSimulation.FirstPeriod,lossSimulation.SelectedReviewersType);


            return timeMachine;

        }

        private IEnumerable<SimulatedLeaver> GetLeavers(Period period, IEnumerable<Developer> availableDevelopers, Dictionary<string, DeveloperContribution> developersContributions, LossSimulation lossSimulation)
        {
            var allPotentialLeavers = GetPotentialLeavers(period, availableDevelopers, developersContributions, lossSimulation).ToArray();
            var filteredLeavers = FilterDevelopersBasedOnLeaverType(period, developersContributions, lossSimulation.LeaversType, allPotentialLeavers).ToArray();

            return filteredLeavers;

        }

        private static IEnumerable<SimulatedLeaver> FilterDevelopersBasedOnLeaverType(Period period, Dictionary<string, DeveloperContribution> developersContributions, string leaversType, IEnumerable<SimulatedLeaver> leavers)
        {
            var isCore = leaversType == LeaversType.Core;

            foreach (var leaver in leavers)
            {
                if (leaversType == LeaversType.All)
                    yield return leaver;
                else
                {
                    var developerContribution = developersContributions.GetValueOrDefault(period.Id + "-" + leaver.NormalizedName);

                    // some of the devs have no contribution,
                    // because they authored files with unknown extensions
                    // or we have ignored they knowledge because of mega commits or mega PRs
                    // or their knowledge is rewritten by someone else 
                    if (developerContribution == null)
                        continue;

                    if (developerContribution.IsCore == isCore)
                        yield return leaver;
                }
            }
        }

        private static IEnumerable<SimulatedLeaver> GetPotentialLeavers(Period period, IEnumerable<Developer> potentialLeavers, Dictionary<string, DeveloperContribution> developersContributions, LossSimulation lossSimulation)
        {
            var extendedAbsence = lossSimulation.LeaversOfPeriodExtendedAbsence;
            
            var leaversOfPeriod = potentialLeavers.Where(q => q.LastParticipationPeriodId == period.Id);
            
            var extendedLeaversOfPeriod = extendedAbsence > 0
            ? potentialLeavers.Where(q => q.LastParticipationPeriodId > period.Id).ToList():new List<Developer>();

            // we do not want to put these variablles inside the for loop for performance reasons.
            var extendedPeriodId = 0L;
            DeveloperContribution contribution = null;

            for (var j=extendedLeaversOfPeriod.Count()-1;j>=0;j--)
            {
                var hasPariticipatedInExtendedPeriods=false;

                for (var i = 1; i <= extendedAbsence && !hasPariticipatedInExtendedPeriods; i++)
                {
                    extendedPeriodId = period.Id + i;
                    contribution = developersContributions.GetValueOrDefault(extendedPeriodId + "-" + extendedLeaversOfPeriod[j].NormalizedName);
                    hasPariticipatedInExtendedPeriods = contribution?.TotalCommits + contribution?.TotalReviews > 0;
                }

                if (hasPariticipatedInExtendedPeriods)
                        extendedLeaversOfPeriod.RemoveAt(j);
            }

            var allLeavers = extendedLeaversOfPeriod
            .Select(q=> new SimulatedLeaver()
            {
                PeriodId=period.Id,
                LossSimulationId=lossSimulation.Id,
                Developer=q,
                NormalizedName=q.NormalizedName,
                LeavingType = "extended"
            }).Concat(leaversOfPeriod.Select(q=> new SimulatedLeaver()
            {
                PeriodId=period.Id,
                LossSimulationId=lossSimulation.Id,
                Developer=q,
                NormalizedName=q.NormalizedName,
                LeavingType = "last-commit"
            }));

            return allLeavers;
        }

        private IEnumerable<SimulatedAbondonedFile> GetAbandonedFiles(Period period,IEnumerable<SimulatedLeaver> leavers, IEnumerable<Developer> availableDevelopers, KnowledgeDistributionMap knowledgeMap, LossSimulation lossSimulation)
        {

            var leaversDic = leavers.ToDictionary(q=>q.Developer.NormalizedName);
            var availableDevelopersDic = availableDevelopers.ToDictionary(q=>q.NormalizedName);

            var authorsFileBlames = knowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(period.Id);

            foreach(var filePath in authorsFileBlames.FilePaths)
            {
                var isFileSavedByReview = IsFileSavedByReview(filePath, knowledgeMap.ReviewBasedKnowledgeMap, period);
                if (isFileSavedByReview)
                    continue;

                var fileTotalLines = (double) authorsFileBlames[filePath].Sum(q=>q.Value.TotalAuditedLines);

                var remainingBlames = authorsFileBlames[filePath].Values.Where(q => availableDevelopersDic.ContainsKey(q.NormalizedDeveloperName))
                    .OrderByDescending(q => q.TotalAuditedLines)
                    .ToArray();
                
                var abandonedBlames = remainingBlames.Where(q => leaversDic.ContainsKey(q.NormalizedDeveloperName)).ToArray();

                var remainingTotalLines = remainingBlames.Sum(q => q.TotalAuditedLines);
                var abandonedTotalLines = abandonedBlames.Sum(q => q.TotalAuditedLines);

                var remainingPercentage = remainingTotalLines / fileTotalLines;
                var abandonedPercentage = abandonedTotalLines / fileTotalLines;

                var leftKnowledgePercentage = 1 - (remainingPercentage - abandonedPercentage);

                if(leftKnowledgePercentage>=lossSimulation.FilesAtRiksOwnershipThreshold)
                {
                    yield return new SimulatedAbondonedFile()
                    {
                        FilePath=filePath,
                        PeriodId=period.Id,
                        TotalLinesInPeriod= remainingTotalLines,
                        LossSimulationId=lossSimulation.Id,
                        AbandonedLinesInPeriod = abandonedTotalLines,
                        SavedLinesInPeriod = remainingTotalLines - abandonedTotalLines,
                        RiskType = "abandoned"
                    };
                }

                var nonAbandonedBlames = remainingBlames.Where(q => !leaversDic.ContainsKey(q.NormalizedDeveloperName)).ToArray();
                var totalNonAbandonedLines = (double) nonAbandonedBlames.Sum(q=>q.TotalAuditedLines);

                var topOwnedPortion = 0.0;
                for(var i=0;i< nonAbandonedBlames.Count() && i<lossSimulation.FilesAtRiksOwnersThreshold;i++)
                {
                    topOwnedPortion+= nonAbandonedBlames[i].TotalAuditedLines/totalNonAbandonedLines;
                }

                if(topOwnedPortion>=lossSimulation.FilesAtRiksOwnershipThreshold)
                {
                    yield return new SimulatedAbondonedFile()
                    {
                        FilePath=filePath,
                        PeriodId=period.Id,
                        TotalLinesInPeriod= remainingTotalLines,
                        LossSimulationId=lossSimulation.Id,
                        AbandonedLinesInPeriod = abandonedTotalLines,
                        SavedLinesInPeriod = remainingTotalLines - abandonedTotalLines,
                        RiskType = "hoarded"
                    };
                }
            }
        }

        private bool IsFileSavedByReview(string filePath,ReviewBasedKnowledgeMap reviewBasedKnowledgeMap,Period period)
        {
            var reviewers = reviewBasedKnowledgeMap.GetReviewsOfFile(filePath);
            
            if(reviewers==null)
                return false;

            var reviewsDetails = reviewers.Values;

            foreach(var reviewDetail in reviewsDetails)
            {
                if(reviewDetail.Periods.Min(q=>q.Id)<=period.Id && reviewDetail.Developer.LastParticipationPeriodId>period.Id)
                    return true;
            }

            return false;
        }

    }
}
