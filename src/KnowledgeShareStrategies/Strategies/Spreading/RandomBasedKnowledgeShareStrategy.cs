using Microsoft.Extensions.Logging;
using System;

namespace RelationalGit.KnowledgeShareStrategies.Strategies.Spreading
{
    public class RandomBasedKnowledgeShareStrategy : ScoreBasedSpreadingKnowledgeShareStrategy
    {
        private Random _rnd = new Random();

        public RandomBasedKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption,changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            return _rnd.NextDouble();
        }
    }
}
