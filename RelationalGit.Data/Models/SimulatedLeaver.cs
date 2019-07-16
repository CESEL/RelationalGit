using System.ComponentModel.DataAnnotations.Schema;

namespace RelationalGit.Data
{
    public class SimulatedLeaver
    {
        public long Id { get; set; }

        public long LossSimulationId { get; set; }

        public long PeriodId { get; set; }

        public string NormalizedName { get; set; }

        public string LeavingType { get; set; }

        [NotMapped]
        public Developer Developer { get; set; }
    }
}
