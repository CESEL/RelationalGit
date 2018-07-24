using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class ExtractBlameForEachPeriodOption
    {
        public string RepositoryPath { get; internal set; }
        public string GitBranch { get; set; }
        public string[] Extensions { get; set; }
        public int[] PeriodIds { get; set; }
        public string[] ExcludeBlamePath { get; internal set; }
    }
}

