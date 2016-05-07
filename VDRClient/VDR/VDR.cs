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
                    channelgroup.Add(new Channel(channelid, channelname, shortname, logo));
                }
                groups.Add(channelgroup);
            }
            return groups;
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
    }
}
