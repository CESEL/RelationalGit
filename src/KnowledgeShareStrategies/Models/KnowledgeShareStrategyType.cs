namespace RelationalGit
{
    public static class KnowledgeShareStrategyType
    {
        public static string Nothing => "nothing";

        public static string ActualReviewers => "reviewers-actual";

        public static string BlameBasedKnowledgeShare => "line";

        public static string CommitBasedKnowledgeShare => "commit";

        public static string BirdBasedKnowledgeShare => "bird";

        public static string RandomBasedKnowledgeShare => "random";

        public static string PersistBasedKnowledgeShare => "persist";

        public static string SpreadingBasedKnowledgeShare => "spreading";

        public static string PersistSpreadingBasedKnowledgeShare => "persist-spreading";

        public static string ReviewBasedKnowledgeShare => "review";
    }
}
