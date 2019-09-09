namespace RelationalGit.KnowledgeShareStrategies
{
    public static class KnowledgeShareStrategyType
    {
        public static string Nothing => "NoReviews";

        public static string ActualReviewers => "Reality";

        public static string BlameBasedKnowledgeShare => "line";

        public static string CommitBasedKnowledgeShare => "AuthorshipRec";

        public static string BirdBasedKnowledgeShare => "cHRev";

        public static string RandomBasedKnowledgeShare => "random";

        public static string PersistBasedKnowledgeShare => "RetentionRec";

        public static string SpreadingBasedKnowledgeShare => "LearnRec";

        public static string PersistSpreadingBasedKnowledgeShare => "TurnoverRec";

        public static string SophiaBasedKnowledgeShare => "Sofia";

        public static string ReviewBasedKnowledgeShare => "RevOwnRec";

        public static string ContributionBasedKnowledgeShare => "contribution";
    }
}
