using Microsoft.Extensions.Logging;

namespace RelationalGit.Commands
{
    internal abstract class BaseCommand
    {
        private ILogger _logger;

        public BaseCommand(ILogger logger)
        {
            _logger = logger;
        }
    }
}
