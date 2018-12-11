
using RelationalGit.KnowledgeShareStrategies.Models;
using RelationalGit.KnowledgeShareStrategies.Strategies.Spreading;
using System;
using System.Linq;

namespace RelationalGit
{
    public abstract class KnowledgeShareStrategy
    {
        protected string ReviewerReplacementStrategyType { get; private set; }

        public KnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
        {
            ReviewerReplacementStrategyType = knowledgeSaveReviewerReplacementType;
        }

        public static KnowledgeShareStrategy Create(string knowledgeShareStrategyType, string knowledgeSaveReviewerReplacementType)
        {
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Nothing)
            {
                return new NothingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ActualReviewers)
            {
                return new ActualKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Ideal)
            {
                return new IdealKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RealisticIdeal)
            {
                return new RealisticIdealKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedExpertiseReviewers)
            {
                return new CommitBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }

            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.CommitBasedSpreadingReviewers)
            {
                return new SpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.SpreadingKnowledge2)
            {
                return new FileLevelSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.FileLevelSpreadingReplaceAll)
            {
                return new FileLevelSpreadingKnowledgeReplaceAllShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.BlameBasedSpreadingReviewers)
            {
                return new BlameBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.ReviewBasedSpreadingReviewers)
            {
                return new ReviewBasedKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RendomReviewers)
            {
                return new RandomKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RealisticRandomSpreading)
            {
                return new RealisticRandomSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.RandomSpreading)
            {
                return new RandomSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.FolderLevelSpreading)
            {
                return new FolderLevelSpreadingKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.FolderLevelSpreadingPlusOne)
            {
                return new FolderLevelSpreadingPlusOneKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.LeastTouchedFiles)
            {
                return new LeastTouchedFilesKnowlegdeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.MostTouchedFiles)
            {
                return new MostTouchedFilesKnowlegdeShareStrategy(knowledgeSaveReviewerReplacementType);
            }
            if (knowledgeShareStrategyType == KnowledgeShareStrategyType.Bird)
            {
                return new BirdKnowledgeShareStrategy(knowledgeSaveReviewerReplacementType);
            }


            throw new ArgumentException($"invalid {nameof(knowledgeShareStrategyType)}");
        }

        internal PullRequestRecommendationResult Recommend(PullRequestContext pullRequestContext)
        {
            var pullRequestRecommendationResult = RecommendReviewers(pullRequestContext);

            pullRequestRecommendationResult.ActualReviewers = pullRequestContext.ActualReviewers.Select(q=>q.DeveloperName).ToArray();
            pullRequestRecommendationResult.PullRequestNumber = pullRequestContext.PullRequest.Number;
            pullRequestRecommendationResult.IsSimulated = true;

            CalculateMetrics(pullRequestRecommendationResult);

            return pullRequestRecommendationResult;

        }

        private void CalculateMetrics(PullRequestRecommendationResult result)
        {
            if (result.ActualReviewers.Count()==0 || result.SortedCandidates == null || result.SortedCandidates.Count() == 0)
                return;

            result.TopFiveIsAccurate = result.SortedCandidates.TakeLast(5).Any(q=> result.ActualReviewers.Any(a=>a==q));
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
                return true;

            return pullRequestContext.Developers[developerName].GetContributionsOfPeriod(pullRequestContext.Period.Id)?.TotalCommits > 20
                            || pullRequestContext.Developers[developerName].GetContributionsOfPeriod(pullRequestContext.Period.Id)?.TotalReviews > 5;
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
