using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReliveDownloader
{
    class CommandLineArguments
    {
        [Option('d', "downloadReliveActivities", Required = false, HelpText = "Download relive activities")]
        public bool DownloadReliveActivities { get; set; }
    }
}
