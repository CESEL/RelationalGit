using Microsoft.Extensions.Logging;
using RelationalGit.KnowledgeShareStrategies.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{
    public abstract class KnowledgeShareStrategy
    {
        protected string ReviewerReplacementStrategyType { get; private set; }

        public KnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
        {
            ReviewerReplacementStrategyType = knowledgeSaveReviewerReplacementType;
        }

        public static KnowledgeShareStrategy Create(ILogger logger,string knowledgeShareStrategyType, string knowledgeSaveReviewerReplacementType, int? numberOfPeriodsForCalculatingProbabilityOfStay, string pullRequestReviewerSelectionStrategy,bool? addOnlyToUnsafePullrequests)
        {
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Nothing)
            {
                return new NothingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ActualReviewers)
            {
                return new ActualKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.CommitBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.BlameBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.BlameBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.BirdBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.BirdSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ReviewBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.ReviewBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.SpreadingBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.SpreadingBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.PersistBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.PersistBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.PersistSpreadingBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.PersistSpreadingBasedSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, numberOfPeriodsForCalculatingProbabilityOfStay, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }
            else if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RandomBasedKnowledgeShare)
            {
                return new KnowledgeShareStrategies.Strategies.Spreading.RandomBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests);
            }

            throw new ArgumentException($"invalid {nameof(knowledgeShareStrategyType)}");
        }

        internal PullRequestRecommendationResult Recommend(PullRequestContext pullRequestContext)
        {
            var pullRequestRecommendationResult = RecommendReviewers(pullRequestContext);

            // we sort the names alphabetically. Helps when analyzing the results later.
            pullRequestRecommendationResult.ActualReviewers = pullRequestContext.ActualReviewers.Select(q => q.DeveloperName).OrderBy(q => q).ToArray();
            pullRequestRecommendationResult.SelectedReviewers = pullRequestRecommendationResult.SelectedReviewers?.OrderBy(q => q).ToArray();

            var selectedReviewersKnowledge = pullRequestRecommendationResult.GetSelectedReviewersKnowledge()
                .SelectMany(q => q.GetTouchedFiles())
                .ToHashSet();

            var actualReviewersKnowledge = pullRequestContext.ActualReviewers
                .SelectMany(q => q.GetTouchedFiles())
                .ToHashSet();

            var prFiles = pullRequestContext.PullRequestFiles
                .Select(q => pullRequestContext.CanononicalPathMapper.GetValueOrDefault(q.FileName));

            pullRequestRecommendationResult.LossOfExpertise = prFiles.Count(q => selectedReviewersKnowledge.Contains(q))
                - prFiles.Count(q => actualReviewersKnowledge.Contains(q));

            pullRequestRecommendationResult.PullRequestNumber = pullRequestContext.PullRequest.Number;
            pullRequestRecommendationResult.IsSimulated = true;

            CalculateMetrics(pullRequestRecommendationResult);

            return pullRequestRecommendationResult;
        }

        private void CalculateMetrics(PullRequestRecommendationResult result)
        {
            if (result.ActualReviewers.Count() == 0 || result.SortedCandidates == null || result.SortedCandidates.Count() == 0)
            {
                return;
            }

            result.TopFiveIsAccurate = result.SortedCandidates.TakeLast(5).Any(q => result.ActualReviewers.Any(a => a == q));
            result.TopTenIsAccurate = result.SortedCandidates.TakeLast(10).Any(q => result.ActualReviewers.Any(a => a == q));
            var firstCorretRecommendation = result.SortedCandidates.TakeLast(10).LastOrDefault(q => result.ActualReviewers.Any(a => a == q));

            if (firstCorretRecommendation != null)
            {
                result.MeanReciprocalRank = 1.0 / (Array.FindIndex(result.SortedCandidates.TakeLast(10).Reverse().ToArray(), q => q == firstCorretRecommendation) + 1);
            }
            else
            {
                result.MeanReciprocalRank = 0;
            }
        }

        protected abstract PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext);

        protected static bool IsCoreDeveloper(PullRequestContext pullRequestContext, string developerName)
        {
            if (pullRequestContext.SelectedReviewersType == SelectedReviewersType.All)
            {
                return true;
            }

            return pullRequestContext.Developers[developerName].GetContributionsOfPeriod(pullRequestContext.PullRequestPeriod.Id)?.TotalCommits > 20
                            || pullRequestContext.Developers[developerName].GetContributionsOfPeriod(pullRequestContext.PullRequestPeriod.Id)?.TotalReviews > 5;
        }

        protected static bool IsDevelperAmongActualReviewers(PullRequestContext pullRequestContext, string developerName)
        {
            return pullRequestContext.ActualReviewers.Any(a => a.DeveloperName == developerName);
        }

        protected static bool IsDeveloperAvailable(PullRequestContext pullRequestContext, string developerName)
        {
            return pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == developerName);
        }
    }
}
