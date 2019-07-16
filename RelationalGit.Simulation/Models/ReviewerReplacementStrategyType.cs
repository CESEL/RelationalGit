namespace RelationalGit.Simulation
{
    public static class ReviewerReplacementStrategyType
    {
        public static string OneOfActuals => "one-of-actuals";

        public static string AllOfActuals => "all-of-actuals";

        public static string AddNewReviewerToActuals => "add-one-reviewer-to-actuals";
    }
}
