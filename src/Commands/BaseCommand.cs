using Microsoft.Extensions.Logging;

namespace RelationalGit.Commands
{
    internal abstract class BaseCommand
    {
        private readonly ILogger _logger;

        public BaseCommand(ILogger logger)
        {
            _logger = logger;
        }
    }
}
