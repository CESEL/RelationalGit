namespace RelationalGit
{
    public class FileKnowledgeable
    {
        public long Id { get; set; }

        public long PeriodId { get; internal set; }

        public string CanonicalPath { get; internal set; }

        public int TotalKnowledgeables { get; internal set; }

        public long LossSimulationId { get; internal set; }

        public string Knowledgeables { get; internal set; }

        public int TotalAvailableCommitters { get; internal set; }

        public int TotalAvailableReviewers { get; internal set; }

        public int TotalAvailableReviewOnly { get; internal set; }

        public int TotalAvailableCommitOnly { get; internal set; }

        public string AvailableCommitters { get; internal set; }

        public string AvailableReviewers { get; internal set; }

        public bool HasReviewed { get; set; }

        public int TotalCommitters { get; internal set; }

        public int TotalReviewers { get; internal set; }

        public int TotalPullRequests { get; internal set; }
    }
}
