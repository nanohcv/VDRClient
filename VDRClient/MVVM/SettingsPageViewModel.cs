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
    }
}
