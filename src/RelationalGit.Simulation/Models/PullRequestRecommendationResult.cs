using System.Collections.Generic;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class PullRequestRecommendationResult
    {
        private DeveloperKnowledge[] _selectedReviewersKnowledge;

        public PullRequestRecommendationResult(DeveloperKnowledge[] selectedReviewers, DeveloperKnowledge[] sortedCandidates,bool? isRisky,string features)
        {
            SortedCandidates = sortedCandidates;
            SelectedReviewers = selectedReviewers.Select(q => q.DeveloperName).ToArray();
            _selectedReviewersKnowledge = selectedReviewers;
            IsRisky = isRisky;
            Features = features;
        }

        public long PullRequestNumber { get; set; }

        public double Expertise { get; set; }

        public string[] ActualReviewers { get; set; }

        public string[] SelectedReviewers { get; set; }

        public DeveloperKnowledge[] SortedCandidates { get; set; }

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

        public bool? TopFiveIsAccurate { get;  set; }

        public bool? TopTenIsAccurate { get;  set; }

        public double? MeanReciprocalRank { get;  set; }

        public bool IsSimulated { get; set; }

        public string Features { get; set; }

        public bool? IsRisky { get; set; }

        public DeveloperKnowledge[] GetSelectedReviewersKnowledge()
        {
            return _selectedReviewersKnowledge;
        }
    }
}
