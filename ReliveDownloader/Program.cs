﻿using Microsoft.Extensions.Configuration;
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
        private const string DownloadedActivitiesFile = "DownloadedActivities.json";
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Configuration>()
                .Build();



            var alreadyDownloadedActivities = File.Exists(DownloadedActivitiesFile) ?
                JsonConvert.DeserializeObject<DownloadedActivities>(File.ReadAllText(DownloadedActivitiesFile)) :
                new DownloadedActivities();

            var directoryStructureManager = new DirectoryStructureManager();
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            chromeOptions.AddArguments("lang=en");

            using var driver = new ChromeDriver(chromeOptions)
            {
                Url = "https://www.relive.cc/login"
            };

            LoginToRelive(config, driver);

            driver.Navigate().GoToUrl(@"https://www.relive.cc/settings/my-data");            

            var activitiesToDownloadUrl = new List<string>();
            GetUrlToVideoPage(driver, activitiesToDownloadUrl);

            var webClient = DownloadVideos(alreadyDownloadedActivities, directoryStructureManager, driver, activitiesToDownloadUrl);

            driver.Close();
        }

        private static void LoginToRelive(IConfigurationRoot config, ChromeDriver driver)
        {
            var configurationSection = config.GetSection(nameof(Configuration));
            var email = configurationSection.GetSection(nameof(Configuration.Email)).Value;
            var password = configurationSection.GetSection(nameof(Configuration.Password)).Value;

            driver.FindElement(By.Name("email")).SendKeys(email);
            driver.FindElement(By.Name("password")).SendKeys(password);
            driver.FindElement(By.CssSelector("form .login-button")).Click();
        }

        private static WebClient DownloadVideos(DownloadedActivities alreadyDownloadedActivities, DirectoryStructureManager directoryStructureManager, ChromeDriver driver, List<string> activitiesToDownloadUrl)
        {
            var webClient = new WebClient();
            foreach (var activityToDownloadUrl in activitiesToDownloadUrl)
            {
                driver.Navigate().GoToUrl(activityToDownloadUrl);
                var videoContainer = default(IWebElement);
                var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(20));

                videoContainer = wait.Until(driver => driver.FindElement(By.ClassName("video-container")));
                var name = videoContainer.FindElement(By.CssSelector("[itemprop=name]")).GetAttribute("content");

                if (name.Contains("Ride", System.StringComparison.InvariantCultureIgnoreCase) || name.Contains("hike", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var videoUrl = videoContainer.FindElement(By.CssSelector("[itemprop=contentURL]")).GetAttribute("content");
                string date = null;
                var titles = driver.FindElementByClassName("titles").FindElements(By.CssSelector("p"));

                foreach (var item in titles)
                {
                    if (item.Text.Contains("•"))
                    {
                        date = Regex.Match(item.Text, "•.+•").Value.Trim('•');
                    }
                }
                if (date == null)
                {
                    throw new Exception();
                }
                var activity = new DownloadedActivity
                {
                    Name = name,
                    Date = date
                };

                if (alreadyDownloadedActivities.Names.Contains(activity))
                {
                    continue;
                }

                webClient.DownloadFile(videoUrl, directoryStructureManager.GetFilePath(activity));

                var mapDiv = driver.FindElementById("map");
                SaveScreenShotWithElement(driver, mapDiv, directoryStructureManager, activity);


                alreadyDownloadedActivities.Names.Add(activity);
                var json = JsonConvert.SerializeObject(alreadyDownloadedActivities, Formatting.Indented);
                File.WriteAllText(DownloadedActivitiesFile, json);
            }

            return webClient;
        }

        private static void GetUrlToVideoPage(ChromeDriver driver, List<string> activitiesToDownloadUrl)
        {
            var activities = driver.FindElementsByCssSelector(".export-card");
            foreach (var activity in activities)
            {
                var hrefs = activity.FindElements(By.CssSelector("a"));

                foreach (var href in hrefs)
                {
                    var innerText = href.Text;
                    if (innerText == "View")
                    {
                        activitiesToDownloadUrl.Add(href.GetAttribute("href"));
                    }
                }
            }
        }

        public static void SaveScreenShotWithElement(ChromeDriver driver, IWebElement elem,
            DirectoryStructureManager directoryStructureManager,
            DownloadedActivity activity)
        {
            driver.ExecuteScript("arguments[0].scrollIntoView(true);", elem);

            var myScreenShot = ((ITakesScreenshot)driver).GetScreenshot();
            var screen = new Bitmap(new MemoryStream(myScreenShot.AsByteArray));
            using var elemScreenshot = screen.Clone(new Rectangle(new Point(elem.Location.X, 0), elem.Size), screen.PixelFormat);
            screen.Dispose();
            elemScreenshot.Save(directoryStructureManager.GetImagePath(activity));
        }
    }
}