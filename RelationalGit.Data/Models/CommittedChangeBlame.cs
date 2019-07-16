using System;
using System.ComponentModel.DataAnnotations;

namespace RelationalGit.Data
{
    public class CommittedChangeBlame
    {
        public CommittedChangeBlame()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        public string CommitSha { get; set; }

        public string CanonicalPath { get; set; }

        public string Path { get;  set; }

        public string NormalizedDeveloperIdentity { get; set; }

        public DateTime AuthorDateTime { get; set; }

        public int AuditedLines { get; set; }

        public double AuditedPercentage { get; set; }

        public bool Ignore { get; set; }

        public string DeveloperIdentity { get;  set; }
    }
}
