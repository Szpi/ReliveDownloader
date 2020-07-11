using System;
using System.Collections.Generic;
using System.Text;

namespace ReliveDownloader
{
    public class DownloadedActivity
    {
        public string Name { get; set; }
        public string Date { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DownloadedActivity activity &&
                   Name == activity.Name &&
                   Date == activity.Date;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Date);
        }
    }
}
