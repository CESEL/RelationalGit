using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationalGit
{
    public class SimulatedLeaver
    {
        public long Id { get; set; }
        public long LossSimulationId { get; set; }   
        public long PeriodId { get; set; }
        public string NormalizedName { get; internal set; }    }
}
