using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class EPGEntry
    {
        public string EventID { get; private set; }
        public string ChannelID { get; private set; }
        public string Title { get; private set; }
        public string ShortText { get; private set; }
        public string Description { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime Stop { get; private set; }
        public int Duration { get; private set; }

        public string TimeString { get; private set; }

        public EPGEntry(string eventid, string channelid, string title, string shorttext, string description, string start, string stop, string duration)
        {
            EventID = eventid;
            ChannelID = channelid;
            Title = title;
            ShortText = shorttext;
            Description = description;
            try
            {
                DateTimeOffset start_uxtime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(start));
                Start = start_uxtime.LocalDateTime;
                DateTimeOffset stop_uxtime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(stop));
                Stop = stop_uxtime.LocalDateTime;
                Duration = Convert.ToInt32(duration);
            }
            catch { }
            TimeString = Start.ToString("ddd HH:mm") + "  -  " + Stop.ToString("HH:mm");
        }

    }
}
