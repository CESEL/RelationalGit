using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class Commit
    {
        [Key]
        public string Sha { get; set; }
        public string AuthorEmail { get; set; }
        public string CommitterEmail { get; set; }
        public DateTime AuthorDateTime { get; set; }
        public DateTime CommitterDateTime { get; set; }
        public string MessageShort  { get; set; }
        public string Message  { get; set; }
        public string TreeSha { get; set; }
        public bool IsMergeCommit { get; set; }

        [NotMapped]
        public ICollection<CommitRelationship> CommitRelationship { get; set; }

        [NotMapped]
        public LibGit2Sharp.Commit GitCommit { get; set; }

        [NotMapped]
        public ICollection<CommittedChange> CommittedChanges { get; set; }

        [NotMapped]
        public ICollection<CommittedBlob> Blobs { get; set; }
    }
}
