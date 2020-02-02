using Microsoft.Extensions.Logging;
using RelationalGit.Recommendation;
using RelationalGit.Simulation;
using System;

namespace RelationalGit.KnowledgeShareStrategies
{
    public static class KnowledgeShareStrategyFactory
    {
        public static RecommendationStrategy Create(ILogger logger, string recommendationStrategy, string knowledgeSaveReviewerReplacementType, int? numberOfPeriodsForCalculatingProbabilityOfStay, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast)
        {
            if (recommendationStrategy == KnowledgeShareStrategyType.NoReviewer)
            {
                return new NoReviewerRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.Reality)
            {
                return new RealityRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.AuthorshipRec)
            {
                return new AuthorshipRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.LineRec)
            {
                return new LineRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.CHRev)
            {
                return new CHRevRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.RevOwnRec)
            {
                return new RevOwnRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.LearnRec)
            {
                return new LearnRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.RetentionRec)
            {
                return new RetentionRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.TurnoverRec)
            {
                return new TurnoverRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.RandomRec)
            {
                return new RandomRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.Sofia)
            {
                return new SofiaRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }
            else if (recommendationStrategy == KnowledgeShareStrategyType.ContributionRec)
            {
                return new ContributionRecRecommendationStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast);
            }

            throw new ArgumentException($"invalid {nameof(recommendationStrategy)}");
        }
    }
}
