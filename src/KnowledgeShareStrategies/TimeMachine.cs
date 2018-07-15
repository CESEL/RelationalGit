
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace RelationalGit
{
    public class TimeMachine
    {
        #region Fields
        private Commit[] Commits { get; set; }
        public Dictionary<long, Dictionary<string,Dictionary<string,FileBlame>>> CommitBlobBlamesDic { get; private set; }
        private Dictionary<string, Commit> CommitsDic { get; set; }
        private Dictionary<string, Developer> Developers { get; set; }
        private DeveloperContribution[] DevelopersContributions { get; set; }
        private Dictionary<string, List<CommittedChange>> CommittedChangesDic { get; set; }
        private Dictionary<int,PullRequest> PullRequests { get; set; }
        private Dictionary<long, List<PullRequestFile>> PullRequestFilesDic { get; set; }
        private Dictionary<long, List<string>> PullRequestReviewersDic { get; set; }
        private PullRequestReviewerComment[] PullRequestReviewComments { get; set; }
        private Dictionary<string, string> CanononicalPathMapper { get; set; }
        private GitHubGitUser[] GitHubGitUsernameMapper { get; set; }
        private Dictionary<long, Period> PeriodsDic { get; set; }
        private IEnumerable<CommitPullRequest> SortedCommitsPullRequests { get; set; }
        private Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>> CommitBasedKnowledgeMap = new Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>>();
        private Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> ReviewBasedKnowledgeMap = new Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>>();
        private Func<PullRequestContext,string[]> ChangeThePastByRecommendingReviewersFunc;

        #endregion
        
        public TimeMachine(Func<PullRequestContext,string[]> changeThePastByRecommendingReviewersFunc=null)
        {
            ChangeThePastByRecommendingReviewersFunc=changeThePastByRecommendingReviewersFunc;
        }
        internal void Initiate(Commit[] commits,CommitBlobBlame[] commitBlobBlames, Developer[] developers, DeveloperContribution[] developersContributions,
        CommittedChange[] committedChanges, PullRequest[] pullRequests, PullRequestFile[] pullRequestFiles,
         PullRequestReviewer[] pullRequestReviewers, PullRequestReviewerComment[] pullRequestReviewComments,
         Dictionary<string, string> canononicalPathMapper, GitHubGitUser[] gitHubGitUsernameMapper, Period[] periods)
        {
            Commits = commits.OrderBy(q=>q.AuthorDateTime).ToArray();
            CommitsDic = commits.ToDictionary(q => q.Sha);
            PeriodsDic = periods.ToDictionary(q => q.Id);
            CommitBlobBlamesDic =GetCommitBlobBlamesDictionary(commitBlobBlames);
            Developers = developers.ToDictionary(q => q.NormalizedName);
            DevelopersContributions = developersContributions;
            CommittedChangesDic = GetCommittedChangesDictionary(committedChanges);
            PullRequests = pullRequests.ToDictionary(q=>q.Number);
            PullRequestFilesDic = GetPullRequestFilesDictionary(pullRequestFiles);
            PullRequestReviewersDic = GetPullRequestReviewersDictionary(pullRequestReviewers, pullRequestReviewComments, gitHubGitUsernameMapper);
            PullRequestReviewComments = pullRequestReviewComments;
            CanononicalPathMapper = canononicalPathMapper;
            GitHubGitUsernameMapper = gitHubGitUsernameMapper;
            SortedCommitsPullRequests=GetCommitsPullRequests();
        }

        private Dictionary<long,Dictionary<string,Dictionary<string,FileBlame>>>  GetCommitBlobBlamesDictionary(CommitBlobBlame[] commitBlobBlames)
        {
            var commitBlobBlamesDic=new Dictionary<long,Dictionary<string,Dictionary<string,FileBlame>>>();

            foreach(var commitBlobBlame in commitBlobBlames)
            {
                var commit=CommitsDic.GetValueOrDefault(commitBlobBlame.CommitSha);
                var periodId=commit.PeriodId.Value;
                var period=PeriodsDic[periodId];
                var filePath= commitBlobBlame.CanonicalPath;
                var devName = commitBlobBlame.NormalizedDeveloperIdentity;

                if(!commitBlobBlamesDic.ContainsKey(periodId))
                {
                    commitBlobBlamesDic[periodId] = new Dictionary<string,Dictionary<string,FileBlame>>();
                }

                if(!commitBlobBlamesDic[periodId].ContainsKey(filePath))
                {
                    commitBlobBlamesDic[periodId][filePath] = new Dictionary<string,FileBlame>();
                }    

                if(!commitBlobBlamesDic[periodId][filePath].ContainsKey(devName))
                {
                    commitBlobBlamesDic[periodId][filePath][devName] = new FileBlame()
                    {
                        FileName=filePath,
                        Period=period,
                        NormalizedDeveloperName=devName
                    };
                }

                commitBlobBlamesDic[periodId][filePath][devName].TotalAuditedLines+=commitBlobBlame.AuditedLines;
            }

            return commitBlobBlamesDic;

        }

        private IEnumerable<CommitPullRequest> GetCommitsPullRequests()
        {
            foreach(var commit in Commits)
            {
                // It's possible that two pull requests have the same merge commit
                // it's a bug in github. We take only one of them in such a scenario.
                // https://github.com/dotnet/coreclr/pull/9909
                // https://github.com/dotnet/coreclr/pull/9379
                var mergedPullRequest=PullRequests
                .Values
                .FirstOrDefault(q=>q.MergeCommitSha==commit.Sha);

                yield return new CommitPullRequest(){
                
                    Commit=commit,
                    MergedPullRequest=mergedPullRequest
                };
            }

        }

        private Dictionary<long, List<PullRequestFile>> GetPullRequestFilesDictionary(PullRequestFile[] pullRequestFiles)
        {
            var result = new Dictionary<long, List<PullRequestFile>>();
            var key = 0L;

            for (var i = 0; i < pullRequestFiles.Length; i++)
            {
                key = pullRequestFiles[i].PullRequestNumber;
                if (!result.ContainsKey(key))
                    result[key] = new List<PullRequestFile>();

                result[key].Add(pullRequestFiles[i]);
            }

            return result;
        }

        private Dictionary<string, List<CommittedChange>> GetCommittedChangesDictionary(CommittedChange[] committedChanges)
        {
            var result = new Dictionary<string, List<CommittedChange>>();
            string key = "";

            for (var i = 0; i < committedChanges.Length; i++)
            {
                key = committedChanges[i].CommitSha;
                if (!result.ContainsKey(key))
                    result[key] = new List<CommittedChange>();

                result[key].Add(committedChanges[i]);
            }

            return result;
        }

        private Dictionary<long, List<string>> GetPullRequestReviewersDictionary(
            PullRequestReviewer[] pullRequestReviewers,
            PullRequestReviewerComment[] pullRequestReviewComments,
            GitHubGitUser[] gitHubGitUsernameMapper)
        {
            var result = new Dictionary<long, List<string>>();

            for (var i = 0; i < pullRequestReviewers.Length; i++)
            {
                var key = (int) pullRequestReviewers[i].PullRequestNumber; // what a bullshit cast!! :(
                var prSubmitter = PullRequests[key].UserLogin;

                if(prSubmitter==pullRequestReviewers[i].UserLogin)
                    continue;

                if (!result.ContainsKey(key))
                    result[key] = new List<string>();

                var normalizedName = gitHubGitUsernameMapper
                    .FirstOrDefault(q => q.GitHubUsername == pullRequestReviewers[i].UserLogin);

                if (normalizedName != null)
                    result[key].Add(normalizedName.GitNormalizedUsername);
            }

            for (var i = 0; i < pullRequestReviewComments.Length; i++)
            {
                var key = pullRequestReviewComments[i].PullRequestNumber;
                var prMergedDateTime = PullRequests[key].MergedAtDateTime;
                var prSubmitter = PullRequests[key].UserLogin;

                if(prMergedDateTime< pullRequestReviewComments[i].CreatedAtDateTime)
                    continue;

                if(prSubmitter==pullRequestReviewComments[i].UserLogin)
                    continue;

                if (!result.ContainsKey(key))
                    result[key] = new List<string>();

                var normalizedName = gitHubGitUsernameMapper
                    .FirstOrDefault(q => q.GitHubUsername == pullRequestReviewComments[i].UserLogin);

                

                if (normalizedName != null && !result[key].Any(q => q == normalizedName.GitNormalizedUsername))
                {
                    result[key].Add(normalizedName.GitNormalizedUsername);
                }
            }

            return result;
        }

        internal KnowledgeMap FlyInTime()
        {

            var knowledgeMap = new KnowledgeMap()
            {
                CommitBasedKnowledgeMap = this.CommitBasedKnowledgeMap,
                ReviewBasedKnowledgeMap = this.ReviewBasedKnowledgeMap,
                PullRequestReviewers=PullRequestReviewersDic
            };

            foreach (var commitPullRequest in SortedCommitsPullRequests)
            {
                var commit=commitPullRequest.Commit;
                var pullRequest=commitPullRequest.MergedPullRequest;

                UpdateCommitBasedKnowledgeMap(commit);

                if(pullRequest!=null)
                {
                    AddProposedChangesToPrSubmitterKnowledge(pullRequest, commit);

                    ChangeThePastByRecommendingReviewers(knowledgeMap, pullRequest);

                    UpdateReviewBasedKnowledgeMap(pullRequest);
                }
            }



            return knowledgeMap;
        }

        private void ChangeThePastByRecommendingReviewers(KnowledgeMap knowledgeMap, PullRequest pullRequest)
        {
            if (ChangeThePastByRecommendingReviewersFunc != null)
            {
                var pullRequestContext = GetPullRequestContext(pullRequest, knowledgeMap);
                var recommendedReviewers = ChangeThePastByRecommendingReviewersFunc(pullRequestContext);
                PullRequestReviewersDic[pullRequest.Number] = recommendedReviewers.ToList();
            }
        }
        private PullRequestContext GetPullRequestContext(PullRequest pullRequest, KnowledgeMap knowledgeMap)
        {
            var actualReviewers = PullRequestReviewersDic
            .GetValueOrDefault(pullRequest.Number, defaultValue: new List<string>())
            .ToArray();

            var pullRequestFiles = this.PullRequestFilesDic
            .GetValueOrDefault(pullRequest.Number,new List<PullRequestFile>())
            .ToArray();

            var availableDevelopers = GetAvailableDevelopersForPullRequest(pullRequest)
            .ToArray();

            var prSubmitterNormalizedName = GitHubGitUsernameMapper
            .FirstOrDefault(q=>q.GitHubUsername==pullRequest.UserLogin)
            ?.GitNormalizedUsername;

            var period = GetPeriodOfPullRequest(pullRequest); 

            return new PullRequestContext()
            {
                PRSubmitterNormalizedName=prSubmitterNormalizedName,
                ActualReviewers = actualReviewers,
                PullRequestFiles = pullRequestFiles,
                availableDevelopers = availableDevelopers,
                PullRequest = pullRequest,
                KnowledgeMap = knowledgeMap,
                CanononicalPathMapper=CanononicalPathMapper,
                Period=period,
                Developers=new ReadOnlyDictionary<string,Developer>(Developers),
                Blames= CommitBlobBlamesDic[period.Id]
            };

        }
        private IEnumerable<Developer> GetAvailableDevelopersForPullRequest(PullRequest pullRequest)
        {
            Period period = GetPeriodOfPullRequest(pullRequest);

            foreach (var developer in Developers.Values)
            {
                if (developer.FirstPeriodId >= period.Id && developer.LastPeriodId <= period.Id)
                    yield return developer;
            }
        }

        private Period GetPeriodOfPullRequest(PullRequest pullRequest)
        {
            var mergeCommitd = CommitsDic[pullRequest.MergeCommitSha];
            var period = PeriodsDic[mergeCommitd.PeriodId.Value];
            return period;
        }

        private void UpdateReviewBasedKnowledgeMap(PullRequest pullRequest)
        {
            var reviewersNamesOfPullRequest = PullRequestReviewersDic[pullRequest.Number];

            // some of the pull requests have no modified files strangely
            // for example, https://github.com/dotnet/coreclr/pull/13534
            var filesOfPullRequest = PullRequestFilesDic
                .GetValueOrDefault(pullRequest.Number, new List<PullRequestFile>(0));

            foreach (var file in filesOfPullRequest)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(file.FileName);

                if (canonicalPath == null)
                    continue;

                if (!ReviewBasedKnowledgeMap.ContainsKey(canonicalPath))
                    ReviewBasedKnowledgeMap[canonicalPath] = new Dictionary<string, DeveloperFileReveiewDetail>();

                var period = GetPeriodOfPullRequest(pullRequest);

                foreach (var reviewerName in reviewersNamesOfPullRequest)
                {
                    if (!ReviewBasedKnowledgeMap[canonicalPath].ContainsKey(reviewerName))
                    {
                        ReviewBasedKnowledgeMap[canonicalPath][reviewerName] =
                        new DeveloperFileReveiewDetail()
                        {
                            FilePath = canonicalPath,
                            Developer = Developers[reviewerName]
                        };
                    }

                    if(!ReviewBasedKnowledgeMap[canonicalPath][reviewerName].Periods.Any(q=>q.Id==period.Id))
                        ReviewBasedKnowledgeMap[canonicalPath][reviewerName].Periods.Add(period);

                    ReviewBasedKnowledgeMap[canonicalPath][reviewerName].PullRequests.Add(pullRequest);
                }
            }
        }

        private void AddProposedChangesToPrSubmitterKnowledge(PullRequest pullRequest, Commit prMergedCommit)
        {
            // we assume the PR submitter is the dev who has modified the files
            // however it's not the case always. for example https://github.com/dotnet/coreclr/pull/1

            if (prMergedCommit == null || pullRequest == null)
                return;

            if (pullRequest.MergeCommitSha != prMergedCommit.Sha)
                return;

            // some of the pull requests has no modified file
            // https://github.com/dotnet/coreclr/pull/13534
            var pullRequestFiles = PullRequestFilesDic.GetValueOrDefault(pullRequest.Number);

            if(pullRequestFiles==null)
                return;

            var devName = GitHubGitUsernameMapper
            .FirstOrDefault(q => q.GitHubUsername == pullRequest.UserLogin)
            ?.GitNormalizedUsername;

            if (devName == null)
                return; 
        
            var period = PeriodsDic[prMergedCommit.PeriodId.Value];

            foreach (var file in pullRequestFiles)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(file.FileName);

                if (canonicalPath == null)
                    continue;

                // we assume all the file changes are committed by the main pull request's author
                if (!CommitBasedKnowledgeMap.ContainsKey(canonicalPath))
                    CommitBasedKnowledgeMap[canonicalPath] = new Dictionary<string, DeveloperFileCommitDetail>();

                if (!CommitBasedKnowledgeMap[canonicalPath].ContainsKey(devName))
                {
                    CommitBasedKnowledgeMap[canonicalPath][devName] = new DeveloperFileCommitDetail()
                    {
                        FilePath = canonicalPath,
                        Developer = Developers[devName],
                    };
                }

                if(!CommitBasedKnowledgeMap[canonicalPath][devName].Commits.Any(q=>q.Sha==prMergedCommit.Sha))
                    CommitBasedKnowledgeMap[canonicalPath][devName].Commits.Add(prMergedCommit);

                if(!CommitBasedKnowledgeMap[canonicalPath][devName].Periods.Any(q=>q.Id==period.Id))
                    CommitBasedKnowledgeMap[canonicalPath][devName].Periods.Add(period);
            }
        }

        private void UpdateCommitBasedKnowledgeMap(Commit currentCommit)
        {
            // merged commits probably contain no changes
            var changes = CommittedChangesDic.GetValueOrDefault(currentCommit.Sha, new List<CommittedChange>(0));
            var devName = currentCommit.NormalizedAuthorName;
            var period = PeriodsDic[currentCommit.PeriodId.Value];

            foreach (var change in changes)
            {
                var canonicalPath=change.CanonicalPath;
                // should we consider Canonical Path or Path?
                if (!CommitBasedKnowledgeMap.ContainsKey(canonicalPath))
                    CommitBasedKnowledgeMap[canonicalPath] = new Dictionary<string, DeveloperFileCommitDetail>();

                if (!CommitBasedKnowledgeMap[canonicalPath].ContainsKey(devName))
                {
                    CommitBasedKnowledgeMap[canonicalPath][devName]
                    = new DeveloperFileCommitDetail()
                    {
                        FilePath = canonicalPath,
                        Developer = Developers[devName],
                    };
                }

                if(!CommitBasedKnowledgeMap[canonicalPath][devName].Periods.Any(q=>q.Id==period.Id))
                        CommitBasedKnowledgeMap[canonicalPath][devName].Periods.Add(period);

                CommitBasedKnowledgeMap[canonicalPath][devName].Commits.Add(currentCommit);
            }
        }
    }

    public class KnowledgeMap
    {
        public Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>> CommitBasedKnowledgeMap;
        public Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> ReviewBasedKnowledgeMap;

        public Dictionary<long, List<string>> PullRequestReviewers { get; internal set; }
    }

    public class DeveloperFileReveiewDetail
    {
        public string FilePath { get; set; }
        public Developer Developer { get; set; }
        public List<Period> Periods { get; set; } = new List<Period>();
        public List<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
    }

    public class DeveloperFileCommitDetail
    {
        public string FilePath { get; set; }
        public Developer Developer { get; set; }
        public List<Period> Periods { get; set; } = new List<Period>();
        public List<Commit> Commits { get; set; } = new List<Commit>();
    }

    public class PullRequestContext
    {
        public string PRSubmitterNormalizedName { get; set; }
        internal Developer[] availableDevelopers;
        public string[] ActualReviewers { get; internal set; }
        public PullRequestFile[] PullRequestFiles { get; internal set; }
        public PullRequest PullRequest { get; internal set; }
        public KnowledgeMap KnowledgeMap { get; internal set; }
        public Dictionary<string, string> CanononicalPathMapper { get; internal set; }
        public Period Period { get; internal set; }
        public ReadOnlyDictionary<string, Developer> Developers { get; internal set; }
        public Dictionary<string, Dictionary<string, FileBlame>> Blames { get; internal set; }
    }

    public class CommitPullRequest
    {
        public Commit Commit { get; set; }
        public PullRequest MergedPullRequest { get; set; }
    }

    public class FileBlame
    {
        public int TotalAuditedLines { get; set; }
        public string FileName { get; set; }
        public Period Period { get; set; }
        public string NormalizedDeveloperName { get; set; }
    }
}
