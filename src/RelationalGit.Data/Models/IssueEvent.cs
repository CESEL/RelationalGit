using System;

namespace RelationalGit.Data
{
    public class IssueEvent
    {
        public long Id { get; set; }

        public string ActorLogin { get; set; }

        public string Url { get; set; }

        public string Event { get; set; }

        public string CommitId { get; set; }

        public DateTime CreatedAtDateTime { get; set; }

        public int IssueNumber { get; set; }
    }
}
