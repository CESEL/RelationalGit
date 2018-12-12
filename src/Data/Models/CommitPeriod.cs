﻿using System;
using System.ComponentModel.DataAnnotations;

namespace RelationalGit
{
    public class CommitPeriod
    {
        [MaxLength(40)]
        public string CommitSha { get; set; }
        public Guid PeriodId { get; set; }

    }
}
