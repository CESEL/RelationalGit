namespace RelationalGit.Data
{
    public class FileKnowledgeable
    {
        public long Id { get; set; }

        public long PeriodId { get;   set; }

        public string CanonicalPath { get;   set; }

        public int TotalKnowledgeables { get;   set; }

        public long LossSimulationId { get;   set; }

        public string Knowledgeables { get;   set; }

        public int TotalAvailableCommitters { get;   set; }

        public int TotalAvailableReviewers { get;   set; }

        public int TotalAvailableReviewOnly { get;   set; }

        public int TotalAvailableCommitOnly { get;   set; }

        public string AvailableCommitters { get;   set; }

        public string AvailableReviewers { get;   set; }

        public bool HasReviewed { get; set; }

        public int TotalCommitters { get;   set; }

        public int TotalReviewers { get;   set; }

        public int TotalPullRequests { get;   set; }
    }
}
