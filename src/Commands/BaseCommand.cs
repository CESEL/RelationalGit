using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

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
