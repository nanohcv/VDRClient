using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.MVVM
{
    public class EPGPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private List<VDR.ChannelGroup> channelList;
        public List<VDR.ChannelGroup> ChannelList
        {
            get { return channelList; }
            set
            {
                channelList = value;
                NotifyPropertyChanged("ChannelList");
            }
        }

        private VDR.ChannelGroup selectedGroup;
        public VDR.ChannelGroup SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                NotifyPropertyChanged("SelectedGroup");
            }
        }

        private VDR.Channel selectedChannel;
        public VDR.Channel SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                selectedChannel = value;
                GetEpgEntries(selectedChannel);
            }
        }

        private List<VDR.EPGEntry> currentEPGEntries;
        public List<VDR.EPGEntry> CurrentEPGEntries
        {
            get { return currentEPGEntries; }
            set
            {
                currentEPGEntries = value;
                NotifyPropertyChanged("CurrentEPGEntries");
            }
        }

        private VDR.Settings settings;
        private VDR.VDR vdr;
        public EPGPageViewModel(VDR.Settings settings)
        {
            this.settings = settings;
            this.vdr = new VDR.VDR(this.settings);
            SetChannelList();
        }

        private async void SetChannelList()
        {
            try
            {
                ChannelList = await vdr.GetChannelList();
                SelectedGroup = ChannelList[0];
            }
            catch { }
        }

        private async void GetEpgEntries(VDR.Channel channel)
        {
            try
            {
                CurrentEPGEntries = await vdr.GetEPGEntries(channel.ChannelID);
            }
            catch { }
        }
    }
}
