using System.ComponentModel.DataAnnotations.Schema;

namespace RelationalGit
{
    public class SimulatedLeaver
    {
        public long Id { get; set; }

        public long LossSimulationId { get; set; }

        public long PeriodId { get; set; }

        public string NormalizedName { get; internal set; }

        public string LeavingType { get; set; }

        [NotMapped]
        public Developer Developer { get; set; }
    }
}
