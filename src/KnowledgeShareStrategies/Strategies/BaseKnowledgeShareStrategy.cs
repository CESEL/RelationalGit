
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

namespace RelationalGit
{
    public abstract class BaseKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        internal override string[] RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if (pullRequestContext.ActualReviewers.Count() == 0)
                return new string[0];

            var sortedDevelopersKnowledge = GetSortedDevelopersKnowledge(pullRequestContext).ToArray();

            var leastKnowledgedReviewer = WhoHasTheLeastKnowledge(pullRequestContext, sortedDevelopersKnowledge);
            var mostKnowledgedReviewer = WhoHasTheMostKnowlege(pullRequestContext, sortedDevelopersKnowledge);

            var recommendedReviewers = RepleaceLeastWithMostKnowledged(pullRequestContext, leastKnowledgedReviewer, mostKnowledgedReviewer);

            return recommendedReviewers;
        }

        private string[] RepleaceLeastWithMostKnowledged(PullRequestContext pullRequestContext, string leastKnowledgedReviewer, string mostKnowledgedReviewer)
        {
            var actualReviewers = pullRequestContext.ActualReviewers;

            if (mostKnowledgedReviewer == null)
                return actualReviewers;

            var index = Array.IndexOf(actualReviewers, leastKnowledgedReviewer);
            actualReviewers[index] = mostKnowledgedReviewer;
            return actualReviewers;
        }

        private string WhoHasTheMostKnowlege(PullRequestContext pullRequestContext, DeveloperKnowledge[] sortedDevelopersKnowledge)
        { 
            var actualReviewers = pullRequestContext.ActualReviewers;

            for (var i = sortedDevelopersKnowledge.Length - 1; i >= 0; i--)
            {
                var isReviewer = actualReviewers.Any(q => q == sortedDevelopersKnowledge[i].DeveloperName);
                var isPrSubmitter = sortedDevelopersKnowledge[i].DeveloperName==pullRequestContext.PRSubmitterNormalizedName;
                var isAvailable = pullRequestContext.AvailableDevelopers.Any(q=>q.NormalizedName==sortedDevelopersKnowledge[i].DeveloperName);

                if (!isReviewer && !isPrSubmitter && isAvailable)
                    return sortedDevelopersKnowledge[i].DeveloperName;
            }

            return null;
        }

        private string WhoHasTheLeastKnowledge(PullRequestContext pullRequestContext, DeveloperKnowledge[] sortedDevelopersKnowledge)
        {
            var leastRank = int.MaxValue;
            var actualReviewers = pullRequestContext.ActualReviewers;

            for (var i = 0; i < actualReviewers.Length; i++)
            {
                var rank = Array.FindIndex(sortedDevelopersKnowledge, q => q.DeveloperName == actualReviewers[i]);

                if (rank == -1)
                    return actualReviewers[i];

                if (rank < leastRank)
                    leastRank = rank;
            }

            return sortedDevelopersKnowledge[leastRank].DeveloperName;
        }
        private IEnumerable<DeveloperKnowledge> GetSortedDevelopersKnowledge(PullRequestContext pullRequestContext)
        {
            var developersKnowledge = new Dictionary<string, DeveloperKnowledge>();

            foreach (var file in pullRequestContext.PullRequestFiles)
            {
                AddFileOwnership(pullRequestContext, developersKnowledge, file);
            }
            
            return SortDevelopersKnowledge(developersKnowledge.Values.ToArray(),pullRequestContext);
        }

        protected abstract IEnumerable<DeveloperKnowledge> SortDevelopersKnowledge(DeveloperKnowledge[] developerKnowledges,PullRequestContext pullRequestContext);
        private void AddFileOwnership(PullRequestContext pullRequestContext,Dictionary<string, DeveloperKnowledge> developersKnowledge,PullRequestFile file)
        {
            var canonicalPath = pullRequestContext.CanononicalPathMapper[file.FileName];

            CalculateModificationExpertise();
            CalculateReviewExpertise();

            void CalculateModificationExpertise()
            {
                var fileCommitsDetail = pullRequestContext.KnowledgeMap.CommitBasedKnowledgeMap[canonicalPath];

                if (fileCommitsDetail == null)
                    return;

                foreach (var devCommitDetail in fileCommitsDetail.Values)
                {
                    var devName = devCommitDetail.Developer.NormalizedName;

                    var fileBlame = pullRequestContext.Blames[canonicalPath]?.GetValueOrDefault(devName, null);

                    var totalAuditedLines = fileBlame != null ? fileBlame.TotalAuditedLines : 0;

                    AddModificationOwnershipDetail(developersKnowledge, devCommitDetail, totalAuditedLines);
                }
            }

            void CalculateReviewExpertise()
            {
                var fileReviewDetails = pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap[canonicalPath];

                if (fileReviewDetails == null)
                    return;

                foreach (var devReviewDetail in fileReviewDetails.Values)
                {
                    var devName = devReviewDetail.Developer.NormalizedName;

                    var hasCommittedThisFileBefore = pullRequestContext
                        .KnowledgeMap
                        .CommitBasedKnowledgeMap.IsPersonHasCommittedThisFile(devName, canonicalPath);

                    AddReviewOwnershipDetail(developersKnowledge, devReviewDetail, hasCommittedThisFileBefore);
                }
            }
        }

        private void AddReviewOwnershipDetail(Dictionary<string, DeveloperKnowledge> developersKnowledge, DeveloperFileReveiewDetail developerFileReveiewDetail, bool hasCommittedThisFileBefore)
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

            if(!hasCommittedThisFileBefore)
                developersKnowledge[developerName].NumberOfTouchedFiles++;

            developersKnowledge[developerName].NumberOfReviewedFiles++;
        }

        private void AddModificationOwnershipDetail(Dictionary<string, DeveloperKnowledge> developersKnowledge, DeveloperFileCommitDetail developerFileCommitsDetail,int totalAuditedLines)
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
            developersKnowledge[developerName].NumberOfAuthoredLines+=totalAuditedLines;
        }
    }
}
