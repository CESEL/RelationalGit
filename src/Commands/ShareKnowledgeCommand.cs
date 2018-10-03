using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

            var timeMachine = CreateTimeMachine(lossSimulation.KnowledgeShareStrategyType, lossSimulation.MegaPullRequestSize);

            var knowledgeDistributioneMap = timeMachine.FlyInTime();

            var leavers = GetLeavers(lossSimulation);
            SaveLeaversAndFilesAtRisk(lossSimulation, knowledgeDistributioneMap, leavers);
            SavePullRequestReviewes(knowledgeDistributioneMap, lossSimulation);
            SaveFileTouches(knowledgeDistributioneMap, lossSimulation);
            SaveOwnershipDistribution(knowledgeDistributioneMap,lossSimulation,leavers);

            lossSimulation.EndDateTime = DateTime.Now;
            _dbContext.Entry(lossSimulation).State=EntityState.Modified;

            _logger.LogInformation("{datetime}: trying to save results into database", DateTime.Now);
            _dbContext.SaveChanges();
            _logger.LogInformation("{datetime}: results have been saved", DateTime.Now);
            _dbContext.Dispose();

            return Task.CompletedTask;
        }

        private void SaveOwnershipDistribution(KnowledgeDistributionMap knowledgeDistributioneMap, LossSimulation lossSimulation
            , Dictionary<long, IEnumerable<SimulatedLeaver>> leavers)
        {

            foreach (var period in _periods)
            {
                var distribution = new Dictionary<string, HashSet<string>>();

                // get the final list of files by the end of period
                var blameSnapshot = knowledgeDistributioneMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(period.Id);

                foreach (var filePath in blameSnapshot.FilePaths)
                {
                    var committers = blameSnapshot[filePath];

                    distribution[filePath] = new HashSet<string>();

                    foreach (var committer in committers)
                    {
                        // later we can change this threshold!
                        if(committer.Value.OwnedPercentage>0)
                            distribution[filePath].Add(committer.Value.NormalizedDeveloperName);
                    }

                    var reviewers = knowledgeDistributioneMap.ReviewBasedKnowledgeMap[filePath]?.Where(q=>q.Value.Periods.Any(p=>p.Id<= period.Id));

                    foreach (var reviewer in reviewers)
                    {
                        distribution[filePath].Add(reviewer.Value.Developer.NormalizedName);
                    }
                }

                var availableDevelopersOfPeriod = GetAvailableDevelopersOfPeriod(period).ToArray();
                var leaversOfPeriod = leavers[period.Id].ToArray();

                foreach (var filePath in distribution.Keys)
                {
                    var knowledgeables = distribution[filePath]
                        .Where(q=>  availableDevelopersOfPeriod.Any(a=>a.NormalizedName==q) && leaversOfPeriod.All(l=>l.NormalizedName!=q));

                    _dbContext.Add(new FileKnowledgeable()
                    {
                        CanonicalPath=filePath,
                        PeriodId=period.Id,
                        TotalKnowledgeables= knowledgeables.Count(),
                        Knowledgeables= knowledgeables.Count()>0?knowledgeables.Aggregate((a, b) => a + "," + b):null,
                        LossSimulationId= lossSimulation.Id
                    });
                }
            }
        }

        private void SaveLeaversAndFilesAtRisk(LossSimulation lossSimulation, KnowledgeDistributionMap knowledgeDistributioneMap, 
            Dictionary<long, IEnumerable<SimulatedLeaver>> leavers)
        {

            foreach (var period in _periods)
            {
                _logger.LogInformation("{datetime}: computing knowledge loss for period {pid}.", DateTime.Now, period.Id);

                var availableDevelopers = GetAvailableDevelopersOfPeriod(period);

                _dbContext.AddRange(leavers[period.Id]);

                var abandonedFiles = GetAbandonedFiles(period, leavers[period.Id], availableDevelopers, knowledgeDistributioneMap, lossSimulation);
                _dbContext.AddRange(abandonedFiles);

                _logger.LogInformation("{datetime}: computing knowledge loss for period {pid} is done.", DateTime.Now, period.Id);
            }

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
            return GetLeavers(period, availableDevelopers, _developersContributions, lossSimulation);
        }

        private IEnumerable<Developer> GetAvailableDevelopersOfPeriod(Period period)
        {
            return _developers.Where(q => q.LastParticipationPeriodId >= period.Id && q.FirstParticipationPeriodId <= period.Id);
        }

        private void SaveFileTouches(KnowledgeDistributionMap knowledgeMap,LossSimulation lossSimulation)
        {
            var developerFileCommitDetails = knowledgeMap.CommitBasedKnowledgeMap.Details;

            foreach(var detail in developerFileCommitDetails)
            {
                foreach(var period in detail.Periods)
                {
                    _dbContext.Add(new FileTouch()
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
                    _dbContext.Add(new FileTouch()
                    {
                        NormalizeDeveloperName = detail.Developer.NormalizedName,
                        PeriodId = period.Id,
                        CanonicalPath=detail.FilePath,
                        TouchType="review",
                        LossSimulationId=lossSimulation.Id
                    });
                }
            }
        }

        private void SavePullRequestReviewes(KnowledgeDistributionMap knowledgeMap,LossSimulation lossSimulation)
        {
            foreach(var pullRequestReviewerItem in knowledgeMap.PullRequestReviewers)
            {
                var pullRequestNumber=pullRequestReviewerItem.Key;
                foreach(var reviewer in pullRequestReviewerItem.Value)
                {
                    _dbContext.Add(new RecommendedPullRequestReviewer()
                    {
                        PullRequestNumber=pullRequestNumber,
                        NormalizedReviewerName=reviewer,
                        LossSimulationId=lossSimulation.Id
                    });
                }
            }
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
            };

            _dbContext.Add(lossSimulation);
            _dbContext.SaveChanges();
            return lossSimulation;
        }

        private TimeMachine CreateTimeMachine(string knowledgeShareStrategyType, int megaPullRequestSize)
        {
            _logger.LogInformation("{datetime}: initializing the Time Machine.",DateTime.Now);

            var knowledgeShareStrategy = KnowledgeShareStrategy.Create(knowledgeShareStrategyType);

            var timeMachine = new TimeMachine(knowledgeShareStrategy.RecommendReviewers,_logger);

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
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate}")
            .ToArray();

            _logger.LogInformation("{datetime}: Pull Request are loaded.", DateTime.Now);


            _pullRequestFiles = _dbContext
            .PullRequestFiles
            .FromSql($@"SELECT * From PullRequestFiles Where PullRequestNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .ToArray();

            _logger.LogInformation("{datetime}: Pull Request Files are loaded.", DateTime.Now);

            _pullRequestReviewers = _dbContext
            .PullRequestReviewers
            .FromSql($@"SELECT * From PullRequestReviewers Where PullRequestNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
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
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .ToArray();

            _issueComments = _dbContext
            .IssueComments
            .FromSql($@"SELECT * From IssueComments Where IssueNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .ToArray();

            _logger.LogInformation("{datetime}: Pull Request Reviewer Comments are loaded.", DateTime.Now);

            _developers = _dbContext.Developers.ToArray();

            _logger.LogInformation("{datetime}: Committed Changes are loaded.", DateTime.Now);

            _developersContributions = _dbContext.DeveloperContributions.ToArray();

            _logger.LogInformation("{datetime}: Committed Changes are loaded.", DateTime.Now);

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
            _periods);
            return timeMachine;
        }

        private IEnumerable<SimulatedLeaver> GetLeavers(
            Period period, 
            IEnumerable<Developer> availableDevelopers, 
            DeveloperContribution[] developersContributions,
            LossSimulation lossSimulation)
        {
            var allPotentialLeavers = GetPotentialLeavers(period, availableDevelopers, developersContributions, lossSimulation);
            var filteredLeavers = FilterDevelopersBasedOnLeaverType(period, developersContributions, lossSimulation.LeaversType, allPotentialLeavers);

            return filteredLeavers;

        }
        private static IEnumerable<SimulatedLeaver> FilterDevelopersBasedOnLeaverType(Period period, DeveloperContribution[] developersContributions, string leaversType, IEnumerable<SimulatedLeaver> leavers)
        {
            var isCore = leaversType == LeaversType.Core;
            var isAll = leaversType == LeaversType.All;

            foreach (var leaver in leavers)
            {
                var developerContribution = developersContributions
                    .SingleOrDefault(q => q.NormalizedName == leaver.NormalizedName && q.PeriodId == period.Id
                    && (isAll || q.IsCore == isCore));

                // some of the devs have no contribution,
                // because they authored files with unknown extensions
                // or we have ignored they knowledge because of mega commits or mega PRs
                // or their knowledge is rewritten by someone else 
                if (developerContribution == null)
                    continue;

               yield return leaver;
            }
        }

        private static IEnumerable<SimulatedLeaver> GetPotentialLeavers(Period period, IEnumerable<Developer> potentialLeavers, DeveloperContribution[] developersContributions, LossSimulation lossSimulation)
        {
            var extendedAbsence = lossSimulation.LeaversOfPeriodExtendedAbsence;
            
            var leaversOfPeriod = potentialLeavers.Where(q => q.LastParticipationPeriodId == period.Id);
            
            var extendedLeaversOfPeriod = lossSimulation.LeaversOfPeriodExtendedAbsence>0
            ? potentialLeavers.Where(q => q.LastParticipationPeriodId > period.Id).ToList()
            :new List<Developer>();

            for(var j=extendedLeaversOfPeriod.Count()-1;j>=0;j--)
            {
                var hasPariticipatedInExtendedPeriods=false;

                for (var i = 1; i <= extendedAbsence && !hasPariticipatedInExtendedPeriods; i++)
                {
                    var extendedPeriodId = period.Id + i;
                    hasPariticipatedInExtendedPeriods = developersContributions
                        .Any(q => q.PeriodId == extendedPeriodId && q.NormalizedName == extendedLeaversOfPeriod[j].NormalizedName && (q.TotalCommits > 0 || q.TotalReviews>0 ));
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

        private IEnumerable<SimulatedAbondonedFile> GetAbandonedFiles(Period period,IEnumerable<SimulatedLeaver> leavers, 
        IEnumerable<Developer> availableDevelopers, 
        KnowledgeDistributionMap knowledgeMap, 
        LossSimulation lossSimulation)
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
