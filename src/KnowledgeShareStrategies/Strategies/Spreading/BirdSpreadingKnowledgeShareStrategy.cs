using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class BirdSpreadingKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        public BirdSpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            foreach (var pullRequestFile in pullRequestContext.PullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);
                if (canonicalPath == null)
                {
                    continue;
                }

                var fileExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetFileExpertise(canonicalPath);

                if (fileExpertise.TotalComments == 0)
                {
                    continue;
                }

                var reviewerExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(canonicalPath, reviewer.DeveloperName);

                if (reviewerExpertise == (0, 0, null))
                {
                    continue;
                }

                var scoreTotalComments = reviewerExpertise.TotalComments / (double)fileExpertise.TotalComments;
                var scoreTotalWorkDays = reviewerExpertise.TotalWorkDays / (double)fileExpertise.TotalWorkDays;
                var scoreRecency = (fileExpertise.RecentWorkDay == reviewerExpertise.RecentWorkDay)
                    ? 1
                    : 1 / (fileExpertise.RecentWorkDay - reviewerExpertise.RecentWorkDay).Value.TotalDays;

                var score = scoreTotalComments + scoreTotalWorkDays + scoreRecency;

                return score;
            }

            return 0;
        }
    }
}
