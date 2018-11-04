
using System;

namespace RelationalGit
{
    public abstract class KnowledgeShareStrategy
    {
        public static KnowledgeShareStrategy Create(string knowledgeShareStrategyType)
        {
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Nothing)
            {
                return new NothingKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ActualReviewers)
            {
                return new ActualKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Ideal)
            {
                return new IdealKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RealisticIdeal)
            {
                return new RealisticIdealKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedExpertiseReviewers)
            {
                return new CommitBasedKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.FileBasedExpertiseReviewers)
            {
                return new FileBasedKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedSpreadingReviewers)
            {
                return new SpreadingKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.SpreadingKnowledge2)
            {
                return new SpreadingKnowledgeShareStrategy2();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.BlameBasedSpreadingReviewers)
            {
                return new BlameBasedKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ReviewBasedSpreadingReviewers)
            {
                return new ReviewBasedKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RendomReviewers)
            {
                return new RandomKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RealisticRandomSpreading)
            {
                return new RealisticRandomSpreadingKnowledgeShareStrategy();
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RandomSpreading)
            {
                return new RandomSpreadingKnowledgeShareStrategy();
            }

            throw new ArgumentException($"invalid {nameof(knowledgeShareStrategyType)}");
        }
        internal abstract string[] RecommendReviewers(PullRequestContext pullRequestContext);
    }
}
