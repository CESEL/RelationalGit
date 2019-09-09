using System;
using System.Threading.Tasks;
using RelationalGit.Commands;
using System.IO;
using RelationalGit.CommandLine;
using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RelationalGit.Data;
using RelationalGit.Gathering;

namespace RelationalGit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConfigMapping.Config();
            var configurationOption = GetConfiguration(args);
            var logger = GetLogger();

            logger.LogInformation("{datetime} operation {command} has started", DateTime.Now, configurationOption.Command);

            InitDatabase();

            await new CommandFactory().Execute(configurationOption, logger).ConfigureAwait(false);

            logger.LogInformation("{datetime} operation {command} has finished", DateTime.Now, configurationOption.Command);
        }

        private static InputOption GetConfiguration(string[] args)
        {
            var consoleConfigurationOption = GetConsoleConfigurationOption(args);
            var fileConfigurationOption = GetFileConfigurationOption(consoleConfigurationOption.AppsettingsPath);
            return consoleConfigurationOption.Override(fileConfigurationOption);
        }

        private static InputOption GetConsoleConfigurationOption(string[] args)
        {
            var userInput = new InputOption();

            Parser.Default.ParseArguments<InputOption>(args)
                .WithParsed(options => userInput = options)
                .WithNotParsed(errors => Console.WriteLine(errors.ToString()));

            if (string.IsNullOrEmpty(userInput.AppsettingsPath))
            {
                userInput.AppsettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "relationalgit.json");
            }

            GitRepositoryDbContext.AppSettingsPath = userInput.AppsettingsPath;

            return userInput;
        }

        private static InputOption GetFileConfigurationOption(string settingpath)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(GitRepositoryDbContext.AppSettingsPath);

            var configuration = builder.Build();
            var section = configuration.GetSection("Mining");
            return section.Get<InputOption>();
        }

        private static ILogger GetLogger()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole().AddDebug();
            return loggerFactory.CreateLogger("RelationalGit.Logger");
        }

        private static void InitDatabase()
        {
            using (var client = new GitRepositoryDbContext())
            {
                client.Database.EnsureCreated();
            }
        }
    }
}
