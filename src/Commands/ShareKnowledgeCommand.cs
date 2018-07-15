
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diacritics.Extensions;
using F23.StringSimilarity;

namespace RelationalGit.Commands
{
    public class ShareKnowledgeCommand
    {
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

        internal async Task Execute(string knowledgeShareStrategyType, double abondonedThreshold,int megaPullRequestSize, string leaversType)
        {
            _dbContext = new GitRepositoryDbContext(false);

            LossSimulation lossSimulation = CreateLossSimulation(knowledgeShareStrategyType, abondonedThreshold, megaPullRequestSize, leaversType);

            TimeMachine timeMachine = CreateTimeMachine(knowledgeShareStrategyType, megaPullRequestSize);

            var knowledgeMap = timeMachine.FlyInTime();

            foreach (var period in _periods)
            {
                CommitBlobBlame[] committedBlobBlames = GetBlames(period);

                var leavers = GetLeavers(period, _developers, _developersContributions, leaversType);
                SaveLeavers(period, lossSimulation, leavers);

                var abondonedFiles = GetAbandonedFiles(period, leavers, _developers, knowledgeMap, committedBlobBlames, abondonedThreshold);
                SaveFiles(period, lossSimulation, abondonedFiles);
            }

            SavePullRequestReviewes(knowledgeMap,lossSimulation);
            SaveKnowledgeSharingStatus(knowledgeMap,lossSimulation);

            lossSimulation.EndDateTime = DateTime.Now;

            _dbContext.SaveChanges();
            _dbContext.Dispose();
        }

        private void SaveKnowledgeSharingStatus(KnowledgeMap knowledgeMap,LossSimulation lossSimulation)
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

        private void SavePullRequestReviewes(KnowledgeMap knowledgeMap,LossSimulation lossSimulation)
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

        private CommitBlobBlame[] GetBlames(Period period)
        {
            return _commitBlobBlames
            .Where(q => q.CommitSha == period.LastCommitSha && !q.Ignore)
            .ToArray();
        }

        private LossSimulation CreateLossSimulation(string knowledgeSaveStrategyType, double abondonedThreshold, int megaPullRequestSize, string leaversType)
        {
            var lossSimulation = new LossSimulation()
            {
                FileAbondonedThreshold = abondonedThreshold,
                MegaPullRequestSize = megaPullRequestSize,
                KnowledgeShareStrategyType = knowledgeSaveStrategyType,
                StartDateTime = DateTime.Now,
                LeaversType = leaversType
            };

            _dbContext.Add(lossSimulation);
            _dbContext.SaveChanges();
            return lossSimulation;
        }

        private TimeMachine CreateTimeMachine(string knowledgeShareStrategyType, int megaPullRequestSize)
        {
            var knowledgeShareStrategy = RecommendingReviewersKnowledgeShareStrategy.Create(knowledgeShareStrategyType);

            var timeMachine = new TimeMachine(knowledgeShareStrategy.RecommendReviewers);

            _commits = _dbContext
            .Commits
            .Where(q => !q.Ignore)
            .ToArray();

            _commitBlobBlames = _dbContext
            .CommitBlobBlames
            .Where(q=>!q.Ignore)
            .ToArray();

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

        private void SaveLeavers(Period period, LossSimulation lossSimulation, Dictionary<string, Developer> leavers)
        {
            foreach(var leaver in leavers.Keys)
            {
                _dbContext.Add(new SimulatedLeaver()
                {
                    NormalizedName=leaver,
                    LossSimulationId=lossSimulation.Id,
                    PeriodId=period.Id
                });
            }        
        }

        private void SaveFiles(Period period, LossSimulation lossSimulation, AbandonedFile[] abandonedFiles)
        {
            foreach(var abandonedFile in abandonedFiles)
            {
                _dbContext.Add(new SimulatedAbondonedFile()
                {
                    FilePath=abandonedFile.FilePath,
                    LossSimulationId=lossSimulation.Id,
                    PeriodId=period.Id,
                    TotalLinesInPeriod = abandonedFile.TotalLinesInPeriod,
                    AbandonedLinesInPeriod = abandonedFile.AbandonedLinesInPeriod,
                    SavedLinesInPeriod = abandonedFile.SavedLines,
                });
            }
        }

        private AbandonedFile[] GetAbandonedFiles(Period period,
            Dictionary<string, Developer> leavers,
            Developer[] developers, 
            KnowledgeMap knowledgeMap, 
            CommitBlobBlame[] committedBlobBlames,
            double abondonedThreshold)
        {

            var leaversKnowledgeOfFiles = GetLeaversKnowledgeOfPeriod (period,leavers,committedBlobBlames);

            var currentKnowledgeOfFiles = GetRemainingKnowledgeOfPeriod(period,developers,committedBlobBlames);

            var abondonedFiles = GetAbandonedFiles(period,leaversKnowledgeOfFiles,currentKnowledgeOfFiles,knowledgeMap.ReviewBasedKnowledgeMap,abondonedThreshold);
        
            return abondonedFiles.ToArray();
        }

        private Dictionary<string,Developer> GetLeavers(
            Period period, 
            Developer[] developers, 
            DeveloperContribution[] developersContributions,
            string leaversType)
        {
            var leavers = new Dictionary<string,Developer>();

            var allLeavers = developers.Where(q=>q.LastPeriodId==period.Id).ToArray();
            var isCore=leaversType==LeaversType.Core;
            var isAll = leaversType==LeaversType.All;   

            foreach(var leaver in allLeavers)
            {
                var developerContribution = developersContributions
                    .SingleOrDefault(q=>q.NormalizedName==leaver.NormalizedName && q.PeriodId==period.Id
                    && (isAll || q.IsCore==isCore));

                // some of the devs have no contribution,
                // because they authored files with unknown extensions
                // or we have ignored they knowledge because of mega commits or mega PRs
                // or their knowledge is rewritten by someone else 
                if(developerContribution==null)
                    continue;

                leavers[leaver.NormalizedName]=leaver;
            }

            return leavers;
        }

        private IEnumerable<AbandonedFile> GetAbandonedFiles(
        Period period,
        Dictionary<string, int> leaversKnowledgeOfFiles, 
        Dictionary<string, int> totalKnowledgeOfFiles, 
        Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> reviewMap, 
        double abondonedThreshold)
        {
            foreach(var file in totalKnowledgeOfFiles.Keys)
            {
                var totalLines = totalKnowledgeOfFiles[file];
                var abandonedLines = leaversKnowledgeOfFiles.GetValueOrDefault(file,defaultValue: 0);
                var savedLines = totalLines - abandonedLines;
                var savedPercentage = savedLines/(double)totalLines;

                if(savedPercentage<abondonedThreshold && !IsFileSavedByReview(file,reviewMap,period))
                {
                    yield return new AbandonedFile()
                    {
                        FilePath=file,
                        TotalLinesInPeriod=totalLines,
                        AbandonedLinesInPeriod=abandonedLines
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
                if(reviewDetail.Periods.Min(q=>q.Id)<=period.Id && reviewDetail.Developer.LastPeriodId>period.Id)
                    return true;
            }

            return false;
        }

        private Dictionary<string,int> GetRemainingKnowledgeOfPeriod(Period period,Developer[] developers, CommitBlobBlame[] committedBlobBlames)
        {
             var fileKnowledgeMap = new Dictionary<string,int>();

            var devs = developers.Where(q=>q.LastPeriodId>=period.Id).ToDictionary(q=>q.NormalizedName);

            foreach(var committedBlobBlame in committedBlobBlames)
            {
                if(!devs.ContainsKey(committedBlobBlame.NormalizedDeveloperIdentity))
                    continue; // we have missed his knowledge already in previous periods.
                
                if(!fileKnowledgeMap.ContainsKey(committedBlobBlame.CanonicalPath))
                {
                    fileKnowledgeMap[committedBlobBlame.CanonicalPath]=0;
                }

                fileKnowledgeMap[committedBlobBlame.CanonicalPath]+=committedBlobBlame.AuditedLines;
            }

            return fileKnowledgeMap;
        }

        private Dictionary<string,int> GetLeaversKnowledgeOfPeriod(Period period, Dictionary<string, Developer> leavers, CommitBlobBlame[] committedBlobBlames)
        {
            var leaversFileKnowledgeMap = new Dictionary<string,int>();

            foreach(var committedBlobBlame in committedBlobBlames)
            {
                if(!leavers.ContainsKey(committedBlobBlame.NormalizedDeveloperIdentity))
                    continue;
                
                if(!leaversFileKnowledgeMap.ContainsKey(committedBlobBlame.CanonicalPath))
                {
                    leaversFileKnowledgeMap[committedBlobBlame.CanonicalPath]=0;
                }

                leaversFileKnowledgeMap[committedBlobBlame.CanonicalPath]+=committedBlobBlame.AuditedLines;
            }

            return leaversFileKnowledgeMap;
        }
    }

    public class AbandonedFile
    {
        public string FilePath { get; set; }         
        public int TotalLinesInPeriod { get; set; }
        public int AbandonedLinesInPeriod { get; set; }
        public int SavedLines =>TotalLinesInPeriod-AbandonedLinesInPeriod;
    }
}
