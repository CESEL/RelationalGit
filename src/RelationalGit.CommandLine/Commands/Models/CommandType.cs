namespace RelationalGit.Commands
{
    public static class CommandType
    {
        public static string GetGitCommits => "get-git-commits";

        public static string GetGitCommitsChanges => "get-git-commits-changes";

        public static string GetPullRequests => "get-github-pullrequests";

        public static string GetPullRequestReviewes => "get-github-pullrequest-reviewers";

        public static string GetPullRequestRevieweComments => "get-github-pullrequest-reviewer-comments";

        public static string GetPullRequestFiles => "get-github-pullrequests-files";

        public static string Periodize => "periodize-git-commits";

        public static string ExtractBlameFromCommit => "get-git-commit-blames";

        public static string ExtractBlameForEachPeriod => "get-git-commit-blames-for-periods";

        public static string DoNameAliasing => "alias-git-names";

        public static string ApplyNameAliasing => "apply-git-aliased";

        public static string ExtractDeveloperInformation => "extract-dev-info";

        public static string IgnoreMegaCommitsAndDevelopers => "ignore-mega-commits";

        public static string MapGitHubGitNames => "map-git-github-names";

        public static string ComputeKnowledgeLoss => "compute-loss";

        public static string GetPullRequestMergeEvents => "get-merge-events";

        public static string GetPullRequestIssueComments => "get-pullrequest-issue-comments";

        public static string GetBlamesOfCommitedChanges => "get-committed-changes-blames";
    }
}
