using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public static class KnowledgeShareStrategyType
    {
        public static string Nothing=>"nothing";
        public static string ActualReviewers=>"reviewers-actual";
        public static string BlameBasedExpertiseReviewers =>"reviewers-expertise-blame";
        public static string FileBasedExpertiseReviewers=>"reviewers-expertise-file";
        public static string CommitBasedExpertiseReviewers=>"reviewers-expertise-commit";
        public static string RendomReviewers=>"reviewers-random";
        public static string KnowledgeSharingReviewers=>"reviewers-knowledge-sharing";
        public static string Expertise => "reviewers-expertise";
        public static string Ideal =>"reviewers-ideal";

        public static string CommitBasedSpreadingReviewers => "reviewers-spreading";
    }
}
