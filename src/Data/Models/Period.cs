using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit
{
    public class Period
    {
        public long Id { get; set; }

        public DateTime FromDateTime { get; set; }

        public DateTime ToDateTime { get; set; }

        public string FirstCommit { get; set; }

        public string LastCommitSha { get; set; }
    }
}
