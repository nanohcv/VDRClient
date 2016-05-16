using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace VDRClient.MVVM
{
    public class TimersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private List<VDR.Timer> timers;
        public List<VDR.Timer> Timers
        {
            get { return timers; }
            set
            {
                timers = value;
                NotifyPropertyChanged("Timers");
            }
        }

        private VDR.Timer selectedTimer;
        public VDR.Timer SelectedTimer
        {
            get { return selectedTimer; }
            set
            {
                selectedTimer = value;
                NotifyPropertyChanged("SelectedTimer");
            }
        }

        private VDR.Settings settings;
        private VDR.VDR vdr;
        public TimersViewModel(VDR.Settings settings)
        {
            this.settings = settings;
            this.vdr = new VDR.VDR(this.settings);
            SetTimers();
        }

        private async void SetTimers()
        {
            try
            {
                Timers = await vdr.GetTimers();
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

        public async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedTimer != null)
            {
                TimerContentDialog dlg = new TimerContentDialog();
                ContentDialogResult result = await dlg.ShowAsync();
                if(result == ContentDialogResult.Primary)
                {
                    TimerContentDialog.TimerAction action = dlg.SelectedAction;
                    if(action == TimerContentDialog.TimerAction.onoff)
                    {
                        try
                        {
                            await vdr.OnOffTimer(SelectedTimer);
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
                    else
                    {
                        try
                        {
                            await vdr.DeleteTimer(SelectedTimer);
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
                    SetTimers();
                }
                SelectedTimer = null;
            }
        }
    }
}
