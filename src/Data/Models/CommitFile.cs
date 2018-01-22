using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class CommittedBlob
    {
        public string CommitSha { get; set; }
        public string CanonicalPath { get; set; }
        public string Path { get; set; }
        public int NumberOfLines { get; set; }

        [NotMapped]
        public ICollection<CommitBlobBlame> CommitBlobBlames { get; set; }
    }
}
