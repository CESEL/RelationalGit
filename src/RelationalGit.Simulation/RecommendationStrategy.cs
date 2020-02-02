using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Simulation
{
    public abstract class RecommendationStrategy
    {
        protected string ReviewerReplacementStrategyType { get; private set; }

        protected bool ChangePast { get; private set; }

        public RecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger,bool changePast)
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
            // initiating values
            result.TopFiveIsAccurate = false;
            result.TopTenIsAccurate = false;
            result.MeanReciprocalRank = 0;

            if (result.ActualReviewers.Count() == 0 || result.SortedCandidates == null || result.SortedCandidates.Count() == 0)
            {
                return;
            }

            var found = false;
            for (int i = 0; i < result.SortedCandidates.Length && !found; i++)
            {
                foreach (var actualReviewer in result.ActualReviewers)
                {
                    if(result.SortedCandidates[i].DeveloperName == actualReviewer)
                    {
                        result.TopTenIsAccurate = i < 10;
                        result.TopFiveIsAccurate = i < 5;
                        result.MeanReciprocalRank = 1.0 / (i + 1);
                        found = true;
                        break;
                    }
                }
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
