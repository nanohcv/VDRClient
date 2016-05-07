using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace VDRClient.VDR
{
    public class VDRList : ObservableCollection<Settings>
    {
        public Visibility SwitchButtonVisibility { get; private set; }

        public Settings LastUsedSettings { get; set; }

        public void Save()
        {
            if (this.Count > 1)
            {
                SwitchButtonVisibility = Visibility.Visible;
                OnPropertyChanged(new PropertyChangedEventArgs("SwitchButtonVisibility"));
            }
            else if (this.Count == 1)
            {
                SwitchButtonVisibility = Visibility.Collapsed;
                LastUsedSettings = this[0];
            }
            else
            {
                SwitchButtonVisibility = Visibility.Collapsed;
                OnPropertyChanged(new PropertyChangedEventArgs("SwitchButtonVisibility"));
            }
            
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            StringWriter sw = new StringWriter();
            try
            {
                serializer.Serialize(sw, this);
            }
            catch { }
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["VDRClient"] = sw.ToString();
            roamingSettings.Values["VDRClient_LastIndex"] = this.IndexOf(LastUsedSettings);
        }

        public bool Load()
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            string xml = (string)roamingSettings.Values["VDRClient"];
            int index = 0;
            try
            {
                index = (int)roamingSettings.Values["VDRClient_LastIndex"];
            }
            catch { }
            if (xml == null)
            {
                SwitchButtonVisibility = Visibility.Collapsed;
                OnPropertyChanged(new PropertyChangedEventArgs("SwitchButtonVisibility"));
                return false;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(VDRList));
            StringReader sr = new StringReader(xml);
            try
            {
                VDRList vdrs = (VDRList)serializer.Deserialize(sr);
                if (vdrs.Count == 0)
                {
                    LastUsedSettings = null;
                    SwitchButtonVisibility = Visibility.Collapsed;
                    OnPropertyChanged(new PropertyChangedEventArgs("SwitchButtonVisibility"));
                    return false;
                }
                    
                this.Clear();
                foreach (Settings settings in vdrs)
                {
                    this.Add(settings);
                }
                if(this.Count > 1)
                {
                    SwitchButtonVisibility = Visibility.Visible;
                    OnPropertyChanged(new PropertyChangedEventArgs("SwitchButtonVisibility"));
                }
                else
                {
                    SwitchButtonVisibility = Visibility.Collapsed;
                    OnPropertyChanged(new PropertyChangedEventArgs("SwitchButtonVisibility"));
                }
                if(index >= 0 && index < this.Count)
                {
                    LastUsedSettings = this[index];
                }
                else
                {
                    if(this.Count > 0)
                    {
                        LastUsedSettings = this[0];
                    }
                    else
                    {
                        LastUsedSettings = null;
                    }
                }
            }
            catch { return false; }
            return true;
        }
    }
}
