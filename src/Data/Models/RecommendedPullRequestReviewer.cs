namespace RelationalGit
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

        public long PullRequestNumber { get; internal set; }

        public string NormalizedReviewerName { get; internal set; }

        public long LossSimulationId { get; internal set; }

        public RecommendedPullRequestReviewerType Type { get; set; }
    }

    public enum RecommendedPullRequestReviewerType
    {
        Actual,
        Recommended
    }

}
