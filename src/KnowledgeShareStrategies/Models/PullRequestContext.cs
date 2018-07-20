using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RelationalGit
{
    public class PullRequestContext
    {
        public string PRSubmitterNormalizedName { get; set; }
        internal Developer[] availableDevelopers;
        public string[] ActualReviewers { get; internal set; }
        public PullRequestFile[] PullRequestFiles { get; internal set; }
        public PullRequest PullRequest { get; internal set; }
        public KnowledgeDistributionMap KnowledgeMap { get; internal set; }
        public Dictionary<string, string> CanononicalPathMapper { get; internal set; }
        public Period Period { get; internal set; }
        public ReadOnlyDictionary<string, Developer> Developers { get; internal set; }
        public Dictionary<string, Dictionary<string, FileBlame>> Blames { get; internal set; }
    }
}

