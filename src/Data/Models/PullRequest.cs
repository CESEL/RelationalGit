using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class PullRequest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public long Id { get; set; }
        public string UserLogin { get; set; }
        public DateTime? CreatedAtDateTime { get; set; }
        public DateTime? ClosedAtDateTime { get; set; }
        public DateTime? MergedAtDateTime { get; set; }
        public string BaseSha { get; set; }
        public long IssueId { get; set; }
        public string IssueUrl { get; set; }
        public string HtmlUrl { get; set; }
        public bool Merged { get; set; }
        public int Number { get; set; }
        public string MergeCommitSha { get; internal set; }
    }
}
