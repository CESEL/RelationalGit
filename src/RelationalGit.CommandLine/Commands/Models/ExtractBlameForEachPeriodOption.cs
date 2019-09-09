namespace RelationalGit
{
    public class ExtractBlameForEachPeriodOption
    {
        public string RepositoryPath { get; internal set; }

        public string GitBranch { get; set; }

        public string[] Extensions { get; set; }

        public int[] PeriodIds { get; set; }

        public string[] ExcludedBlamePaths { get; internal set; }

        public bool ExtractBlames { get; internal set; }
    }
}
