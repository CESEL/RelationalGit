using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RelationalGit
{
    public class PullRequestContext
    {
        public string PRSubmitterNormalizedName { get; set; }
        public string SelectedReviewersType { get; set; }
        internal Developer[] AvailableDevelopers;
        public DeveloperKnowledge[] ActualReviewers { get; internal set; }
        public PullRequestFile[] PullRequestFiles { get; internal set; }
        public PullRequest PullRequest { get; internal set; }
        public KnowledgeDistributionMap KnowledgeMap { get; internal set; }
        public Dictionary<string, string> CanononicalPathMapper { get; internal set; }
        public Period Period { get; internal set; }
        public ReadOnlyDictionary<string, Developer> Developers { get; internal set; }
        public BlameSnapshot Blames { get; internal set; }
        public DeveloperKnowledge[] PRKnowledgeables { get; internal set; }
    }
}

