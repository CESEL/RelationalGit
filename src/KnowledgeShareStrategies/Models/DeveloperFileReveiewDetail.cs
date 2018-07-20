using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{

    public class DeveloperFileReveiewDetail
    {
        public string FilePath { get; set; }
        public Developer Developer { get; set; }
        public List<Period> Periods { get; set; } = new List<Period>();
        public List<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
    }
}

