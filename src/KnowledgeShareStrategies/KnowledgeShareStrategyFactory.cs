using Microsoft.Extensions.Logging;
using RelationalGit.Recommendation;
using RelationalGit.Simulation;
using System;

namespace RelationalGit.KnowledgeShareStrategies
{
    public static class KnowledgeShareStrategyFactory
    {
        public static KnowledgeShareStrategy Create(ILogger logger, string knowledgeShareStrategyType, string knowledgeSaveReviewerReplacementType, int? numberOfPeriodsForCalculatingProbabilityOfStay, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
        {
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Nothing)
            {
                return new NothingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ActualReviewers)
            {
                return new ActualKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedKnowledgeShare)
            {
                return new CommitBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.BlameBasedKnowledgeShare)
            {
                return new BlameBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.BirdBasedKnowledgeShare)
            {
                return new BirdSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ReviewBasedKnowledgeShare)
            {
                return new ReviewBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.SpreadingBasedKnowledgeShare)
            {
                return new SpreadingBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.PersistBasedKnowledgeShare)
            {
                return new PersistBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.PersistSpreadingBasedKnowledgeShare)
            {
                return new PersistSpreadingBasedSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RandomBasedKnowledgeShare)
            {
                return new RandomBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }

            throw new ArgumentException($"invalid {nameof(knowledgeShareStrategyType)}");
        }
    }
}
