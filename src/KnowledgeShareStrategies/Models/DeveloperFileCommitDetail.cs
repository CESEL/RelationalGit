using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class DeveloperFileCommitDetail
    {
        public string FilePath { get; set; }
        public Developer Developer { get; set; }
        public List<Period> Periods { get; set; } = new List<Period>();
        public List<Commit> Commits { get; set; } = new List<Commit>();
    }
}

