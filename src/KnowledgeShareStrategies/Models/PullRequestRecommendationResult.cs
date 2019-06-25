using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.KnowledgeShareStrategies.Models
{
    public class PullRequestRecommendationResult
    {
        private DeveloperKnowledge[] _selectedReviewersKnowledge;

        public PullRequestRecommendationResult(DeveloperKnowledge[] selectedReviewers, DeveloperKnowledge[] sortedCandidates = null)
        {
            SortedCandidates = sortedCandidates?.Select(q => q.DeveloperName).ToArray();
            SelectedReviewers = selectedReviewers.Select(q => q.DeveloperName).ToArray();
            _selectedReviewersKnowledge = selectedReviewers;
        }

        public long PullRequestNumber { get; internal set; }

        public double LossOfExpertise { get; internal set; }

        public string[] ActualReviewers { get; internal set; }

        public string[] SelectedReviewers { get; internal set; }

        public string[] SortedCandidates { get; internal set; }

        public IEnumerable<RecommendedPullRequestReviewer> RecommendedPullRequestReviewers
        {
            get
            {
                foreach (var recommendedReviewer in SelectedReviewers)
                {
                    var reviewerType = ActualReviewers.Any(q => q == recommendedReviewer)
                        ? RecommendedPullRequestReviewerType.Actual : RecommendedPullRequestReviewerType.Recommended;

                    yield return new RecommendedPullRequestReviewer(PullRequestNumber, recommendedReviewer, reviewerType);
                }
            }
        }

        public bool? TopFiveIsAccurate { get; internal set; }

        public bool? TopTenIsAccurate { get; internal set; }

        public double? MeanReciprocalRank { get; internal set; }

        public bool IsSimulated { get; internal set; }

        public DeveloperKnowledge[] GetSelectedReviewersKnowledge()
        {
            return _selectedReviewersKnowledge;
        }
    }
}
