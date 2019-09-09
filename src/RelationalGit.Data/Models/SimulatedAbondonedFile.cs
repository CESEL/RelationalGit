namespace RelationalGit.Data
{
    public class SimulatedAbondonedFile
    {
        public long Id { get; set; }

        public string FilePath { get; set; }

        public long LossSimulationId { get; set; }

        public long PeriodId { get; set; }

        public int TotalLinesInPeriod { get;  set; }

        public int AbandonedLinesInPeriod { get;  set; }

        public int SavedLinesInPeriod { get;  set; }

        public string RiskType { get; set; }
    }
}
