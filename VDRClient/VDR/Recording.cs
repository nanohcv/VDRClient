using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class Recording
    {
        public string Name { get; private set; }
        public string FileName { get; private set; }
        public string Title { get; private set; }
        public bool InUse { get; private set; }
        public int SizeMB { get; private set; }
        public int DurationInSec { get; private set; }
        public string FileInformation { get; private set; }
        public DateTime Deleted { get; private set; }
        public string ChannelID { get; private set; }
        public string ChannelName { get; private set; }
        public string EPGTitle { get; private set; }
        public string EPGShortText { get; private set; }
        public string EPGDescription { get; private set; }

        public Recording(string name, string filename, string title, string inuse, string size, string duration, string deleted,
                         string channelid, string channelname, string epgtitle, string epgshorttext, string epgdescription)
        {
            Name = name;
            FileName = filename;
            Title = title;
            if (inuse == "true")
                InUse = true;
            else
                InUse = false;
            SizeMB = 0;
            try
            {
                SizeMB = Convert.ToInt32(size);
            }
            catch { }
            DurationInSec = 0;
            try
            {
                DurationInSec = Convert.ToInt32(duration);
            }
            catch { }
            Deleted = DateTime.MinValue;
            try
            {
                Int64 deleted_i64 = Convert.ToInt64(deleted);
                if (deleted_i64 != 0)
                {
                    DateTimeOffset deleted_uxtime = DateTimeOffset.FromUnixTimeSeconds(deleted_i64);
                    Deleted = deleted_uxtime.LocalDateTime;
                }
            }
            catch { }
            TimeSpan span = new TimeSpan(0, 0, DurationInSec);
            FileInformation = span.ToString(@"hh\:mm\:ss") + "h  =  " + SizeMB.ToString() + "MB";
            ChannelID = channelid;
            ChannelName = channelname;
            EPGTitle = epgtitle;
            EPGShortText = epgshorttext;
            EPGDescription = epgdescription;
        }
    }
}
