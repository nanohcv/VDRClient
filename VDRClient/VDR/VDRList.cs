using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VDRClient.VDR
{
    public class VDRList : ObservableCollection<Settings>
    {
        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            StringWriter sw = new StringWriter();
            try
            {
                serializer.Serialize(sw, this);
            }
            catch { }
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["VDRClient"] = sw.ToString();
        }

        public bool Load()
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            string xml = (string)roamingSettings.Values["VDRClient"];
            if (xml == null)
                return false;
            XmlSerializer serializer = new XmlSerializer(typeof(VDRList));
            StringReader sr = new StringReader(xml);
            try
            {
                VDRList vdrs = (VDRList)serializer.Deserialize(sr);
                this.Clear();
                foreach (Settings settings in vdrs)
                {
                    this.Add(settings);
                }
            }
            catch { return false; }
            return true;
        }
    }
}
