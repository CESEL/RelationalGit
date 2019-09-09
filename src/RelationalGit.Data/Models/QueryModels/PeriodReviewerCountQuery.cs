using System;

namespace RelationalGit.Data
{
    public class PeriodReviewerCountQuery
    {
        public long PeriodId { get; set; }

        public string GitHubUserLogin { get; set; }

        public int Count { get; set; }
    }

    public class ReviewersParticipationDateTimeQuery
    {
        public string GitHubUserLogin { get; set; }

        public DateTime FirstReviewDateTime { get; set; }

        public DateTime LastReviewDateTime { get; set; }
    }
}
