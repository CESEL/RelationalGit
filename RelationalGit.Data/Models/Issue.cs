using System;

namespace RelationalGit.Data
{
    public class Issue
    {
        public long Id { get; set; }

        public string UserLogin { get; set; }

        public DateTime? CreatedAtDateTime { get; set; }

        public DateTime? ClosedAtDateTime { get; set; }

        public string Url { get; set; }

        public string HtmlUrl { get; set; }

        public string State { get; set; }

        public int Number { get; set; }

        public string Title { get; internal set; }

        public string Body { get; internal set; }

        public string PullRequestUrl { get; internal set; }

        public string PullRequestNumber { get; internal set; }

        public string Label { get; internal set; }
    }
}
