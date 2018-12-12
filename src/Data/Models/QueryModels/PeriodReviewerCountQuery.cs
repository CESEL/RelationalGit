namespace RelationalGit
{
    public class PeriodReviewerCountQuery
    {
        public long PeriodId { get; set; }
        public string GitHubUserLogin { get; set; }
        public int Count { get; set; }
    }
}
