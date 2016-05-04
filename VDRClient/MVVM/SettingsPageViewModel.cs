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


        public SettingsPageViewModel()
        {
            protocols = new List<string>();
            protocols.Add("http://");
            protocols.Add("https://");
        }

        private List<string> protocols;

        public List<string> Protocols
        {
            get { return protocols; }
        }

        public bool NewSettings { get; set; }

        public VDRList VDRs
        {
            get
            {
                return VDR.Configuration.VDRs;
            }
        }

        private Settings vdrSettings;

        public Settings VDRSettings
        {
            get { return vdrSettings; }
            set
            {
                if (value != vdrSettings)
                {
                    vdrSettings = value;
                    NewSettings = true;
                    NotifyPropertyChanged("NewSettings");
                    NotifyPropertyChanged("VDRSettings");
                }
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

        private void save()
        {
            vdrSettings = null;
            NewSettings = false;
            NotifyPropertyChanged("NewSettings");
            NotifyPropertyChanged("VDRSettings");
            VDRs.Save();
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

        public bool SearchingProfiles { get; private set; }

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
                MessageDialog errdlg = new MessageDialog(ex.Message);
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
    }
}
