﻿using Microsoft.Extensions.Logging;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class ReviewBasedKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        public ReviewBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var totalReviews = pullRequestContext.PullRequestKnowledgeables.Sum(q => q.NumberOfReviews);

            if(totalReviews == 0)
            {
                return 0;
            }

            return reviewer.NumberOfReviews / (double)totalReviews;
        }
    }
}