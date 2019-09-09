using System.Collections.Generic;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class DeveloperFileReveiewDetail
    {
        public string FilePath { get; set; }

        public Developer Developer { get; set; }

        public List<Period> Periods { get; set; } = new List<Period>();

        public List<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
    }
}
