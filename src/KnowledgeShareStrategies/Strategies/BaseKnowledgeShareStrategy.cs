
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

            pullRequestContext.SortPRKnowledgeables(SortPRKnowledgeables);

            var leastKnowledgedReviewer = pullRequestContext.WhoHasTheLeastKnowledge();
            var mostKnowledgedReviewer = pullRequestContext.WhoHasTheMostKnowlege();

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
        
        protected abstract DeveloperKnowledge[] SortPRKnowledgeables(PullRequestContext pullRequestContext);

    }
}
