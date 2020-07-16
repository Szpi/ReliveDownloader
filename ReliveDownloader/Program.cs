using CommandLine;
using Microsoft.Extensions.Configuration;
using ReliveDownloader.Configurations;

namespace ReliveDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Configuration>()
                .Build();
            var commandLine = Parser.Default.ParseArguments<CommandLineArguments>(args);

            commandLine.WithParsed<CommandLineArguments>(x =>
            {
                if (x.DownloadReliveActivities)
                {
                    new ReliveActivitiesDownloader().DownloadReliveActivities(config);
                }
            });
        }

    }
}
