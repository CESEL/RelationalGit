using System;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RelationalGit.KnowledgeShareStrategies.Models;

namespace RelationalGit
{
    public abstract class BaseKnowledgeShareStrategy : KnowledgeShareStrategy
    {
        public PullRequestContext PullRequestContext { get; private set; }

        public DeveloperKnowledge[] SortedCandidates { get; set; }

        public DeveloperKnowledge[] SortedActualReviewers { get; set; }

        public BaseKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger)
            : base(knowledgeSaveReviewerReplacementType, logger)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            PullRequestContext = pullRequestContext;

            if (pullRequestContext.ActualReviewers.Count() == 0)
            {
                return new PullRequestRecommendationResult(Array.Empty<string>(), null);
            };

            var candidates = GetCandidates(pullRequestContext);
            SortedCandidates = SortCandidates(PullRequestContext, candidates);
            SortedActualReviewers = SortActualReviewers(PullRequestContext, PullRequestContext.ActualReviewers);

            DeveloperKnowledge[] recommendedReviewers = null;

            if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.OneOfActuals)
            {
                var leastKnowledgedReviewer = WhoHasTheLeastKnowledge();
                var mostKnowledgedReviewer = WhoHasTheMostKnowlege();
                recommendedReviewers = RepleaceLeastWithMostKnowledged(pullRequestContext, leastKnowledgedReviewer, mostKnowledgedReviewer);
            }
            else if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.AllOfActuals)
            {
                var countOfPossibleRecommendation = Math.Min(SortedActualReviewers.Length, SortedCandidates.Length);
                recommendedReviewers = SortedCandidates.TakeLast(SortedActualReviewers.Length).ToArray();
            }
            else if (ReviewerReplacementStrategyType == RelationalGit.ReviewerReplacementStrategyType.AddNewReviewerToActuals)
            {
                // we do not take the dev with most expertise to avoid knowledge concentration
                var addedReviewer = SortedCandidates.LastOrDefault(q => SortedActualReviewers.All(sar => sar.DeveloperName != q.DeveloperName));

                if (addedReviewer == null)
                {
                    recommendedReviewers = SortedActualReviewers;
                }
                else
                {
                    recommendedReviewers = SortedActualReviewers.Concat(new DeveloperKnowledge[] { addedReviewer }).ToArray();
                }
            }

            return new PullRequestRecommendationResult(recommendedReviewers.Select(q => q.DeveloperName).ToArray(), SortedCandidates.Select(q => q.DeveloperName).ToArray());
        }

        protected DeveloperKnowledge[] RepleaceLeastWithMostKnowledged(PullRequestContext pullRequestContext, DeveloperKnowledge leastKnowledgedReviewer, DeveloperKnowledge mostKnowledgedReviewer)
        {
            var actualReviewers = PullRequestContext.ActualReviewers;

            if (mostKnowledgedReviewer == null)
            {
                return actualReviewers;
            }

            var index = Array.IndexOf(actualReviewers, leastKnowledgedReviewer);
            actualReviewers[index] = mostKnowledgedReviewer;
            return actualReviewers;
        }

        protected DeveloperKnowledge WhoHasTheMostKnowlege()
        {
            var actualReviewers = PullRequestContext.ActualReviewers;

            for (var i = SortedCandidates.Length - 1; i >= 0; i--)
            {
                var isReviewer = actualReviewers.Any(q => q.DeveloperName == SortedCandidates[i].DeveloperName);
                var isPrSubmitter = PullRequestContext.PullRequestKnowledgeables[i].DeveloperName == PullRequestContext.PRSubmitterNormalizedName;
                var isAvailable = PullRequestContext.AvailableDevelopers.Any(q => q.NormalizedName == PullRequestContext.PullRequestKnowledgeables[i].DeveloperName);

                if (!isReviewer && !isPrSubmitter && isAvailable)
                {
                    return PullRequestContext.PullRequestKnowledgeables[i];
                }
            }

            return null;
        }

        protected DeveloperKnowledge WhoHasTheLeastKnowledge()
        {
            return SortedActualReviewers[0];
        }

        protected DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext)
        {
            return pullRequestContext.PullRequestKnowledgeables
                .Where(q => q.DeveloperName != pullRequestContext.PRSubmitterNormalizedName
                && IsDeveloperAvailable(pullRequestContext, q.DeveloperName)
                && IsCoreDeveloper(pullRequestContext, q.DeveloperName)) // we want to suggest people who have a great chance to stay in the project
            .ToArray();
        }

        protected abstract DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates);

        protected virtual DeveloperKnowledge[] SortActualReviewers(PullRequestContext pullRequestContext, DeveloperKnowledge[] actualReviewers)
        {
            return SortCandidates(pullRequestContext, actualReviewers);
        }

        protected virtual DeveloperKnowledge[] GetCandidates(PullRequestContext pullRequestContext)
        {
            return AvailablePRKnowledgeables(pullRequestContext);
        }
    }
}
