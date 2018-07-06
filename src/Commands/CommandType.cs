using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
