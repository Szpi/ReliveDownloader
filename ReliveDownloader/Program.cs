using CommandLine;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
