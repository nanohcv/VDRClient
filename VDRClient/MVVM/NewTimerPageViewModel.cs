using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;

namespace VDRClient.MVVM
{
    public class NewTimerPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public delegate void NavigateBackDelegate(object sender, BackRequestedEventArgs e);
        public NavigateBackDelegate NavigateBack;

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
                NotifyPropertyChanged("SelectedChannelGroup");

                SelectedChannel = selectedChannelGroup[0];
            }
        }

        private VDR.Channel selectedChannel;
        public VDR.Channel SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                if (value != null)
                {
                    selectedChannel = value;
                    Timer.ChannelID = selectedChannel.ChannelID;
                    Timer.ChannelName = selectedChannel.Name;
                    NotifyPropertyChanged("SelectedChannel");
                }
            }
        }

        private VDR.Timer timer;
        public VDR.Timer Timer
        {
            get { return timer; }
            set
            {
                timer = value;
                NotifyPropertyChanged("Timer");
            }
        }

        private ICommand addCommand;
        public ICommand AddCommand
        {
            get
            {
                if (addCommand == null)
                {
                    addCommand = new RelayCommand(
                        param => this.add(),
                        param => true);
                }
                return addCommand;
            }
        }

        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(
                        param => this.cancel(),
                        param => true);
                }
                return cancelCommand;
            }
        }


        private VDR.Settings settings;
        private VDR.VDR vdr;
        public NewTimerPageViewModel(VDR.Settings settings)
        {
            this.settings = settings;
            this.vdr = new VDR.VDR(this.settings);
            Timer = new VDR.Timer();
            SetChannels();
        }

        private async void SetChannels()
        {
            try
            {
                ChannelList = await vdr.GetChannelList();
                SelectedChannelGroup = ChannelList[0];
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

        private async void add()
        {
            try
            {
                await vdr.AddTimer(this.Timer);
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
            if (NavigateBack != null)
            {
                NavigateBack(null, null);
            }
        }

        private void cancel()
        {
            if(NavigateBack != null)
            {
                NavigateBack(null, null);
            }
        }
    }
}
