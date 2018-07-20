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

        [Option("period-type")]
        public string PeriodType { get; set; }

        [Option("period-length")]
        public int PeriodLength { get; set; }

        [Option("core-dev-threshold")]
        public double CoreDeveloperThreshold { get; set; }

        [Option("mega-commit-size")]
        public int MegaCommitSize { get; set; }

        [Option("save-strategy")]
        public string KnowledgeSaveStrategyType { get; set; }
        
        [Option("mega-pr-size")]
        public int MegaPullRequestSize { get; set; }

        [Option("leavers-type")]
        public string LeaversType { get; set; }
        
        [Option("core-dev-calculatuon-type")]
        public string CoreDeveloperCalculationType { get; set; }
        

        [Option("file-risk-ownership-threshold")]
        public double FilesAtRiksOwnershipThreshold { get; set; }
        
        [Option("file-risk-owners-threshold")]
        public int FilesAtRiksOwnersThreshold { get; set; }
        
        [Option("leavers-extention")]
        public int LeaversOfPeriodExtendedAbsence { get; internal set; }
        
        [Option("mega-devs")]
        public IEnumerable<string> MegaDevelopers { get; set; }
    }
}
