using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Simulation
{
    public abstract class KnowledgeShareStrategy
    {
        protected string ReviewerReplacementStrategyType { get; private set; }

        protected bool ChangePast { get; private set; }

        public KnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger,bool changePast)
        {
            ReviewerReplacementStrategyType = knowledgeSaveReviewerReplacementType;
            ChangePast = changePast;
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

            if(prFiles.Count()> 0)
                pullRequestRecommendationResult.Expertise = prFiles.Count(q => selectedReviewersKnowledge.Contains(q)) / (double)prFiles.Count();

            pullRequestRecommendationResult.PullRequestNumber = pullRequestContext.PullRequest.Number;
            pullRequestRecommendationResult.IsSimulated = true;

            CalculateMetrics(pullRequestRecommendationResult);

            if(!ChangePast)
            {
                pullRequestRecommendationResult.SelectedReviewers = pullRequestRecommendationResult.ActualReviewers;
            }

            return pullRequestRecommendationResult;
        }

        private void CalculateMetrics(PullRequestRecommendationResult result)
        {
            if (result.ActualReviewers.Count() == 0 || result.SortedCandidates == null || result.SortedCandidates.Count() == 0)
            {
                return;
            }

            var sortedCandidatesNames = result.SortedCandidates.Select(q => q.DeveloperName);

            result.TopFiveIsAccurate = sortedCandidatesNames.TakeLast(5).Any(q => result.ActualReviewers.Any(a => a == q));
            result.TopTenIsAccurate = sortedCandidatesNames.TakeLast(10).Any(q => result.ActualReviewers.Any(a => a == q));
            var firstCorretRecommendation = sortedCandidatesNames.TakeLast(10).LastOrDefault(q => result.ActualReviewers.Any(a => a == q));

            if (firstCorretRecommendation != null)
            {
                result.MeanReciprocalRank = 1.0 / (Array.FindIndex(sortedCandidatesNames.TakeLast(10).Reverse().ToArray(), q => q == firstCorretRecommendation) + 1);
            }
            else
            {
                result.MeanReciprocalRank = 0;
            }
        }

        protected abstract PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext);

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
