using System;
using System.Threading.Tasks;
using RelationalGit.Commands;
using RelationalGit.Mapping;
using System.IO;
using RelationalGit.CommandLine;
using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace RelationalGit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConfigMapping.Config();
            var configurationOption = GetConfiguration(args);
            var logger = GetLogger();

            logger.LogInformation("{datetime} operation {command} has started",DateTime.Now,configurationOption.Command);

            InitDatabase();

            /*var userInput = new InputOption()
            {
                AppsettingsPath = GitRepositoryDbContext.AppSettingsPath,
                Command = CommandType.GetPullRequestRevieweComments,
                KnowledgeSaveStrategyType = KnowledgeShareStrategyType.Ideal,
                CoreDeveloperCalculationType = CoreDeveloperCalculationType.AuthoredLines,
                LeaversType = LeaversType.All,
                CoreDeveloperThreshold = 15000,
                MegaPullRequestSize = 100,
                FilesAtRiksOwnershipThreshold = 0.90,
                LeaversOfPeriodExtendedAbsence = 4,
                FilesAtRiksOwnersThreshold = 1,
                MegaCommitSize = 200,
                MegaDevelopers = new[] { "dotnetbot" },//,"dotnet-maestro-bot",
                PeriodType = PeriodType.Month,
                PeriodLength = 3,
                GitHubOwner = "dotnet",
                GitHubToken = "",
                GitHubRepo = "corefx",
                RepositoryPath = @"/home/ehsan/Documents/Repositories/corefx",
                GitBranch = "master",
                Extensions = ".cs,.vb,.ts,.js,.jsx,.sh,.yml,.tsx,.css,.json,.py,.c,.h,.cpp,.il,.make,.cmake,.ps1,.r,.cmd,.html,.conf".Split(',')
            };*/

            //var arguments = Parser.Default.FormatCommandLine(userInput);
            
            await new CommandFactory().Execute(configurationOption, logger);



            logger.LogInformation("{datetime} operation {command} has finished", DateTime.Now, configurationOption.Command);

        }

        private static InputOption GetConfiguration(string[] args)
        {
            var consoleConfigurationOption = GetConsoleConfigurationOption(args);
            var fileConfigurationOption = GetFileConfigurationOption(consoleConfigurationOption.AppsettingsPath);
            var configurationOption = consoleConfigurationOption.Override(fileConfigurationOption);
            return configurationOption;
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
            var logger = loggerFactory.CreateLogger("RelationalGit.Logger");
            return logger;
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


