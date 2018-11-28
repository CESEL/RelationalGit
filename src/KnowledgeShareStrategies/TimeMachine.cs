
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
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public class TimeMachine
    {
        #region Fields
        private Commit[] SortedCommits { get; set; }
        public BlameBasedKnowledgeMap BlameBasedKnowledgeMap { get; private set; }
        private Dictionary<string, Commit> CommitsDic { get; set; }
        private Dictionary<string, Developer> DevelopersDic { get; set; }
        private DeveloperContribution[] DevelopersContributions { get; set; }
        private Dictionary<string, List<CommittedChange>> CommittedChangesDic { get; set; }
        private Dictionary<int, PullRequest> PullRequestsDic { get; set; }
        private Dictionary<long, List<PullRequestFile>> PullRequestFilesDic { get; set; }
        private Dictionary<long, List<string>> PullRequestReviewersDic { get; set; }
        private Dictionary<long, PullRequestRecommendationResult> PullRequestSimulatedRecommendationDic { get; set; } = new Dictionary<long, PullRequestRecommendationResult>();
        private PullRequestReviewerComment[] PullRequestReviewComments { get; set; }
        private Dictionary<string, string> CanononicalPathMapper { get; set; }
        private UsernameRepository UsernameRepository { get; set; }
        private Dictionary<long, Period> PeriodsDic { get; set; }

        private CommitBasedKnowledgeMap CommitBasedKnowledgeMap = new CommitBasedKnowledgeMap();

        private ReviewBasedKnowledgeMap ReviewBasedKnowledgeMap = new ReviewBasedKnowledgeMap();

        private ILogger _logger;

        private KnowledgeShareStrategy KnowledgeShareStrategy;
        private int _firstSimulationPeriod;

        #endregion

        public TimeMachine(KnowledgeShareStrategy knowledgeShareStrategy, ILogger logger)
        {
            _logger = logger;
            KnowledgeShareStrategy = knowledgeShareStrategy;
        }
        public void Initiate(Commit[] commits, CommitBlobBlame[] commitBlobBlames, Developer[] developers, DeveloperContribution[] developersContributions,
        CommittedChange[] committedChanges, PullRequest[] pullRequests, PullRequestFile[] pullRequestFiles,IssueComment[] issueComments,
         PullRequestReviewer[] pullRequestReviewers, PullRequestReviewerComment[] pullRequestReviewComments,
         Dictionary<string, string> canononicalPathMapper, GitHubGitUser[] githubGitUsers, Period[] periods, int firstPeriod)
        {
            _logger.LogInformation("{datetime}: Trying to initialize TimeMachine.", DateTime.Now);

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

            BlameBasedKnowledgeMap = GetBlameBasedKnowledgeMap(commitBlobBlames);
            CommittedChangesDic = GetCommittedChangesDictionary(committedChanges);
            PullRequestFilesDic = GetPullRequestFilesDictionary(pullRequestFiles);
            PullRequestReviewersDic = GetPullRequestReviewersDictionary(pullRequestReviewers, pullRequestReviewComments,issueComments);

            GetCommitsPullRequests(SortedCommits, pullRequests);

            _firstSimulationPeriod = firstPeriod;

            _logger.LogInformation("{datetime}: TimeMachine is initialized.", DateTime.Now);

        }

        public KnowledgeDistributionMap FlyInTime()
        {
            _logger.LogInformation("{datetime}: flying in time has started.", DateTime.Now);

            var knowledgeMap = new KnowledgeDistributionMap()
            {
                CommitBasedKnowledgeMap = CommitBasedKnowledgeMap,
                ReviewBasedKnowledgeMap = ReviewBasedKnowledgeMap,
                PullRequestSimulatedRecommendationMap = PullRequestSimulatedRecommendationDic,
                BlameBasedKnowledgeMap = BlameBasedKnowledgeMap
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
            if (KnowledgeShareStrategy != null)
            {
                var pullRequestContext = GetPullRequestContext(pullRequest, knowledgeMap);

                var periodOfPullRequest = GetPeriodOfPullRequest(pullRequest).Id;
                
                if(periodOfPullRequest >= _firstSimulationPeriod)
                {
                    var recommendationResult = KnowledgeShareStrategy.Recommend(pullRequestContext);
                    PullRequestSimulatedRecommendationDic[pullRequest.Number] = recommendationResult;
                }
                else
                {
                    PullRequestSimulatedRecommendationDic[pullRequest.Number] = new PullRequestRecommendationResult(pullRequestContext.ActualReviewers, null)
                    {
                        ActualReviewers = pullRequestContext.ActualReviewers.Select(q => q.DeveloperName).ToArray(),
                        PullRequestNumber = pullRequest.Number,
                        IsSimulated = false
                    };
                }


            }
        }

        private PullRequestContext GetPullRequestContext(PullRequest pullRequest, KnowledgeDistributionMap knowledgeMap)
        {
           
            var pullRequestFiles = PullRequestFilesDic.GetValueOrDefault(pullRequest.Number, new List<PullRequestFile>()).ToArray();

            var period = GetPeriodOfPullRequest(pullRequest);

            var availableDevelopers = GetAvailableDevelopersOfPeriod(period).ToArray();

            var prSubmitter = UsernameRepository.GetByGitHubLogin(pullRequest.UserLogin);

            var pullRequestKnowledgeableDevelopers = GetPullRequestKnowledgeables(pullRequestFiles, knowledgeMap, period);

            var actualReviewers = GetKnowledgeOfActualReviewers(PullRequestReviewersDic.GetValueOrDefault(pullRequest.Number) ?? new List<string>(0), pullRequestKnowledgeableDevelopers);

            return new PullRequestContext()
            {
                PRSubmitterNormalizedName = prSubmitter?.NormalizedName,
                ActualReviewers = actualReviewers.ToArray(),
                PullRequestFiles = pullRequestFiles,
                AvailableDevelopers = availableDevelopers,
                PullRequest = pullRequest,
                KnowledgeMap = knowledgeMap,
                CanononicalPathMapper = CanononicalPathMapper,
                Period = period,
                Developers = new ReadOnlyDictionary<string, Developer>(DevelopersDic),
                Blames = BlameBasedKnowledgeMap.GetSnapshopOfPeriod(period.Id),
                PRKnowledgeables = pullRequestKnowledgeableDevelopers
            };

        }

        private IEnumerable<DeveloperKnowledge> GetKnowledgeOfActualReviewers(List<string> subset, DeveloperKnowledge[] superset)
        {
            foreach (var item in subset)
            {
                yield return superset.SingleOrDefault(q => q.DeveloperName == item)?? new DeveloperKnowledge()
                {
                    DeveloperName = item
                };
            }
        }

        private DeveloperKnowledge[] GetPullRequestKnowledgeables(PullRequestFile[] pullRequestFiles, KnowledgeDistributionMap knowledgeDistributionMap, Period period)
        {
            var developersKnowledge = new Dictionary<string, DeveloperKnowledge>();
            var blameSnapshot = BlameBasedKnowledgeMap.GetSnapshopOfPeriod(period.Id);

            foreach (var file in pullRequestFiles)
            {
                AddFileOwnership(knowledgeDistributionMap,blameSnapshot, developersKnowledge, file.FileName, CanononicalPathMapper);
            }

            return developersKnowledge.Values.ToArray();
        }

        internal static void AddFileOwnership(KnowledgeDistributionMap knowledgeDistributionMap,BlameSnapshot blameSnapshot, Dictionary<string, DeveloperKnowledge> developersKnowledge, string filePath, Dictionary<string,string> canononicalPathMapper)
        {
            var canonicalPath = canononicalPathMapper[filePath];

            CalculateModificationExpertise();
            CalculateReviewExpertise();

            void CalculateModificationExpertise()
            {
                var fileCommitsDetail = knowledgeDistributionMap.CommitBasedKnowledgeMap[canonicalPath];

                if (fileCommitsDetail == null)
                    return;

                foreach (var devCommitDetail in fileCommitsDetail.Values)
                {
                    var devName = devCommitDetail.Developer.NormalizedName;

                    var fileBlame = blameSnapshot[canonicalPath]?.GetValueOrDefault(devName, null);

                    var totalAuditedLines = fileBlame != null ? fileBlame.TotalAuditedLines : 0;

                    AddModificationOwnershipDetail(developersKnowledge, devCommitDetail, totalAuditedLines);
                }
            }

            void CalculateReviewExpertise()
            {
                var fileReviewDetails = knowledgeDistributionMap.ReviewBasedKnowledgeMap[canonicalPath];

                if (fileReviewDetails == null)
                    return;

                foreach (var devReviewDetail in fileReviewDetails.Values)
                {
                    var devName = devReviewDetail.Developer.NormalizedName;

                    var hasCommittedThisFileBefore = knowledgeDistributionMap
                        .CommitBasedKnowledgeMap.IsPersonHasCommittedThisFile
                        (devName, canonicalPath);

                    AddReviewOwnershipDetail(developersKnowledge, devReviewDetail, hasCommittedThisFileBefore);
                }
            }
        }

        private static void AddReviewOwnershipDetail(Dictionary<string, DeveloperKnowledge> developersKnowledge, DeveloperFileReveiewDetail developerFileReveiewDetail, bool hasCommittedThisFileBefore)
        {
            var developerName = developerFileReveiewDetail.Developer.NormalizedName;

            if (!developersKnowledge.ContainsKey(developerName))
            {
                developersKnowledge[developerName] = new DeveloperKnowledge()
                {
                    DeveloperName = developerName
                };
            }

            developersKnowledge[developerName].NumberOfReviews += developerFileReveiewDetail.PullRequests.Count();

            if (!hasCommittedThisFileBefore)
                developersKnowledge[developerName].NumberOfTouchedFiles++;

            developersKnowledge[developerName].NumberOfReviewedFiles++;
        }

        private static void AddModificationOwnershipDetail(Dictionary<string, DeveloperKnowledge> developersKnowledge, DeveloperFileCommitDetail developerFileCommitsDetail, int totalAuditedLines)
        {
            var developerName = developerFileCommitsDetail.Developer.NormalizedName;

            if (!developersKnowledge.ContainsKey(developerName))
            {
                developersKnowledge[developerName] = new DeveloperKnowledge()
                {
                    DeveloperName = developerName
                };
            }

            developersKnowledge[developerName].NumberOfCommits += developerFileCommitsDetail.Commits.Count();
            developersKnowledge[developerName].NumberOfTouchedFiles++;
            developersKnowledge[developerName].NumberOfCommittedFiles++;
            developersKnowledge[developerName].NumberOfAuthoredLines += totalAuditedLines;
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
            var reviewersNamesOfPullRequest = PullRequestSimulatedRecommendationDic[pullRequest.Number].SelectedReviewers.Select(reviewerName => DevelopersDic[reviewerName]);
            var period = GetPeriodOfPullRequest(pullRequest);

            // some of the pull requests have no modified files strangely
            // for example, https://github.com/dotnet/coreclr/pull/13534
            var filesOfPullRequest = PullRequestFilesDic.GetValueOrDefault(pullRequest.Number, new List<PullRequestFile>());

            foreach (var file in filesOfPullRequest)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(file.FileName);
                ReviewBasedKnowledgeMap.Add(canonicalPath, reviewersNamesOfPullRequest, pullRequest, period);
            }
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
                AssignKnowledgeToDeveloper(prMergedCommit, file.ChangeKind ,prSubmitter, period, canonicalPath);
            }
        }

        private void AssignKnowledgeToDeveloper(Commit commit,ChangeKind changeKind, string developerName, Period period, string filePath)
        {
            if (filePath == null || developerName == null)
                return;

            var developer = DevelopersDic[developerName];

            if (changeKind == ChangeKind.Deleted)
            {
                CommitBasedKnowledgeMap.Remove(filePath);
            }
            else if (changeKind != ChangeKind.Renamed) // if it's Added or Modified
            {
                CommitBasedKnowledgeMap.Add(filePath,changeKind, developer, commit, period);
            }
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
                AssignKnowledgeToDeveloper(commit,(ChangeKind)change.Status, developerName, period, canonicalPath);
            }
        }

        private Period GetPeriodOfCommit(Commit commit)
        {
            return PeriodsDic[commit.PeriodId.Value];
        }

        private BlameBasedKnowledgeMap GetBlameBasedKnowledgeMap(CommitBlobBlame[] commitBlobBlames)
        {
            var blameBasedKnowledgeMap = new BlameBasedKnowledgeMap();

            foreach (var commitBlobBlame in commitBlobBlames)
            {
                var commit = CommitsDic.GetValueOrDefault(commitBlobBlame.CommitSha);
                var periodId = commit.PeriodId.Value;
                var period = PeriodsDic[periodId];
                var filePath = commitBlobBlame.CanonicalPath;
                var devName = commitBlobBlame.NormalizedDeveloperIdentity;

                blameBasedKnowledgeMap.Add(period, filePath, devName, commitBlobBlame);
            }

            blameBasedKnowledgeMap.ComputeOwnershipPercentage();

            return blameBasedKnowledgeMap;
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

        private Dictionary<long, List<string>> GetPullRequestReviewersDictionary(PullRequestReviewer[] pullRequestReviewers,PullRequestReviewerComment[] pullRequestReviewComments,IssueComment[] issueComments)
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

            for (var i = 0; i < issueComments.Length; i++)
            {
                var prNumber = (int) issueComments[i].IssueNumber;

                if (ShouldConsiderComment(prNumber, issueComments[i]))
                    AssignReviewerToPullRequest(issueComments[i].UserLogin, prNumber, result);
            }

            return result;
        }

        private bool ShouldConsiderComment(int prNumber, PullRequestReviewerComment pullRequestReviewerComment)
        {
            var pr = PullRequestsDic[prNumber];

            var prMergedDateTime = pr.MergedAtDateTime;

                // if a comment has been left after merge, we don't consider the commenter
                // as a knoledgeable person about the PR
            if (prMergedDateTime < pullRequestReviewerComment.CreatedAtDateTime || pullRequestReviewerComment.UserLogin == pr.UserLogin)
                return false;

            return true;
        }

        private bool ShouldConsiderComment(int prNumber, IssueComment issueComment)
        {
            var pr = PullRequestsDic[prNumber];

            var prMergedDateTime = pr.MergedAtDateTime;

            // if a comment has been left after merge, we don't consider the commenter
            // as a knoledgeable person about the PR
            if (prMergedDateTime < issueComment.CreatedAtDateTime || issueComment.UserLogin==pr.UserLogin)
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
