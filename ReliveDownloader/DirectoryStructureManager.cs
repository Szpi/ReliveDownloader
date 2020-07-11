using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ReliveDownloader
{
    class DirectoryStructureManager
    {
        public string GetFilePath(DownloadedActivity activity)
        {
            return GetPath(activity, "mp4");
        }
        public string GetImagePath(DownloadedActivity activity)
        {
            return GetPath(activity, "jpeg");
        }
        private static string GetPath(DownloadedActivity activity, string fileExtension)
        {
            var date = DateTime.Parse(activity.Date);
            var nameWithoutRelive = Regex.Match(activity.Name, "'.+'").Value.Trim('\'');
            var directoryPath = Path.Combine("Relive", date.Year.ToString());
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine("Relive", date.Year.ToString(), $"{nameWithoutRelive}.{fileExtension}");

            while (File.Exists(filePath))
            {
                var filePathWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                filePath = Path.Combine("Relive", date.Year.ToString(), $"{filePathWithoutExtension}(1).{fileExtension}");
            }
            return filePath;
        }        
    }
}
