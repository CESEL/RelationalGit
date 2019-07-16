namespace RelationalGit.Data
{
    public class RecommendedPullRequestCandidate
    {
        private RecommendedPullRequestCandidate()
        {
        }

        public RecommendedPullRequestCandidate(long lossSimulationId,int rank, string normalizedReviewerName, double score, long pullRequestNumber)
        {
            LossSimulationId = lossSimulationId;
            Rank = rank;
            PullRequestNumber = pullRequestNumber;
            NormalizedReviewerName = normalizedReviewerName;
            Score = score;
        }

        public long Id { get; set; }

        public long PullRequestNumber { get; internal set; }

        public string NormalizedReviewerName { get; internal set; }

        public long LossSimulationId { get; internal set; }

        public int Rank { get; set; }

        public double Score { get; set; }
    }
}
