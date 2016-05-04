using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class Channel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string epgtitle;

        public string ChannelID { get; private set; }
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public string LogoURL { get; private set; }
        public string EPGTitle
        {
            get
            {
                return this.epgtitle;
            }
            set
            {
                if (value != this.epgtitle)
                {
                    this.epgtitle = value;
                    NotifyPropertyChanged("EPGTitle");
                }
            }
        }

        public Channel(string id, string name, string shortname, string logo)
        {
            ChannelID = id;
            Name = name;
            ShortName = shortname;
            LogoURL = logo;
        }
    }
}
