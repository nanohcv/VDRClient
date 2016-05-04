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
            foreach (XElement element in xml.Descendants("groups"))
            {
                foreach (XElement groupElement in element.Descendants("group"))
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
            }
            return groups;
        }
    }
}
