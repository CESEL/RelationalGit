using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class KnowledgeDistributionMap
    {
        public Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>> CommitBasedKnowledgeMap;
        public Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> ReviewBasedKnowledgeMap;
        public Dictionary<long, List<string>> PullRequestReviewers { get;  set; }
        public Dictionary<long, Dictionary<string, Dictionary<string, FileBlame>>> BlameDistribution { get; internal set; }
    }
}

