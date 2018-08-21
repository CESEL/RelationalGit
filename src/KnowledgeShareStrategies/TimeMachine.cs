﻿
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
        private Commit[] SortedCommits { get; set; }
        public Dictionary<long, Dictionary<string, Dictionary<string, FileBlame>>> CommitBlobBlamesDic { get; private set; }
        private Dictionary<string, Commit> CommitsDic { get; set; }
        private Dictionary<string, Developer> DevelopersDic { get; set; }
        private DeveloperContribution[] DevelopersContributions { get; set; }
        private Dictionary<string, List<CommittedChange>> CommittedChangesDic { get; set; }
        private Dictionary<int, PullRequest> PullRequestsDic { get; set; }
        private Dictionary<long, List<PullRequestFile>> PullRequestFilesDic { get; set; }
        private Dictionary<long, List<string>> PullRequestReviewersDic { get; set; }
        private PullRequestReviewerComment[] PullRequestReviewComments { get; set; }
        private Dictionary<string, string> CanononicalPathMapper { get; set; }
        private UsernameRepository UsernameRepository { get; set; }
        private Dictionary<long, Period> PeriodsDic { get; set; }
        private Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>> CommitBasedKnowledgeMap = new Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>>();
        private Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> ReviewBasedKnowledgeMap = new Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>>();
        private ILogger _logger;
        private Func<PullRequestContext, string[]> ChangeThePastByRecommendingReviewersFunc;

        #endregion

        public TimeMachine(Func<PullRequestContext, string[]> changeThePastByRecommendingReviewersFunc,ILogger logger)
        {
            _logger = logger;
            ChangeThePastByRecommendingReviewersFunc = changeThePastByRecommendingReviewersFunc;
        }
        public void Initiate(Commit[] commits, CommitBlobBlame[] commitBlobBlames, Developer[] developers, DeveloperContribution[] developersContributions,
        CommittedChange[] committedChanges, PullRequest[] pullRequests, PullRequestFile[] pullRequestFiles,
         PullRequestReviewer[] pullRequestReviewers, PullRequestReviewerComment[] pullRequestReviewComments,
         Dictionary<string, string> canononicalPathMapper, GitHubGitUser[] githubGitUsers, Period[] periods)
        {
            UsernameRepository = new UsernameRepository(githubGitUsers, developers);

            SortedCommits = commits.OrderBy(q => q.AuthorDateTime).ToArray();

            DevelopersContributions = developersContributions;
            PullRequestReviewComments = pullRequestReviewComments;
            CanononicalPathMapper = canononicalPathMapper;

            CommitsDic = commits.ToDictionary(q => q.Sha);
            PeriodsDic = periods.ToDictionary(q => q.Id);

            GetDevelopersContributions(developers, developersContributions);
            DevelopersDic = developers.ToDictionary(q => q.NormalizedName);

            PullRequestsDic = pullRequests.ToDictionary(q => q.Number);

            CommitBlobBlamesDic = GetCommitBlobBlamesDictionary(commitBlobBlames);
            CommittedChangesDic = GetCommittedChangesDictionary(committedChanges);
            PullRequestFilesDic = GetPullRequestFilesDictionary(pullRequestFiles);
            PullRequestReviewersDic = GetPullRequestReviewersDictionary(pullRequestReviewers, pullRequestReviewComments);


            GetCommitsPullRequests(SortedCommits, pullRequests);
        }

        public KnowledgeDistributionMap FlyInTime()
        {
            _logger.LogInformation("{datetime}: flying in time has started.", DateTime.Now);

            var knowledgeMap = new KnowledgeDistributionMap()
            {
                CommitBasedKnowledgeMap = this.CommitBasedKnowledgeMap,
                ReviewBasedKnowledgeMap = this.ReviewBasedKnowledgeMap,
                PullRequestReviewers = PullRequestReviewersDic,
                BlameDistribution = CommitBlobBlamesDic
            };

            foreach (var commit in SortedCommits)
            {
                UpdateCommitBasedKnowledgeMap(commit);
                UpdateReviewBasedKnowledgeMap(knowledgeMap, commit, commit.MergedPullRequest);
            }

            _logger.LogInformation("{datetime}: flying in time has finished.", DateTime.Now);

            return knowledgeMap;
        }

        #region Private Methods

        private void UpdateReviewBasedKnowledgeMap(KnowledgeDistributionMap knowledgeMap, Commit commit, PullRequest pullRequest)
        {
            if (pullRequest != null)
            {
                AddProposedChangesToPrSubmitterKnowledge(pullRequest, commit);

                ChangeThePastByRecommendingReviewers(knowledgeMap, pullRequest);

                UpdateReviewBasedKnowledgeMap(pullRequest);
            }
        }

        private void ChangeThePastByRecommendingReviewers(KnowledgeDistributionMap knowledgeMap, PullRequest pullRequest)
        {
            if (ChangeThePastByRecommendingReviewersFunc != null)
            {
                var pullRequestContext = GetPullRequestContext(pullRequest, knowledgeMap);
                var recommendedReviewers = ChangeThePastByRecommendingReviewersFunc(pullRequestContext);
                PullRequestReviewersDic[pullRequest.Number] = recommendedReviewers.ToList();
            }
        }
        private PullRequestContext GetPullRequestContext(PullRequest pullRequest, KnowledgeDistributionMap knowledgeMap)
        {
            var actualReviewers = PullRequestReviewersDic
            .GetValueOrDefault(pullRequest.Number, defaultValue: new List<string>())
            .ToArray();

            var pullRequestFiles = this.PullRequestFilesDic
            .GetValueOrDefault(pullRequest.Number, new List<PullRequestFile>())
            .ToArray();

            var period = GetPeriodOfPullRequest(pullRequest);

            var availableDevelopers = GetAvailableDevelopersOfPeriod(period)
            .ToArray();

            var prSubmitter = UsernameRepository.GetByGitHubLogin(pullRequest.UserLogin);

            return new PullRequestContext()
            {
                PRSubmitterNormalizedName = prSubmitter?.NormalizedName,
                ActualReviewers = actualReviewers,
                PullRequestFiles = pullRequestFiles,
                availableDevelopers = availableDevelopers,
                PullRequest = pullRequest,
                KnowledgeMap = knowledgeMap,
                CanononicalPathMapper = CanononicalPathMapper,
                Period = period,
                Developers = new ReadOnlyDictionary<string, Developer>(DevelopersDic),
                Blames = CommitBlobBlamesDic[period.Id]
            };

        }
        private IEnumerable<Developer> GetAvailableDevelopersOfPeriod(Period period)
        {
            return DevelopersDic.Values.Where(dev => dev.FirstParticipationPeriodId <= period.Id && dev.LastParticipationPeriodId >= period.Id);
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
            var period = GetPeriodOfPullRequest(pullRequest);

            // some of the pull requests have no modified files strangely
            // for example, https://github.com/dotnet/coreclr/pull/13534
            var filesOfPullRequest = PullRequestFilesDic.GetValueOrDefault(pullRequest.Number, new List<PullRequestFile>());

            foreach (var file in filesOfPullRequest)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(file.FileName);
                AssignKnowledgeToReviewers(pullRequest, reviewersNamesOfPullRequest, period, canonicalPath);
            }
        }

        private void AssignKnowledgeToReviewers(PullRequest pullRequest, IEnumerable<string> reviewersNamesOfPullRequest, Period period, string filePath)
        {
            if (filePath == null)
                return;

            if (!ReviewBasedKnowledgeMap.ContainsKey(filePath))
                ReviewBasedKnowledgeMap[filePath] = new Dictionary<string, DeveloperFileReveiewDetail>();

            foreach (var reviewerName in reviewersNamesOfPullRequest)
            {
                AssignKnowledgeToReviewer(pullRequest,reviewerName, period, filePath);
            }
        }

        private void AssignKnowledgeToReviewer(PullRequest pullRequest, string reviewerName, Period period, string filePath)
        {
            if (!ReviewBasedKnowledgeMap[filePath].ContainsKey(reviewerName))
            {
                ReviewBasedKnowledgeMap[filePath][reviewerName] = new DeveloperFileReveiewDetail()
                {
                    FilePath = filePath,
                    Developer = DevelopersDic[reviewerName]
                };
            }

            if (!ReviewBasedKnowledgeMap[filePath][reviewerName].Periods.Any(q => q.Id == period.Id))
                ReviewBasedKnowledgeMap[filePath][reviewerName].Periods.Add(period);

            ReviewBasedKnowledgeMap[filePath][reviewerName].PullRequests.Add(pullRequest);
        }

        private void AddProposedChangesToPrSubmitterKnowledge(PullRequest pullRequest, Commit prMergedCommit)
        {
            // we assume the PR submitter is the dev who has modified the files
            // however it's not the case always. for example https://github.com/dotnet/coreclr/pull/1
            // we assume all the proposed file changes are committed and owned by the main pull request's author
            // which may not be correct in rare scenarios

            if (prMergedCommit == null || pullRequest == null)
                return;

            if (pullRequest.MergeCommitSha != prMergedCommit.Sha)
                return;

            // some of the pull requests have no modified files
            // https://github.com/dotnet/coreclr/pull/13534
            var pullRequestFiles = PullRequestFilesDic.GetValueOrDefault(pullRequest.Number, new List<PullRequestFile>());

            var prSubmitter = UsernameRepository.GetByGitHubLogin(pullRequest.UserLogin)?.NormalizedName;

            // we have ignored mega developers
            if(prSubmitter==null)
                return;

            var period = GetPeriodOfCommit(prMergedCommit);

            foreach (var file in pullRequestFiles)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(file.FileName);
                AssignKnowledgeToDeveloper(prMergedCommit, prSubmitter, period, canonicalPath);
            }
        }

        private void AssignKnowledgeToDeveloper(Commit commit, string developerName, Period period, string filePath)
        {
            if (filePath == null || developerName == null)
                return;

            if (!CommitBasedKnowledgeMap.ContainsKey(filePath))
                CommitBasedKnowledgeMap[filePath] = new Dictionary<string, DeveloperFileCommitDetail>();

            if (!CommitBasedKnowledgeMap[filePath].ContainsKey(developerName))
            {
                CommitBasedKnowledgeMap[filePath][developerName] = new DeveloperFileCommitDetail()
                {
                    FilePath = filePath,
                    Developer = DevelopersDic[developerName],
                };
            }

            if (!CommitBasedKnowledgeMap[filePath][developerName].Commits.Any(q => q.Sha == commit.Sha))
                CommitBasedKnowledgeMap[filePath][developerName].Commits.Add(commit);

            if (!CommitBasedKnowledgeMap[filePath][developerName].Periods.Any(q => q.Id == period.Id))
                CommitBasedKnowledgeMap[filePath][developerName].Periods.Add(period);
        }

        private void UpdateCommitBasedKnowledgeMap(Commit commit)
        {
            // merged commits probably contain no changes
            var changes = CommittedChangesDic.GetValueOrDefault(commit.Sha, new List<CommittedChange>());
            var developerName = commit.NormalizedAuthorName;
            var period = GetPeriodOfCommit(commit);

            foreach (var change in changes)
            {
                // should we consider Canonical Path or Path?
                var canonicalPath = change.CanonicalPath;
                AssignKnowledgeToDeveloper(commit, developerName, period, canonicalPath);
            }
        }

        private Period GetPeriodOfCommit(Commit commit)
        {
            return PeriodsDic[commit.PeriodId.Value];
        }

        private Dictionary<long, Dictionary<string, Dictionary<string, FileBlame>>> GetCommitBlobBlamesDictionary(CommitBlobBlame[] commitBlobBlames)
        {
            var commitBlobBlamesDic = new Dictionary<long, Dictionary<string, Dictionary<string, FileBlame>>>();

            UnifyBlames();
            ComputeOwnershipPercentage();

            return commitBlobBlamesDic;


            void UnifyBlames()
            {
                foreach (var commitBlobBlame in commitBlobBlames)
                {
                    var commit = CommitsDic.GetValueOrDefault(commitBlobBlame.CommitSha);
                    var periodId = commit.PeriodId.Value;
                    var period = PeriodsDic[periodId];
                    var filePath = commitBlobBlame.CanonicalPath;
                    var devName = commitBlobBlame.NormalizedDeveloperIdentity;

                    if (!commitBlobBlamesDic.ContainsKey(periodId))
                    {
                        commitBlobBlamesDic[periodId] = new Dictionary<string, Dictionary<string, FileBlame>>();
                    }

                    if (!commitBlobBlamesDic[periodId].ContainsKey(filePath))
                    {
                        commitBlobBlamesDic[periodId][filePath] = new Dictionary<string, FileBlame>();
                    }

                    if (!commitBlobBlamesDic[periodId][filePath].ContainsKey(devName))
                    {
                        commitBlobBlamesDic[periodId][filePath][devName] = new FileBlame()
                        {
                            FileName = filePath,
                            Period = period,
                            NormalizedDeveloperName = devName
                        };
                    }

                    commitBlobBlamesDic[periodId][filePath][devName].TotalAuditedLines += commitBlobBlame.AuditedLines;
                }
            }

            void ComputeOwnershipPercentage()
            {
                foreach (var periodId in commitBlobBlamesDic.Keys)
                {
                    foreach (var filePath in commitBlobBlamesDic[periodId].Keys)
                    {
                        var totalLines = (double)commitBlobBlamesDic[periodId][filePath].Values.Sum(q => q.TotalAuditedLines);

                        foreach (var developer in commitBlobBlamesDic[periodId][filePath].Keys)
                        {
                            commitBlobBlamesDic[periodId][filePath][developer].OwnedPercentage = commitBlobBlamesDic[periodId][filePath][developer].TotalAuditedLines / totalLines;
                        }
                    }
                }
            }
        }

        private void GetCommitsPullRequests(Commit[] commits, PullRequest[] pullRequests)
        {
            foreach (var commit in commits)
            {
                // It's possible that two pull requests have the same merge commit
                // it's a bug in github. We take only one of them in such a scenario.
                // https://github.com/dotnet/coreclr/pull/9909
                // https://github.com/dotnet/coreclr/pull/9379
                var mergedPullRequest = pullRequests.FirstOrDefault(q => q.MergeCommitSha == commit.Sha);
                commit.MergedPullRequest = mergedPullRequest;
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

        private Dictionary<long, List<string>> GetPullRequestReviewersDictionary(PullRequestReviewer[] pullRequestReviewers,PullRequestReviewerComment[] pullRequestReviewComments)
        {
            var result = new Dictionary<long, List<string>>();

            for (var i = 0; i < pullRequestReviewers.Length; i++)
            {
                var prNumber = (int)pullRequestReviewers[i].PullRequestNumber; // what a bullshit cast!! :(
                AssignReviewerToPullRequest(pullRequestReviewers[i].UserLogin,  prNumber,result);
            }

            for (var i = 0; i < pullRequestReviewComments.Length; i++)
            {
                var prNumber = pullRequestReviewComments[i].PullRequestNumber;

                if(ShouldConsiderComment(prNumber,pullRequestReviewComments[i]))
                    AssignReviewerToPullRequest(pullRequestReviewComments[i].UserLogin,  prNumber,result);
            }

            return result;
        }

        private bool ShouldConsiderComment(int prNumber, PullRequestReviewerComment pullRequestReviewerComment)
        {
            var prMergedDateTime = PullRequestsDic[prNumber].MergedAtDateTime;

                // if a comment has been left after merge, we don't consider the commenter
                // as a knoledgeable person about the PR
            if (prMergedDateTime < pullRequestReviewerComment.CreatedAtDateTime)
                return false;

            return true;
        }

        private void AssignReviewerToPullRequest(string reviewerName, int prNumber,Dictionary<long, List<string>> prReviewers)
        {
            var prSubmitter = PullRequestsDic[prNumber].UserLogin;

            if (prSubmitter == reviewerName)
                return;

            if (!prReviewers.ContainsKey(prNumber))
                prReviewers[prNumber] = new List<string>();

            var reviewerNormalizedName = UsernameRepository.GetByGitHubLogin(reviewerName)?.NormalizedName;

            // Pull Request Reviewers and Comments contains duplicated items, So we need to check for it
            // https://api.github.com/repos/dotnet/coreclr/pulls/7886/reviews
            if (reviewerNormalizedName != null && !prReviewers[prNumber].Any(q => q == reviewerNormalizedName))
                prReviewers[prNumber].Add(reviewerNormalizedName);
        }


        private void GetDevelopersContributions(Developer[] developers, DeveloperContribution[] developersContributions)
        {
            foreach (var developer in developers)
            {
                var contributions = developersContributions
                    .Where(q => q.NormalizedName == developer.NormalizedName);

                developer.AddContributions(contributions);
            }
        }

        #endregion
    }
}
