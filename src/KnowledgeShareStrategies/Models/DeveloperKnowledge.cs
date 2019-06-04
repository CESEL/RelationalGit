namespace RelationalGit
{
    public class DeveloperKnowledge
    {
        public int NumberOfTouchedFiles { get; set; }

        public int NumberOfReviewedFiles { get; set; }

        public int NumberOfCommittedFiles { get; set; }

        public int NumberOfCommits { get; set; }

        public int NumberOfReviews { get; set; }

        public string DeveloperName { get; set; }

        public int NumberOfAuthoredLines { get; set; }

        /// <summary>
        /// Use this field when you want assign score to developers
        /// </summary>
        public double Score { get; set; }

        public bool IsFolderLevel { get; set; }
    }
}
