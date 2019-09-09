namespace RelationalGit.Data
{
    public class FileTouch
    {
        public long Id { get; set; }

        public string NormalizeDeveloperName { get; set; }

        public long PeriodId { get; set; }

        public string CanonicalPath { get; set; }

        public string TouchType { get; set; }

        public long LossSimulationId { get; set; }
    }
}
