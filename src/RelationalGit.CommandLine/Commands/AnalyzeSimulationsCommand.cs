using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelationalGit.Data;
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

        public async Task Execute(long actualSimulationId, long[] recommenderSimulationIds, string analyzeResultPath)
        {
            var analyzer = new Analyzer();
            analyzer.AnalyzeSimulations(actualSimulationId, recommenderSimulationIds, analyzeResultPath);
        }
    }
}
