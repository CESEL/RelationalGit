using System;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Calculation
{
    class Program
    {
        static void Main(string[] args)
        {
            var actualId = 47;

            using (var dbContext = GetDbContext())
            {
                var simulationsIds = dbContext.LossSimulations.Where(q => q.KnowledgeShareStrategyType == "persist-spreading" || q.KnowledgeShareStrategyType== "TurnoverRec")
                    .Where(q=>q.EndDateTime>DateTime.MinValue && q.ChangePast)
                    .Select(q => q.Id).ToArray();

                var path = @"Results\rust-TurnoverRec";

                var analyzer = new Analyzer();
                analyzer.AnalyzeSimulations(actualId, simulationsIds, path);
            }

            using (var dbContext = GetDbContext())
            {
                var simulationsIds = dbContext.LossSimulations.Where(q => q.KnowledgeShareStrategyType == "bird" || q.KnowledgeShareStrategyType == "cHRev")
                    .Where(q => q.EndDateTime > DateTime.MinValue && q.ChangePast)
                    .Select(q => q.Id).ToArray();

                var path = @"Results\roslyn-chrev";

                var analyzer = new Analyzer();
                analyzer.AnalyzeSimulations(actualId, simulationsIds, path);
            }

            using (var dbContext = GetDbContext())
            {
                var simulationsIds = dbContext.LossSimulations.Where(q => q.KnowledgeShareStrategyType == "sophia" || q.KnowledgeShareStrategyType == "Sofia")
                    .Where(q => q.EndDateTime > DateTime.MinValue && q.ChangePast)
                    .Select(q => q.Id).ToArray();

                var path = @"Results\roslyn-sophia";

                var analyzer = new Analyzer();
                analyzer.AnalyzeSimulations(actualId, simulationsIds, path);
            }
        }

        private static GitRepositoryDbContext GetDbContext()
        {
            return new GitRepositoryDbContext(autoDetectChangesEnabled: false);
        }
    }
}
