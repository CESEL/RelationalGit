namespace RelationalGit
{
    public static class KnowledgeShareStrategyType
    {
        public static string Nothing => "nothing";

        public static string ActualReviewers => "reviewers-actual";

        public static string BlameBasedExpertiseReviewers => "reviewers-expertise-blame";

        public static string FileBasedExpertiseReviewers => "reviewers-expertise-file";

        public static string CommitBasedExpertiseReviewers => "reviewers-expertise-commit";

        public static string RendomReviewers => "reviewers-random";

        public static string RandomSpreading => "random-spreading";

        public static string KnowledgeSharingReviewers => "reviewers-knowledge-sharing";

        public static string Expertise => "reviewers-expertise";

        public static string Ideal => "reviewers-ideal";

        public static string RealisticIdeal => "reviewers-realistic-ideal";

        public static string BlameBasedSpreadingReviewers => "reviewers-expertise-blame";

        public static string ReviewBasedSpreadingReviewers => "reviewers-expertise-review";

        public static string FileLevelSpreading => "file-level-spreading-knowledge";

        public static string FileLevelSpreadingReplaceAll => "file-level-spreading-knowledge-replace-all";

        public static string FolderLevelSpreading => "folder-level-spreading-knowledge";

        public static string MostTouchedFiles => "most-touched-files";

        public static string LeastTouchedFiles => "least-touched-files";

        public static string FolderLevelSpreadingPlusOne => "folder-level-spreading-knowledge-plus-one";

        public static string Bird => "bird";

        public static string FolderLevelProbabilityBasedSpreading => "folder-level-probability-based-spreading-knowledge";

        public static string FileLevelProbabilityBasedSpreading => "file-level-probability-based-spreading-knowledge";
    }
}
