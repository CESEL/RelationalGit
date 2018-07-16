
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
    public abstract class ExpertiseBasedKnowledgeShareStrategy : RecommendingReviewersKnowledgeShareStrategy
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
                
                if (!isReviewer && !isPrSubmitter)
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
        private void AddFileOwnership(PullRequestContext pullRequestContext,
        Dictionary<string, DeveloperKnowledge> developersKnowledge,
        PullRequestFile file)
        {
            var canonicalPath = pullRequestContext
                .CanononicalPathMapper[file.FileName];

            var developersFileCommitsDetails = pullRequestContext
                .KnowledgeMap
                .CommitBasedKnowledgeMap
                .GetValueOrDefault(canonicalPath);
            
            if(developersFileCommitsDetails==null)
                return;

            foreach (var developerFileCommitsDetail in developersFileCommitsDetails.Values)
            {

                var devName = developerFileCommitsDetail.Developer.NormalizedName;
                
                var fileBlame  = pullRequestContext.Blames
                .GetValueOrDefault(canonicalPath,null)
                ?.GetValueOrDefault(devName,null);

                var totalAuditedLines = fileBlame!=null? fileBlame.TotalAuditedLines : 0;

                AddOwnershipDetail(developersKnowledge, developerFileCommitsDetail,totalAuditedLines);
            }

            var developerFileReviewDetails = pullRequestContext
            .KnowledgeMap
            .ReviewBasedKnowledgeMap
            .GetValueOrDefault(canonicalPath);

            if (developerFileReviewDetails == null)
                return;

            foreach (var developerFileReviewDetail in developerFileReviewDetails.Values)
            {
                var hasCommittedThisFileBefore=IsPersonHasCommittedThisFile(
                developerFileReviewDetail.Developer.NormalizedName
                ,canonicalPath
                ,pullRequestContext.KnowledgeMap);

                AddOwnershipDetail(developersKnowledge, developerFileReviewDetail,hasCommittedThisFileBefore);
            }

        }

        private void AddOwnershipDetail(Dictionary<string, DeveloperKnowledge> developersKnowledge, DeveloperFileReveiewDetail developerFileReveiewDetail, bool hasCommittedThisFileBefore)
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

        private bool IsPersonHasCommittedThisFile(string normalizedName, string canonicalPath, KnowledgeMap knowledgeMap)
        {
            var developersFileCommitsDetails = knowledgeMap.CommitBasedKnowledgeMap[canonicalPath];
            return developersFileCommitsDetails.Any(q=>q.Value.Developer.NormalizedName==normalizedName);
        }

        private void AddOwnershipDetail(Dictionary<string, DeveloperKnowledge> developersKnowledge, DeveloperFileCommitDetail developerFileCommitsDetail
        ,int totalAuditedLines)
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

        public class DeveloperKnowledge
        {
            public int NumberOfTouchedFiles { get; set; }
            public int NumberOfReviewedFiles { get; set; }
            public int NumberOfCommittedFiles { get; set; }
            public int NumberOfCommits { get; set; }
            public int NumberOfReviews { get; set; }
            public string DeveloperName { get; set; }
            public int NumberOfAuthoredLines { get; internal set; }
        }
    }
}
