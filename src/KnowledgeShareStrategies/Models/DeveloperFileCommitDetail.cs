using LibGit2Sharp;
using System.Collections.Generic;

namespace RelationalGit
{
    public class DeveloperFileCommitDetail
    {
        public string FilePath { get; set; }
        public Developer Developer { get; set; }
        public List<Period> Periods { get; set; } = new List<Period>();
        public List<Commit> Commits { get; set; } = new List<Commit>();
        public List<CommitDetail> CommitDetails { get; set; } = new List<CommitDetail>();
    }

    public class CommitDetail
    {
        public CommitDetail(Commit commit,Period period, ChangeKind changeKind)
        {
            Commit = commit;
            Period = period;
            ChangeKind = changeKind;
        }

        public Commit Commit { get; private set; }

        public Period Period { get; private set; }

        public ChangeKind ChangeKind { get; private set; }
    }
}

