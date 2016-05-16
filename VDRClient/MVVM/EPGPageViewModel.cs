using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

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

        private VDR.EPGEntry selectedEPGEntry;
        public VDR.EPGEntry SelectedEPGEntry
        {
            get { return selectedEPGEntry; }
            set
            {
                selectedEPGEntry = value;
                NotifyPropertyChanged("SelectedEPGEntry");
                if(selectedEPGEntry != null)
                {
                    ListBox_SelectionChanged();
                }
            }
        }

        private Visibility epgpageVisible;
        public Visibility EPGPageVisible
        {
            get { return epgpageVisible; }
            set
            {
                epgpageVisible = value;
                if (epgpageVisible == Visibility.Visible)
                    searchResultVisible = Visibility.Collapsed;
                else
                    searchResultVisible = Visibility.Visible;
                NotifyPropertyChanged("EPGPageVisible");
                NotifyPropertyChanged("SearchResultVisible");
            }
        }

        private Visibility searchResultVisible;
        public Visibility SearchResultVisible
        {
            get { return searchResultVisible; }
            set
            {
                searchResultVisible = value;
                if (searchResultVisible == Visibility.Visible)
                    epgpageVisible = Visibility.Collapsed;
                else
                    epgpageVisible = Visibility.Visible;
                NotifyPropertyChanged("SearchResultVisible");
                NotifyPropertyChanged("EPGPageVisible");
            }
        }

        public bool SearchTitle { get; set; }
        public bool SearchInfo { get; set; }
        public bool SearchDescr { get; set; }

        public string SearchText { get; set; }
        private List<VDR.EPGEntry> searchResult;
        public List<VDR.EPGEntry> SearchResult
        {
            get { return searchResult; }
            set
            {
                searchResult = value;
                NotifyPropertyChanged("SearchResult");
            }
        }
        public async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(SearchText != null && SearchText != "")
            {
                List<VDR.EPGEntry> entries = null;
                try
                {
                    entries = await vdr.SearchEPG(null, SearchText, SearchTitle, SearchInfo, SearchDescr);
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
                    return;
                }
                for(int i = 0; i<entries.Count; i++)
                {
                    VDR.Channel ch = (from chgroup in ChannelList
                                      from channel in chgroup
                                      where channel.ChannelID == entries[i].ChannelID
                                      select channel).FirstOrDefault();
                    entries[i].ChannelName = ch.Name;
                }
                SelectedEPGEntry = null;
                SearchResult = entries.Where(x => x.Stop > DateTime.Now).ToList().OrderBy(x => x.Start).ToList();
                SearchResultVisible = Visibility.Visible;
            }
        }

        private VDR.Settings settings;
        private VDR.VDR vdr;
        public EPGPageViewModel(VDR.Settings settings)
        {
            this.settings = settings;
            this.vdr = new VDR.VDR(this.settings);
            EPGPageVisible = Visibility.Visible;
            SearchTitle = true;
            SetChannelList();
        }

        private async void SetChannelList()
        {
            try
            {
                ChannelList = await vdr.GetChannelList();
                SelectedGroup = ChannelList[0];
            }
            catch(Exception ex)
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

        private async void GetEpgEntries(VDR.Channel channel)
        {
            try
            {
                List<VDR.EPGEntry> entries = await vdr.GetEPGEntries(channel.ChannelID);
                for (int i = 0; i < entries.Count; i++)
                {
                    VDR.Channel ch = (from chgroup in ChannelList
                                      from chname in chgroup
                                      where chname.ChannelID == entries[i].ChannelID
                                      select chname).FirstOrDefault();
                    entries[i].ChannelName = ch.Name;
                }
                SelectedEPGEntry = null;
                CurrentEPGEntries = entries.Where(x => x.Stop > DateTime.Now).ToList();
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

        public async void ListBox_SelectionChanged()
        {
            if(SelectedEPGEntry != null)
            {
                AddEPGEntryTimerContentDialog dlg = new AddEPGEntryTimerContentDialog(SelectedEPGEntry.Title);
                var result = await dlg.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        await vdr.AddTimer(SelectedEPGEntry);
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
                SelectedEPGEntry = null;
            }
        }
    }
}
