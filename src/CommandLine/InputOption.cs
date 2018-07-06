using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit.CommandLine
{
    internal class InputOption
    {
        [Option("cmd")]
        public string Command { get; set; }

        [Option("repo-path")]
        public string RepositoryPath { get; set; }

        [Option("github-token")]
        public string GitHubToken { get; set; }

        [Option("git-branch")]
        public string GitBranch { get; set; }

        [Option("github-owner")]
        public string GitHubOwner { get; set; }

        [Option("github-repo")]
        public string GitHubRepo { get; set; }

        [Option("sha")]
        public string CommitSha { get; set; }

        [Option("extensions")]
        public IEnumerable<string> Extensions { get; set; }

        [Option("issue-states")]
        public IEnumerable<string> IssueLabels { get; set; }
        public string IssueState { get; set; }

        [Option("p-type")]
        public string PeriodType { get; set; }

        [Option("p-length")]
        public int PeriodLength { get; set; }
    }
}
