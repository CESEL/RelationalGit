﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationalGit
{
    public class LossSimulation
    {
        public long Id { get; set; }
        public string KnowledgeShareStrategyType { get; set; }
        public int MegaPullRequestSize { get; set; }
        public double FileAbondonedThreshold { get; set; }
        public DateTime StartDateTime { get; set; }
        public string LeaversType { get; set; }
        public DateTime EndDateTime { get; internal set; }
    }
}
