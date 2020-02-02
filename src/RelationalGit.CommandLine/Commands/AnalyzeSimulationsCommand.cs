using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using RelationalGit.Calculation;

namespace RelationalGit.Commands
{
    public class AnalyzeSimulationsCommand
    {
        private readonly ILogger _logger;

        public AnalyzeSimulationsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(long actualSimulationId, long? noReviewSimulationId, long[] recommenderSimulationIds, string analyzeResultPath)
        {
            var analyzer = new Analyzer();

            if(noReviewSimulationId.HasValue)
            {
                analyzer.CalculateFaRReductionBetweenRealityAndNoReviews(actualSimulationId, noReviewSimulationId.Value , analyzeResultPath);

            }
            else
            {
                analyzer.AnalyzeSimulations(actualSimulationId, recommenderSimulationIds, analyzeResultPath);
            }
        }
    }
}
