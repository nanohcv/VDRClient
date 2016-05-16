using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace VDRClient.VDR
{
    public class VDR
    {
        private Settings settings;

        public VDR(Settings settings)
        {
            this.settings = settings;
        }

        public async Task<List<string>> GetProfiles()
        {
            List<string> profiles = new List<string>();
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string presets = await client.GetStringAsync(settings.BaseURL + "presets.ini");
            Regex r = new Regex(@"[\s\t]*\[([^\[\]]*)\][\s\t]*\r?\n[\s\t]*Cmd=");
            MatchCollection matches = r.Matches(presets);
            for (int i = 0; i < matches.Count; i++)
            {
                profiles.Add(matches[i].Groups[1].Value);
            }
            return profiles;
        }

        public async Task<XmlApiVersion> GetXMLAPIVersion()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "version.xml");
            var xml = XDocument.Parse(xmlstring);
            XmlApiVersion v = new XmlApiVersion(xml.Root.Element("version").Value);
            return v;
        }

        public async Task<List<ChannelGroup>> GetChannelList()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "channels.xml");
            var xml = XDocument.Parse(xmlstring);
            List<ChannelGroup> groups = new List<ChannelGroup>();
            foreach (XElement groupElement in xml.Descendants("group"))
            {
                string groupname = groupElement.Attribute("name").Value;
                ChannelGroup channelgroup = new ChannelGroup(groupname);
                foreach (XElement channelElement in groupElement.Descendants("channel"))
                {
                    string channelid = channelElement.Attribute("id").Value;
                    bool isradio = false;
                    if(channelElement.Element("isradio") != null)
                    {
                        string ir = channelElement.Element("isradio").Value;
                        if (ir == "true")
                            isradio = true;
                    }
                    string channelname = "";
                    if (channelElement.Element("name") != null)
                    {
                        channelname = channelElement.Element("name").Value;
                    }
                    string shortname = "";
                    if (channelElement.Element("shortname") != null)
                    {
                        shortname = channelElement.Element("shortname").Value;
                    }
                    string logo = "";
                    if (channelElement.Element("logo") != null)
                    {
                        logo = channelElement.Element("logo").Value;
                    }
                    channelgroup.Add(new Channel(channelid, isradio, channelname, shortname, logo));
                }
                groups.Add(channelgroup);
            }
            return groups;
        }

        public async Task<List<EPGEntry>> SearchEPG(string channelid, string search, bool stitle=true, bool sshorttext=false, bool sdescription=false)
        {
            string options = "";
            if (stitle)
                options += "T";
            if (sshorttext)
                options += "S";
            if (sdescription)
                options += "D";
            string url = settings.BaseURL + "epg.xml?search=" + search + "&options=" + options;
            if (channelid != null && channelid != "")
                url += "&chid=" + channelid;

            List<EPGEntry> epgentries = new List<EPGEntry>();
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(url);
            var xml = XDocument.Parse(xmlstring);
            foreach (XElement eventElement in xml.Descendants("event"))
            {
                string eventid = eventElement.Attribute("id").Value;
                string chid = "";
                if (eventElement.Element("channelid") != null)
                {
                    chid = eventElement.Element("channelid").Value;
                }
                string title = "";
                if (eventElement.Element("title") != null)
                {
                    title = eventElement.Element("title").Value;
                }
                string shorttext = "";
                if (eventElement.Element("shorttext") != null)
                {
                    shorttext = eventElement.Element("shorttext").Value;
                }
                string description = "";
                if (eventElement.Element("description") != null)
                {
                    description = eventElement.Element("description").Value;
                }
                string start = "";
                if (eventElement.Element("start") != null)
                {
                    start = eventElement.Element("start").Value;
                }
                string stop = "";
                if (eventElement.Element("stop") != null)
                {
                    stop = eventElement.Element("stop").Value;
                }
                string duration = "";
                if (eventElement.Element("duration") != null)
                {
                    duration = eventElement.Element("duration").Value;
                }
                epgentries.Add(new EPGEntry(eventid, chid, title, shorttext, description, start, stop, duration));
            }
            return epgentries;
        }

        public async Task<List<EPGEntry>> GetEPGEntries(string channelid)
        {
            List<EPGEntry> epgentries = new List<EPGEntry>();
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "epg.xml?chid=" + channelid);
            var xml = XDocument.Parse(xmlstring);
            foreach (XElement eventElement in xml.Descendants("event"))
            {
                string eventid = eventElement.Attribute("id").Value;
                string title = "";
                if(eventElement.Element("title") != null)
                {
                    title = eventElement.Element("title").Value;
                }
                string shorttext = "";
                if(eventElement.Element("shorttext") != null)
                {
                    shorttext = eventElement.Element("shorttext").Value;
                }
                string description = "";
                if(eventElement.Element("description") != null)
                {
                    description = eventElement.Element("description").Value;
                }
                string start = "";
                if(eventElement.Element("start") != null)
                {
                    start = eventElement.Element("start").Value;
                }
                string stop = "";
                if(eventElement.Element("stop") != null)
                {
                    stop = eventElement.Element("stop").Value;
                }
                string duration = "";
                if(eventElement.Element("duration") != null)
                {
                    duration = eventElement.Element("duration").Value;
                }
                epgentries.Add(new EPGEntry(eventid, channelid, title, shorttext, description, start, stop, duration));
            }
            return epgentries;
        }

        public async Task<EPGEntry> GetCurrentEPGEntry(string channelid)
        {
            return await GetEPGEntry(channelid, "now");
        }

        public async Task<EPGEntry> GetNextEPGEntry(string channelid)
        {
            return await GetEPGEntry(channelid, "next");
        }

        public async Task<EPGEntry> GetEPGEntryAtTime(string channelid, DateTime time)
        {
            DateTime localtime = DateTime.SpecifyKind(time, DateTimeKind.Local);
            DateTimeOffset dtoffset = localtime;
            string uxtime = dtoffset.ToUnixTimeSeconds().ToString();
            return await GetEPGEntry(channelid, uxtime);
        }

        private async Task<EPGEntry> GetEPGEntry(string channelid, string parameter)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "epg.xml?chid=" + channelid + "&at=" + parameter);
            var xml = XDocument.Parse(xmlstring);
            EPGEntry entry = null;
            foreach (XElement eventElement in xml.Descendants("event"))
            {
                string eventid = eventElement.Attribute("id").Value;
                string title = "";
                if (eventElement.Element("title") != null)
                {
                    title = eventElement.Element("title").Value;
                }
                string shorttext = "";
                if (eventElement.Element("shorttext") != null)
                {
                    shorttext = eventElement.Element("shorttext").Value;
                }
                string description = "";
                if (eventElement.Element("description") != null)
                {
                    description = eventElement.Element("description").Value;
                }
                string start = "";
                if (eventElement.Element("start") != null)
                {
                    start = eventElement.Element("start").Value;
                }
                string stop = "";
                if (eventElement.Element("stop") != null)
                {
                    stop = eventElement.Element("stop").Value;
                }
                string duration = "";
                if (eventElement.Element("duration") != null)
                {
                    duration = eventElement.Element("duration").Value;
                }
                entry = new EPGEntry(eventid, channelid, title, shorttext, description, start, stop, duration);
            }
            return entry;
        }

        public async Task<List<Recording>> GetRecordings(bool deleted = false)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string url = settings.BaseURL + "recordings.xml";
            if (deleted)
                url = settings.BaseURL + "deletedrecordings.xml";
            List<Recording> recordings = new List<Recording>();
            string xmlstring = await client.GetStringAsync(url);
            var xml = XDocument.Parse(xmlstring);

            foreach (XElement recordingElement in xml.Descendants("recording"))
            {
                string name = "";
                if(recordingElement.Element("name") != null)
                {
                    name = recordingElement.Element("name").Value;
                }
                string filename = "";
                if(recordingElement.Element("filename") != null)
                {
                    filename = recordingElement.Element("filename").Value;
                }
                string title = "";
                if(recordingElement.Element("title") != null)
                {
                    title = recordingElement.Element("title").Value;
                }
                string inuse = "";
                if(recordingElement.Element("inuse") != null)
                {
                    inuse = recordingElement.Element("inuse").Value;
                }
                string size = "";
                if(recordingElement.Element("size") != null)
                {
                    size = recordingElement.Element("size").Value;
                }
                string duration = "";
                if(recordingElement.Element("duration") != null)
                {
                    duration = recordingElement.Element("duration").Value;
                }
                string sdeleted = "";
                if(recordingElement.Element("deleted") != null)
                {
                    sdeleted = recordingElement.Element("deleted").Value;
                }
                string channelid = "";
                string channelname = "";
                string epgtitle = "";
                string epgshorttext = "";
                string epgdescription = "";
                if(recordingElement.Element("infos") != null)
                {
                    if(recordingElement.Element("infos").Element("channelid") != null)
                    {
                        channelid = recordingElement.Element("infos").Element("channelid").Value;
                    }
                    if(recordingElement.Element("infos").Element("channelname") != null)
                    {
                        channelname = recordingElement.Element("infos").Element("channelname").Value;
                    }
                    if(recordingElement.Element("infos").Element("title") != null)
                    {
                        epgtitle = recordingElement.Element("infos").Element("title").Value;
                    }
                    if(recordingElement.Element("infos").Element("shorttext") != null)
                    {
                        epgshorttext = recordingElement.Element("infos").Element("shorttext").Value;
                    }
                    if(recordingElement.Element("infos").Element("description") != null)
                    {
                        epgdescription = recordingElement.Element("infos").Element("description").Value;
                    }
                }
                recordings.Add(new Recording(name, filename, title, inuse, size, duration, sdeleted, channelid, channelname, epgtitle, epgshorttext, epgdescription));
            }
            return recordings;
        }

        public async Task<bool> DeleteRecording(Recording recording)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string url = settings.BaseURL + "recordings.xml?filename=" + WebUtility.UrlEncode(recording.FileName) + "&action=delete";
            string xmlstring = await client.GetStringAsync(url);
            var xml = XDocument.Parse(xmlstring);
            if(xml.Element("actions") != null)
            {
                if (xml.Element("actions").Element("delete") != null)
                {
                    string deleteresult = xml.Element("actions").Element("delete").Value;
                    if (deleteresult == "true")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> RemoveRecording(Recording recording)
        {
            if(recording.Deleted == DateTime.MinValue)
            {
                return false;
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string url = settings.BaseURL + "deletedrecordings.xml?filename=" + WebUtility.UrlEncode(recording.FileName) + "&action=remove";
            string xmlstring = await client.GetStringAsync(url);
            var xml = XDocument.Parse(xmlstring);
            if (xml.Element("actions") != null)
            {
                if (xml.Element("actions").Element("remove") != null)
                {
                    string removeresult = xml.Element("actions").Element("remove").Value;
                    if (removeresult == "true")
                        return true;
                }
            }
            return false;
        }

        public async Task<bool> UndeleteRecording(Recording recording)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string url = settings.BaseURL + "deletedrecordings.xml?filename=" + WebUtility.UrlEncode(recording.FileName) + "&action=undelete";
            if (recording.Deleted == DateTime.MinValue)
                return false;
            string xmlstring = await client.GetStringAsync(url);
            var xml = XDocument.Parse(xmlstring);
            if (xml.Element("actions") != null)
            {
                if(xml.Element("actions").Element("undelete") != null)
                {
                    string undeleteresult = xml.Element("actions").Element("undelete").Value;
                    if (undeleteresult == "true")
                        return true;
                }
            }
            return false;
        }

        public async Task<List<Timer>> GetTimers()
        {
            List<Timer> timers = new List<Timer>();
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "timers.xml");
            var xml = XDocument.Parse(xmlstring);
            foreach (XElement timerElement in xml.Descendants("timer"))
            {
                string tid = timerElement.Attribute("id").Value;
                string chid = "";
                if (timerElement.Element("channelid") != null)
                {
                    chid = timerElement.Element("channelid").Value;
                }
                string chname = "";
                if (timerElement.Element("channelname") != null)
                {
                    chname = timerElement.Element("channelname").Value;
                }
                string name = "";
                if (timerElement.Element("name") != null)
                {
                    name = timerElement.Element("name").Value;
                }
                string aux = "";
                if (timerElement.Element("aux") != null)
                {
                    aux = timerElement.Element("aux").Value;
                }
                string sflags = "";
                if (timerElement.Element("flags") != null)
                {
                    sflags = timerElement.Element("flags").Value;
                }
                string swdays = "";
                if (timerElement.Element("weekdays") != null)
                {
                    swdays = timerElement.Element("weekdays").Value;
                }
                string sday = "";
                if (timerElement.Element("day") != null)
                {
                    sday = timerElement.Element("day").Value;
                }
                string sstart = "";
                if (timerElement.Element("start") != null)
                {
                    sstart = timerElement.Element("start").Value;
                }
                string sstop = "";
                if (timerElement.Element("stop") != null)
                {
                    sstop = timerElement.Element("stop").Value;
                }
                string spriority = "";
                if (timerElement.Element("priority") != null)
                {
                    spriority = timerElement.Element("priority").Value;
                }
                string slifetime = "";
                if (timerElement.Element("lifetime") != null)
                {
                    slifetime = timerElement.Element("lifetime").Value;
                }
                timers.Add(new Timer(tid, chid, chname, name, aux, sflags, swdays, sday, sstart, sstop, spriority, slifetime));
            }
            return timers;
        }

        public async Task<bool> AddTimer(EPGEntry entry)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "timers.xml?action=add&chid=" + entry.ChannelID + "&eventid=" + entry.EventID);
            var xml = XDocument.Parse(xmlstring);
            if(xml.Element("added")!=null)
            {
                string added = xml.Element("added").Value;
                if (added == "true")
                    return true;
            }
            return false;
        }

        public async Task<bool> AddTimer(Timer timer)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "timers.xml?action=add&" + timer.AddCmd());
            var xml = XDocument.Parse(xmlstring);
            if (xml.Element("added") != null)
            {
                string added = xml.Element("added").Value;
                if (added == "true")
                    return true;
            }
            return false;
        }

        public async Task<bool> OnOffTimer(Timer timer)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "timers.xml?action=onoff&id=" + WebUtility.UrlEncode(timer.TimerID));
            var xml = XDocument.Parse(xmlstring);
            if (xml.Element("onoff") != null)
            {
                string onoff = xml.Element("onoff").Value;
                if (onoff == "successful")
                    return true;
            }
            return false;
        }

        public async Task<bool> DeleteTimer(Timer timer)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            HttpClient client = new HttpClient(handler);
            string xmlstring = await client.GetStringAsync(settings.BaseURL + "timers.xml?action=delete&id=" + WebUtility.UrlEncode(timer.TimerID));
            var xml = XDocument.Parse(xmlstring);
            if (xml.Element("deleted") != null)
            {
                string deleted = xml.Element("deleted").Value;
                if (deleted == "true")
                    return true;
            }
            return false;
        }
    }
}
