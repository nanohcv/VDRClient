using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class Timer
    {
        public string TimerID { get; set; }
        public string ChannelID { get; set; }
        public string ChannelName { get; set; }
        public string Name { get; set; }
        public string Aux { get; set; }
        public bool Active { get; set; }
        public bool UseVPS { get; set; }
        public bool[] WeekDays { get; private set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public int Priority { get; set; }
        public int Lifetime { get; set; }

        public DateTimeOffset Day
        {
            get
            {
                return Start;
            }
            set
            {
                Start = new DateTime(value.Year, value.Month, value.Day);
            }
        }

        public TimeSpan StartTime
        {
            get
            {
                return new TimeSpan(Start.Hour, Start.Minute, 0);
            }
            set
            {
                Start = new DateTime(Start.Year, Start.Month, Start.Day);
                Start += value;
            }
        }

        public TimeSpan StopTime
        {
            get
            {
                return new TimeSpan(Stop.Hour, Stop.Minute, 0);
            }
            set
            {
                if(value < Start.TimeOfDay)
                {
                    Stop = new DateTime(Start.Year, Start.Month, Start.Day) + new TimeSpan(value.Hours + 24, value.Minutes, 0);
                }
                else
                {
                    Stop = new DateTime(Start.Year, Start.Month, Start.Day) + new TimeSpan(value.Hours, value.Minutes, 0);
                }
            }
        }

        public string TimeString
        {
            get
            {
                return Start.ToString("ddd dd.MM.yy HH:mm") + " - " + Stop.ToString("HH:mm");
            }
        }

        public Timer(string id, string chid, string chname, string name, string aux, string sflags, string swdays, string sday, string sstart, string sstop, string spriority, string slifetime)
        {
            TimerID = id;
            ChannelID = chid;
            ChannelName = chname;
            Name = name;
            Aux = aux;
            int flags = 1;
            int wdays = 0;
            long day = 0;
            int start = 0;
            int stop = 0;
            int priority = 50;
            int lifetime = 99;
            try
            {
                flags = Convert.ToInt32(sflags);
                wdays = Convert.ToInt32(swdays);
                day = Convert.ToInt64(sday);
                start = Convert.ToInt32(sstart);
                stop = Convert.ToInt32(sstop);
                priority = Convert.ToInt32(spriority);
                lifetime = Convert.ToInt32(lifetime);
            }
            catch { }

            Active = (flags & 1) == 1;
            UseVPS = (flags & 4) == 4;
            WeekDays = new bool[7];
            WeekDays[0] = (wdays & 1) == 1;
            WeekDays[1] = (wdays & 2) == 2;
            WeekDays[2] = (wdays & 4) == 4;
            WeekDays[3] = (wdays & 8) == 8;
            WeekDays[4] = (wdays & 16) == 16;
            WeekDays[5] = (wdays & 32) == 32;
            WeekDays[6] = (wdays & 64) == 64;
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(day);
            Start = offset.LocalDateTime;
            Start = Start + new TimeSpan((int)(start / 100), (int)(start % 100), 0);
            if (stop < start)
            {
                Stop = offset.LocalDateTime;
                Stop = Stop + new TimeSpan((int)(stop / 100) + 24, (int)(stop % 100), 0);
            }
            else
            {
                Stop = offset.LocalDateTime;
                Stop = Stop + new TimeSpan((int)(stop / 100), (int)(stop % 100), 0);
            }
            Priority = priority;
            Lifetime = lifetime;
        }

        public Timer()
        {
            TimerID = "";
            ChannelID = "";
            ChannelName = "";
            Name = "";
            Aux = "";
            Active = true;
            UseVPS = false;
            WeekDays = new bool[7];
            Start = DateTime.Now;
            Stop = DateTime.Now + new TimeSpan(1, 0, 0);
            Priority = 50;
            Lifetime = 99;
        }


        public string AddCmd()
        {
            string cmd = "chid=" + ChannelID;
            cmd += "&name=" + WebUtility.UrlEncode(Name);
            cmd += "&aux=" + WebUtility.UrlEncode(Aux);
            int flags = Active ? 1 : 0;
            flags = UseVPS ? flags | 4 : flags;
            cmd += "&flags=" + flags.ToString();
            int weekdays = 0;
            for(int i=0; i<7;i++)
            {
                weekdays += WeekDays[i] ? (int)Math.Pow(2, i) : 0;
            }
            cmd += "&weekdays=" + weekdays.ToString();
            long day = 0;
            DateTimeOffset dtoffset = DateTime.SpecifyKind(new DateTime(Start.Year, Start.Month, Start.Day), DateTimeKind.Local);
            day = dtoffset.ToUnixTimeSeconds();
            cmd += "&day=" + day.ToString();
            int start = (Start.Hour * 100) + Start.Minute;
            int stop = (Stop.Hour * 100) + Stop.Minute;
            cmd += "&start=" + start.ToString() + "&stop=" + stop.ToString();
            cmd += "&priority=" + Priority.ToString();
            cmd += "&lifetime=" + Lifetime.ToString();
            return cmd;
        }

    }
}
