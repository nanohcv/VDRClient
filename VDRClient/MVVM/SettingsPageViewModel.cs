using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VDRClient.VDR;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace VDRClient.MVVM
{
    public class SettingsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private VDRList vdrs;
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        public SettingsPageViewModel()
        {
            protocols = new List<string>();
            protocols.Add("http://");
            protocols.Add("https://");
            vdrs = new VDRList();
            foreach(Settings settings in Configuration.VDRs)
            {
                vdrs.Add(settings);
            }
        }

        private List<string> protocols;

        public List<string> Protocols
        {
            get { return protocols; }
        }

        public bool NewSettings { get; set; }

        public bool EnableAddButton
        {
            get { return !NewSettings; }
        }

        public VDRList VDRs
        {
            get
            {
                //return VDR.Configuration.VDRs;
                return vdrs;
            }
        }

        private Settings vdrSettings;

        public Settings VDRSettings
        {
            get { return vdrSettings; }
            set
            {
                vdrSettings = value;
                NewSettings = true;
                NotifyPropertyChanged("NewSettings");
                NotifyPropertyChanged("EnableAddButton");
                NotifyPropertyChanged("VDRSettings");
            }
        }

        private ICommand addCommand;

        public ICommand AddCommand
        {
            get
            {
                if(addCommand == null)
                {
                    addCommand = new RelayCommand(
                        param => this.add(),
                        param => true);
                }
                return addCommand;
            }
        }

        private void add()
        {
            Settings s = new Settings();
            VDRs.Add(s);
            VDRSettings = s;
        }

        private ICommand removeCommand;

        public ICommand RemoveCommand
        {
            get
            {
                if(removeCommand == null)
                {
                    removeCommand = new RelayCommand(
                        param => this.remove(),
                        param => true);
                }
                return removeCommand;
            }
        }

        private void remove()
        {
            VDRs.Remove(VDRSettings);
            save();
        }

        private ICommand saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if(saveCommand == null)
                {
                    saveCommand = new RelayCommand(
                        param => this.save(),
                        param => true);
                }
                return saveCommand;
            }
        }

        private async void save()
        {
            if (vdrSettings != null)
            {
                if (vdrs.Count(x => x.Name == vdrSettings.Name) > 1)
                {
                    MessageDialog dlg = new MessageDialog(resourceLoader.GetString("SettingNameAlreadyUsed") + " \"" + vdrSettings.Name + "\"");
                    await dlg.ShowAsync();
                    return;
                }
            }
            vdrSettings = null;
            NewSettings = false;
            NotifyPropertyChanged("NewSettings");
            NotifyPropertyChanged("EnableAddButton");
            NotifyPropertyChanged("VDRSettings");
            Configuration.VDRs.Clear();
            foreach(Settings settings in vdrs)
            {
                Configuration.VDRs.Add(settings);
            }
            if (Configuration.VDRs.Count > 0)
            {
                Configuration.VDRs.LastUsedSettings = Configuration.VDRs[Configuration.VDRs.Count - 1];
            }
            else
            {
                Configuration.VDRs.LastUsedSettings = null;
            }
            Configuration.VDRs.Save();
        }

        private ICommand searchProfilesCommand;

        public ICommand SearchProfilesCommand
        {
            get
            {
                if(searchProfilesCommand == null)
                {
                    searchProfilesCommand = new RelayCommand(
                        param => this.searchProfiles(),
                        param => true);
                }
                return searchProfilesCommand;
            }
        }

        private ICommand searchAudioProfilesCommand;

        public ICommand SearchAudioProfilesCommand
        {
            get
            {
                if (searchAudioProfilesCommand == null)
                {
                    searchAudioProfilesCommand = new RelayCommand(
                        param => this.searchAudioProfiles(),
                        param => true);
                }
                return searchAudioProfilesCommand;
            }
        }

        public bool SearchingProfiles { get; private set; }
        public bool SearchingAudioProfiles { get; private set; }

        private async void searchProfiles()
        {
            VDR.VDR vdr = new VDR.VDR(vdrSettings);
            List<string> profiles = null;
            try
            {
                SearchingProfiles = true;
                NotifyPropertyChanged("SearchingProfiles");
                profiles = await vdr.GetProfiles();
                SearchingProfiles = false;
                NotifyPropertyChanged("SearchingProfiles");
            }
            catch(Exception ex)
            {
                SearchingProfiles = false;
                NotifyPropertyChanged("SearchingProfiles");
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += "\r\n" + ex.InnerException.Message;
                }
                LogWriter.WriteToLog(msg);
                LogWriter.WriteLogToFile();
                MessageDialog errdlg = new MessageDialog(msg);
                await errdlg.ShowAsync();
                return;
            }
            if(profiles != null && profiles.Count > 0)
            {
                ProfilesContentDialog profilesDlg = new ProfilesContentDialog(profiles);
                ContentDialogResult result = await profilesDlg.ShowAsync();
                if(result == ContentDialogResult.Primary)
                {
                    vdrSettings.Profile = profilesDlg.SelectedProfile;
                    NotifyPropertyChanged("VDRSettings");
                }
            }

        }

        private async void searchAudioProfiles()
        {
            VDR.VDR vdr = new VDR.VDR(vdrSettings);
            List<string> profiles = null;
            try
            {
                SearchingAudioProfiles = true;
                NotifyPropertyChanged("SearchingAudioProfiles");
                profiles = await vdr.GetProfiles();
                SearchingAudioProfiles = false;
                NotifyPropertyChanged("SearchingAudioProfiles");
            }
            catch (Exception ex)
            {
                SearchingAudioProfiles = false;
                NotifyPropertyChanged("SearchingAudioProfiles");
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += "\r\n" + ex.InnerException.Message;
                }
                LogWriter.WriteToLog(msg);
                LogWriter.WriteLogToFile();
                MessageDialog errdlg = new MessageDialog(msg);
                await errdlg.ShowAsync();
                return;
            }
            if (profiles != null && profiles.Count > 0)
            {
                ProfilesContentDialog profilesDlg = new ProfilesContentDialog(profiles);
                ContentDialogResult result = await profilesDlg.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    vdrSettings.AudioProfile = profilesDlg.SelectedProfile;
                    NotifyPropertyChanged("VDRSettings");
                }
            }

        }
    }
}
