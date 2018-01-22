using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationalGit
{
    public class PullRequestFile
    {
        [Key]
        public Guid Id { get; set; }
        public string Sha { get; set; }
        public string Status { get; set; }
        public string FileName { get; set; }
        public int? Additions { get; set; }
        public int? Deletions { get; set; }
        public int? Changes { get; set; }
        public int PullRequestNumber { get; set; }
    }
}


