using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Collections.Generic;

namespace RelationalGit.Recommendation
{
    public class CHRevRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        public CHRevRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var score = 0.0;

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

                score += scoreTotalComments + scoreTotalWorkDays + scoreRecency;
            }

            return score / (3*pullRequestContext.PullRequestFiles.Length);
        }
    }
}
