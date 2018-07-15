
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
    public class CommitBasedExpertiseKnowledgeShareStrategy : RecommendingReviewersKnowledgeShareStrategy
    {
        internal override string[] RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if(pullRequestContext.ActualReviewers.Count()==0)
                return new string[0];

            var devsTotalCommitsOnPRFiles=GetDevsKnowledge(
                pullRequestContext.KnowledgeMap,
                pullRequestContext.PullRequestFiles,
                pullRequestContext.CanononicalPathMapper);

            var leastKnowledgedReviewer = WhoHasTheLeastKnowledge(pullRequestContext.ActualReviewers,devsTotalCommitsOnPRFiles);
            var mostKnowledgedReviewer = WhoHasTheMostKnowledge(pullRequestContext.ActualReviewers,devsTotalCommitsOnPRFiles);

            var recommendedReviewers = RepleaceLeastWithMostKnowledged(pullRequestContext.ActualReviewers,leastKnowledgedReviewer,mostKnowledgedReviewer);

            return recommendedReviewers;
        }

        private string[] RepleaceLeastWithMostKnowledged(string[] actualReviewers, string leastKnowledgedReviewer, string mostKnowledgedReviewer)
        {
            if(mostKnowledgedReviewer==null)
                return actualReviewers;

            var index = Array.IndexOf(actualReviewers,leastKnowledgedReviewer);
            actualReviewers[index]=mostKnowledgedReviewer;
            return actualReviewers;
        }

        private string WhoHasTheMostKnowledge(string[] actualReviewers, Dictionary<string, int> devsTotalCommitsOnPRFiles)
        {
            var mostKnowledgedAmount= int.MinValue;
            string mostKnowledgedReviewer=null;

            foreach(var devTotalCommitsOnPRFile in devsTotalCommitsOnPRFiles)
            {
                var knowledge = devTotalCommitsOnPRFile.Value;
                var devName = devTotalCommitsOnPRFile.Key;

                // we do not want to resuggest actual reviewers.
                var isReviewer = actualReviewers.Any(q=>q==devName);

                if(knowledge>mostKnowledgedAmount && !isReviewer)
                {
                    mostKnowledgedReviewer=devName;
                    mostKnowledgedAmount=knowledge;
                }
            }

            // it will be null if all the committers be among actual reviewers
            return mostKnowledgedReviewer;
        }

        private string WhoHasTheLeastKnowledge(string[] actualReviewers, Dictionary<string, int> devsTotalCommitsOnPRFiles)
        {
            var leastKnowledgedAmount= int.MaxValue;
            string leastKnowledgedReviewer=null;

            foreach(var actualReviewer in actualReviewers)
            {
                // it's possible that a reviewer has no commits on PR files;
                var knowledge = devsTotalCommitsOnPRFiles.GetValueOrDefault(actualReviewer,0);

                if(knowledge<leastKnowledgedAmount)
                {
                    leastKnowledgedReviewer=actualReviewer;
                    leastKnowledgedAmount=knowledge;
                }
            }

            return leastKnowledgedReviewer;
        }


        private Dictionary<string,int> GetDevsKnowledge(KnowledgeMap knowledgeMap, PullRequestFile[] pullRequestFiles,Dictionary<string, string> canononicalPathMapper)
        {
            var devsTotalCommitsOnPRFiles = new Dictionary<string,int>(); 

            foreach(var file in pullRequestFiles)
            {
                AddFileOwnersAndTheirCommits(knowledgeMap, canononicalPathMapper, devsTotalCommitsOnPRFiles, file);
            }

            return devsTotalCommitsOnPRFiles;
        }

        private static void AddFileOwnersAndTheirCommits(KnowledgeMap knowledgeMap, Dictionary<string, string> canononicalPathMapper, Dictionary<string, int> devsTotalCommitsOnPRFiles, PullRequestFile file)
        {
            var canonicalPath = canononicalPathMapper[file.FileName];
            var developersFileCommitsDetails = knowledgeMap.CommitBasedKnowledgeMap[canonicalPath];

            foreach (var developerFileCommitsDetail in developersFileCommitsDetails.Values)
            {
                AddFileCommitDetail(devsTotalCommitsOnPRFiles, developerFileCommitsDetail);
            }
        }

        private static void AddFileCommitDetail(Dictionary<string, int> devsTotalCommitsOnPRFiles, DeveloperFileCommitDetail developerFileCommitsDetail)
        {
            var devName = developerFileCommitsDetail.Developer.NormalizedName;
            var totalCommitsOnFile = developerFileCommitsDetail.Commits.Count();

            if (devsTotalCommitsOnPRFiles.ContainsKey(devName))
                devsTotalCommitsOnPRFiles[devName] = 0;

            devsTotalCommitsOnPRFiles[devName] += totalCommitsOnFile;
        }
    }
}
