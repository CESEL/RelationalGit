using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RelationalGit
{
    public class PullRequestReviewer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string UserLogin { get; set; }
        public string CommitId { get; internal set; }
        public string State { get; internal set; }

        public long PullRequestNumber { get; set; }
    }
}
