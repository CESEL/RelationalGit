using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelationalGit.Data
{
    public class CommittedBlob
    {
        public long Id { get; set; }

        public string CommitSha { get; set; }

        public string CanonicalPath { get; set; }

        public string Path { get; set; }

        public int NumberOfLines { get; set; }

        [NotMapped]
        public ICollection<CommitBlobBlame> CommitBlobBlames { get; set; }
    }
}
