﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationalGit
{
    public class SimulatedAbondonedFile
    {
        public long Id { get; set; }
        public string FilePath { get; set; }
        public long LossSimulationId { get; set; }   
        public long PeriodId { get; set; }
        public int TotalLinesInPeriod { get; internal set; }
        public int AbandonedLinesInPeriod { get; internal set; }
        public int SavedLinesInPeriod { get; internal set; }
    }
}
