using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationalGit
{
    public class CommitPeriod
    {
        [MaxLength(40)]
        public string CommitSha { get; set; }
        public Guid PeriodId { get; set; }

    }
}
