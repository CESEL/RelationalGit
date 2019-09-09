namespace RelationalGit.Data
{
    public class PullRequestRecommendationResult
    {
        public long Id { get; set; }

        public long PullRequestNumber { get;   set; }

        public string ActualReviewers { get;   set; }

        public int ActualReviewersLength { get;   set; }

        public string SelectedReviewers { get;   set; }

        public int? SelectedReviewersLength { get;   set; }

        public string SortedCandidates { get;   set; }

        public int? SortedCandidatesLength { get;   set; }

        public bool? TopFiveIsAccurate { get;   set; }

        public bool? TopTenIsAccurate { get;   set; }

        public double? MeanReciprocalRank { get;   set; }

        public double Expertise { get; set; }

        public long LossSimulationId { get;   set; }
        public bool IsSimulated { get;   set; }
        public bool? IsRisky { get; set; }
        public string Features { get; set; }
    }
}
