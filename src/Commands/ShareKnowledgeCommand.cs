using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diacritics.Extensions;
using F23.StringSimilarity;
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
        internal async Task Execute(LossSimulationOption lossSimulationOption)
        {
            _dbContext = new GitRepositoryDbContext(false);

            var lossSimulation = CreateLossSimulation(lossSimulationOption);

            var timeMachine = CreateTimeMachine(lossSimulation.KnowledgeShareStrategyType, lossSimulation.MegaPullRequestSize);

            var knowledgeDistributioneMap = timeMachine.FlyInTime();

            SaveLeaversAndFilesAtRisk(lossSimulation, knowledgeDistributioneMap);
            SavePullRequestReviewes(knowledgeDistributioneMap, lossSimulation);
            SaveKnowledgeSharingStatus(knowledgeDistributioneMap, lossSimulation);

            lossSimulation.EndDateTime = DateTime.Now;
            _dbContext.Entry(lossSimulation).State=EntityState.Modified;

            _logger.LogInformation("{datetime}: trying to save results into database", DateTime.Now);
            _dbContext.SaveChanges();
            _logger.LogInformation("{datetime}: results have been saved", DateTime.Now);
            _dbContext.Dispose();
        }

        private void SaveLeaversAndFilesAtRisk(LossSimulation lossSimulation, KnowledgeDistributionMap knowledgeDistributioneMap)
        {
            var hashsetAbandonedFiles = new HashSet<string>();

            foreach (var period in _periods)
            {
                _logger.LogInformation("{datetime}: computing knowledge loss for period {pid}.", DateTime.Now,period.Id);

                var availableDevelopers = _developers.Where(q=>q.LastCommitPeriodId>=period.Id && q.FirstCommitPeriodId<=period.Id);

                var leavers = GetLeavers(period, availableDevelopers, _developersContributions, lossSimulation);
                _dbContext.AddRange(leavers);

                var abandonedFiles = GetAbandonedFiles(period,leavers,availableDevelopers,knowledgeDistributioneMap,lossSimulation);
                var uniqueAbandonedFiles = abandonedFiles.Where(q=>!hashsetAbandonedFiles.Contains(q.FilePath));
                _dbContext.AddRange(uniqueAbandonedFiles);

                foreach(var abandonedFile in abandonedFiles)
                    hashsetAbandonedFiles.Add(abandonedFile.FilePath);

                _logger.LogInformation("{datetime}: computing knowledge loss for period {pid} is done.", DateTime.Now, period.Id);
            }

            
        }

        private void SaveKnowledgeSharingStatus(KnowledgeDistributionMap knowledgeMap,LossSimulation lossSimulation)
        {
            var developerFileCommitDetails = knowledgeMap
            .CommitBasedKnowledgeMap
            .Values
            .SelectMany(q=>q.Values);

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

            var developerFileReviewDetails = knowledgeMap
            .ReviewBasedKnowledgeMap
            .Values
            .SelectMany(q=>q.Values);

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
                FilesAtRiksOwnershipThreshold= lossSimulationOption.FilesAtRiksOwnershipThreshold,
                FilesAtRiksOwnersThreshold = lossSimulationOption.FilesAtRiksOwnersThreshold,
                LeaversOfPeriodExtendedAbsence = lossSimulationOption.LeaversOfPeriodExtendedAbsence
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

            _commits = _dbContext
            .Commits
            .Where(q => !q.Ignore)
            .ToArray();

            _logger.LogInformation("{datetime}: Commits are loaded.", DateTime.Now);

            _commitBlobBlames = _dbContext
            .CommitBlobBlames
            .Where(q=>!q.Ignore)
            .ToArray();

            _logger.LogInformation("{datetime}: Blames are loaded.", DateTime.Now);

            var latestCommitDate = _commits.Max(q => q.AuthorDateTime);

            _committedChanges = _dbContext.
            CommittedChanges.
            ToArray();

            _pullRequests = _dbContext
            .PullRequests
            .FromSql($@"SELECT * FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate}")
            .ToArray();

            _pullRequestFiles = _dbContext
            .PullRequestFiles
            .FromSql($@"SELECT * From PullRequestFiles Where PullRequestNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .ToArray();

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

            _pullRequestReviewComments = _dbContext
            .PullRequestReviewerComments
            .FromSql($@"SELECT * From PullRequestReviewerComments Where PullRequestNumber in
            (SELECT Number FROM PullRequests 
                    WHERE MergeCommitSha IS NOT NULL and Merged=1 AND
                    MergeCommitSha NOT IN (SELECT Sha FROM Commits WHERE Ignore=1) AND 
                    Number NOT IN(select PullRequestNumber FROM PullRequestFiles GROUP BY PullRequestNumber having count(*)>{megaPullRequestSize})
                    AND MergedAtDateTime<={latestCommitDate})")
            .ToArray();

            _logger.LogInformation("{datetime}: Reviewers are loaded.", DateTime.Now);

            _developers = _dbContext
            .Developers
            .ToArray();

            _developersContributions = _dbContext
            .DeveloperContributions
            .ToArray();

            _canononicalPathMapper = _dbContext.GetCanonicalPaths();

            _GitHubGitUsernameMapper = _dbContext
            .GitHubGitUsers.Where(q => q.GitUsername != null)
            .ToArray();

            _periods = _dbContext
            .Periods
            .ToArray();

            timeMachine.Initiate(
            _commits,
            _commitBlobBlames,
            _developers,
            _developersContributions,
            _committedChanges,
            _pullRequests,
            _pullRequestFiles,
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
            
            var leaversOfPeriod = potentialLeavers.Where(q => q.LastCommitPeriodId == period.Id);
            
            var extendedLeaversOfPeriod = lossSimulation.LeaversOfPeriodExtendedAbsence>0
            ? potentialLeavers.Where(q => q.LastCommitPeriodId > period.Id).ToList()
            :new List<Developer>();

            for(var j=extendedLeaversOfPeriod.Count()-1;j>=0;j--)
            {
                var hasPariticipatedInExtendedPeriods=false;

                for (var i = 1; i <= extendedAbsence && !hasPariticipatedInExtendedPeriods; i++)
                {
                    var extendedPeriodId = period.Id + i;
                    hasPariticipatedInExtendedPeriods = developersContributions.Any(q => q.PeriodId == extendedPeriodId
                        && q.NormalizedName == extendedLeaversOfPeriod[j].NormalizedName && q.TotalCommits > 0);
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

        private IEnumerable<SimulatedAbondonedFile> GetAbandonedFiles(
        Period period,
        IEnumerable<SimulatedLeaver> leavers, 
        IEnumerable<Developer> availableDevelopers, 
        KnowledgeDistributionMap knowledgeMap, 
        LossSimulation lossSimulation)
        {

            var leaversDic = leavers.ToDictionary(q=>q.Developer.NormalizedName);
            var availableDevelopersDic = availableDevelopers.ToDictionary(q=>q.NormalizedName);

            var authorsFileBlames = knowledgeMap.BlameDistribution[period.Id];

            foreach(var filePath in authorsFileBlames.Keys)
            {
                var allFileBlamesOfPeriod = authorsFileBlames[filePath].Values
                    .Where(q=>availableDevelopersDic.ContainsKey(q.NormalizedDeveloperName))
                    .OrderByDescending(q=>q.TotalAuditedLines)
                    .ToArray();

                var remainingFileBlamesOfPeriod = allFileBlamesOfPeriod
                .Where(q=> !leaversDic.ContainsKey(q.NormalizedDeveloperName))
                .ToArray();

                var totalLinesOfAllAuthors = allFileBlamesOfPeriod.Sum(q=>q.TotalAuditedLines);
                var totalLinesOfRemainingAuthors  = remainingFileBlamesOfPeriod.Sum(q=>q.TotalAuditedLines);
                var leftKnowledge = 1 - totalLinesOfRemainingAuthors/(double)totalLinesOfAllAuthors;
                var isFileSavedByReviewe = IsFileSavedByReview(filePath,knowledgeMap.ReviewBasedKnowledgeMap,period);

                if(leftKnowledge>=lossSimulation.FilesAtRiksOwnershipThreshold && !isFileSavedByReviewe )
                {
                    yield return new SimulatedAbondonedFile()
                    {
                        FilePath=filePath,
                        PeriodId=period.Id,
                        TotalLinesInPeriod=totalLinesOfAllAuthors,
                        LossSimulationId=lossSimulation.Id,
                        AbandonedLinesInPeriod = totalLinesOfAllAuthors - totalLinesOfRemainingAuthors,
                        SavedLinesInPeriod = totalLinesOfRemainingAuthors,
                        RiskType = "abandoned"
                    };
                }

                var topOwnedPortion = 0.0;
                for(var i=0;i<remainingFileBlamesOfPeriod.Count() && i<lossSimulation.FilesAtRiksOwnersThreshold;i++)
                {
                    topOwnedPortion+=remainingFileBlamesOfPeriod[i].TotalAuditedLines/(double)totalLinesOfRemainingAuthors;
                }

                if(topOwnedPortion>=lossSimulation.FilesAtRiksOwnershipThreshold && !isFileSavedByReviewe)
                {
                    yield return new SimulatedAbondonedFile()
                    {
                        FilePath=filePath,
                        PeriodId=period.Id,
                        TotalLinesInPeriod=totalLinesOfAllAuthors,
                        LossSimulationId=lossSimulation.Id,
                        AbandonedLinesInPeriod = totalLinesOfAllAuthors - totalLinesOfRemainingAuthors,
                        SavedLinesInPeriod = totalLinesOfRemainingAuthors,
                        RiskType = "hoarded"
                    };
                }
            }
        }

        private bool IsFileSavedByReview(string filePath, 
        Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> reviewBasedKnowledgeMap,
         Period period)
        {

            var reviewers = reviewBasedKnowledgeMap.GetValueOrDefault(filePath);
            
            if(reviewers==null)
                return false;

            var reviewsDetails = reviewers.Values;

            foreach(var reviewDetail in reviewsDetails)
            {
                if(reviewDetail.Periods.Min(q=>q.Id)<=period.Id && reviewDetail.Developer.LastCommitPeriodId>period.Id)
                    return true;
            }

            return false;
        }

    }
}
