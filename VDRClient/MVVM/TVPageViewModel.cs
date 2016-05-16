using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace VDRClient.MVVM
{
    public class TVPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private DispatcherTimer epgTimer;
        private DispatcherTimer channelListEpgTimer;

        public delegate void HandleMobileViewDelegate();
        public HandleMobileViewDelegate HandleMobileView;

        private VDR.Settings settings;
        private VDR.VDR vdr;

        private VDR.EPGEntry currentEPG;
        public VDR.EPGEntry CurrentEPG
        {
            get { return currentEPG; }
            set
            {
                currentEPG = value;
                if (currentEPG != null)
                {
                    epgTimer.Stop();
                    epgTimer.Interval = (currentEPG.Stop - DateTime.Now) + TimeSpan.FromSeconds(1);
                    epgTimer.Start();
                }
                else
                {
                    epgTimer.Stop();
                }
                NotifyPropertyChanged("CurrentEPG");
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

        private VDR.ChannelGroup selectedChannelGroup;
        public VDR.ChannelGroup SelectedChannelGroup
        {
            get { return selectedChannelGroup; }
            set
            {
                selectedChannelGroup = value;
                channelListEpgTimer.Stop();
                channelListEpgTimer.Start();
                UpdateEPG(selectedChannelGroup);
            }
        }

        private Uri mediaSource;
        public Uri MediaSource
        {
            get { return mediaSource; }
            set
            {
                mediaSource = value;
                NotifyPropertyChanged("MediaSource");
            }
        }

        private Visibility showSearchResult;
        public Visibility ShowSearchResult
        {
            get { return showSearchResult; }
            set
            {
                showSearchResult = value;
                if (showSearchResult == Visibility.Collapsed)
                {                   
                    showChannelList = Visibility.Visible;
                }
                else
                {
                    showChannelList = Visibility.Collapsed;
                }
                NotifyPropertyChanged("ShowSearchResult");
                NotifyPropertyChanged("ShowChannelList");
            }
        }
        private Visibility showChannelList;
        public Visibility ShowChannelList
        {
            get { return showChannelList; }
            set
            {
                showChannelList = value;
                if(showChannelList == Visibility.Collapsed)
                {
                    showSearchResult = Visibility.Visible;
                }
                else
                {
                    showSearchResult = Visibility.Collapsed;
                }
                NotifyPropertyChanged("ShowSearchResult");
                NotifyPropertyChanged("ShowChannelList");
            }
        }

        private List<VDR.Channel> searchResult;
        public List<VDR.Channel> SearchResult
        {
            get { return searchResult; }
            set
            {
                searchResult = value;
                if(searchResult == null)
                {
                    ShowSearchResult = Visibility.Collapsed;
                }
                else
                {
                    ShowSearchResult = Visibility.Visible;
                }
                NotifyPropertyChanged("SearchResult");
            }
        }

        private VDR.Channel selectedChannel;
        public VDR.Channel SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                selectedChannel = value;
                if (selectedChannel != null)
                {
                    SwitchToChannel();
                }
                NotifyPropertyChanged("SelectedChannel");
            }
        }

        public void MediaPlayer_Stopped(object sender, EventArgs e)
        {
            MediaSource = null;
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                if(searchText != "")
                {
                    List<VDR.Channel> allChannels = new List<VDR.Channel>();
                    foreach(VDR.ChannelGroup gp in ChannelList)
                    {
                        foreach(VDR.Channel ch in gp)
                        {
                            allChannels.Add(ch);
                        }
                    }
                    SearchResult = allChannels.Where(x => x.Name.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                    UpdateEPG(SearchResult);

                }
                else
                {
                    SearchResult = null;
                }
            }
        }

        public TVPageViewModel(VDR.Settings settings)
        {
            this.settings = settings;
            vdr = new VDR.VDR(this.settings);
            searchText = "";
            ShowChannelList = Visibility.Visible;
            epgTimer = new DispatcherTimer();
            epgTimer.Tick += EpgTimer_Tick;
            channelListEpgTimer = new DispatcherTimer();
            channelListEpgTimer.Interval = new TimeSpan(0, 5, 0);
            channelListEpgTimer.Tick += ChannelListEpgTimer_Tick;
            SetChannelList();
        }

        private void ChannelListEpgTimer_Tick(object sender, object e)
        {
            UpdateEPG(selectedChannelGroup);
        }

        private async void EpgTimer_Tick(object sender, object e)
        {
            try
            {
                CurrentEPG = await vdr.GetCurrentEPGEntry(CurrentEPG.ChannelID);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if(ex.InnerException != null)
                {
                    msg += "\r\n" + ex.InnerException.Message;
                }
                LogWriter.WriteToLog(msg);
                LogWriter.WriteLogToFile();
            }
        }

        private async void SetChannelList()
        {
            try
            {
                ChannelList = await vdr.GetChannelList();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += "\r\n" + ex.InnerException.Message;
                }
                LogWriter.WriteToLog(msg);
                LogWriter.WriteLogToFile();
            }
        }

        private async void UpdateEPG(List<VDR.Channel> channels)
        {
            foreach(VDR.Channel channel in channels)
            {
                try
                {
                    VDR.EPGEntry entry = await vdr.GetCurrentEPGEntry(channel.ChannelID);
                    if (entry != null)
                    {
                        channel.EPGTitle = entry.Title;
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        msg += "\r\n" + ex.InnerException.Message;
                    }
                    LogWriter.WriteToLog(msg);
                    LogWriter.WriteLogToFile();
                }
            }
        }

        private async void SwitchToChannel()
        {
            try
            {
                CurrentEPG = await vdr.GetCurrentEPGEntry(SelectedChannel.ChannelID);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += "\r\n" + ex.InnerException.Message;
                }
                LogWriter.WriteToLog(msg);
                LogWriter.WriteLogToFile();
            }
            if(SelectedChannel.IsRadio)
            {
                MediaSource = new Uri(settings.BaseURL + "stream.ts?chid=" + SelectedChannel.ChannelID + "&preset=" + settings.AudioProfile);
            }
            else
            {
                MediaSource = new Uri(settings.BaseURL + "stream.ts?chid=" + SelectedChannel.ChannelID + "&preset=" + settings.Profile);
            }
            
            SelectedChannel = null;
            if(this.HandleMobileView != null)
            {
                HandleMobileView();
            }
        }
    }
}
