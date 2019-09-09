using LibGit2Sharp;
using System;
using System.ComponentModel.DataAnnotations;

namespace RelationalGit.Data
{
    public class PullRequestFile
    {
        public PullRequestFile()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        public string Sha { get; set; }

        public string Status { get; set; }

        public string FileName { get; set; }

        public int? Additions { get; set; }

        public int? Deletions { get; set; }

        public int? Changes { get; set; }

        public int PullRequestNumber { get; set; }

        public ChangeKind ChangeKind
        {
            get
            {
                if (Status == "added")
                {
                    return ChangeKind.Added;
                }
                else if (Status == "modified")
                {
                    return ChangeKind.Modified;
                }
                else if (Status == "removed")
                {
                    return ChangeKind.Deleted;
                }
                else
                {
                    return ChangeKind.Renamed;
                }
            }
        }
    }
}
