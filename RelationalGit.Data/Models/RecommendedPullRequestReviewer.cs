namespace RelationalGit.Data
{
    public class RecommendedPullRequestReviewer
    {
        public RecommendedPullRequestReviewer()
        {
        }

        public RecommendedPullRequestReviewer(long pullRequestNumber, string normalizedReviewerName, RecommendedPullRequestReviewerType type)
        {
            PullRequestNumber = pullRequestNumber;
            NormalizedReviewerName = normalizedReviewerName;
            
            Type = type;
        }

        public long Id { get; set; }

        public long PullRequestNumber { get; set; }

        public string NormalizedReviewerName { get; set; }

        public long LossSimulationId { get; set; }

        public RecommendedPullRequestReviewerType Type { get; set; }
    }

    public enum RecommendedPullRequestReviewerType
    {
        Actual,
        Recommended
    }
}
