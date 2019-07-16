namespace RelationalGit.Data
{
    public class CommitBlobBlame
    {
        public long Id { get; set; }

        public string CommitSha { get; set; }

        public string AuthorCommitSha { get; set; }

        public string CanonicalPath { get; set; }

        public string DeveloperIdentity { get; set; }

        public int AuditedLines { get; set; }

        public double AuditedPercentage { get; set; }

        public string Path { get; set; }

        public bool Ignore { get; set; }

        public string NormalizedDeveloperIdentity { get; set; }
    }
}
