namespace RelationalGit.Data.Models
{
    public class PullRequestRecommendationResult
    {
        public long Id { get; set; }

        public long PullRequestNumber { get; internal set; }

        public string ActualReviewers { get; internal set; }

        public int ActualReviewersLength { get; internal set; }

        public string SelectedReviewers { get; internal set; }

        public int? SelectedReviewersLength { get; internal set; }

        public string SortedCandidates { get; internal set; }

        public int? SortedCandidatesLength { get; internal set; }

        public bool? TopFiveIsAccurate { get; internal set; }

        public bool? TopTenIsAccurate { get; internal set; }

        public double? MeanReciprocalRank { get; internal set; }

        public double LossOfExpertise { get; set; }

        public long LossSimulationId { get; internal set; }

    }
}
