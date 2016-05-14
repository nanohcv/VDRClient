using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;

namespace VDRClient.MVVM
{
    public class ViewRecordingPageViewModel : INotifyPropertyChanged
    {
        public class JumpOption
        {
            public string Name { get; private set; }
            public int Seconds { get; private set; }

            public JumpOption(string name, int seconds)
            {
                Name = name;
                Seconds = seconds;
            }

            public override string ToString()
            {
                return Name;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                JumpOption jo = (JumpOption)obj;
                if (jo == null)
                    return false;
                if (jo.Seconds == this.Seconds)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return Seconds.ToString().GetHashCode();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        private List<JumpOption> jumpOptions;
        public List<JumpOption> JumpOptions
        {
            get { return jumpOptions; }
            set
            {
                jumpOptions = value;
                NotifyPropertyChanged("JumpOptions");
            }
        }

        private JumpOption selectedJumpOption;
        public JumpOption SelectedJumpOption
        {
            get { return selectedJumpOption; }
            set
            {
                selectedJumpOption = value;
                NotifyPropertyChanged("SelectedJumpOption");
            }
        }

        private int jumpTo = 0;
        public TimeSpan CurrentPosition { get; set; }

        private ICommand jumpCommand;

        public ICommand JumpCommand
        {
            get
            {
                if (jumpCommand == null)
                {
                    jumpCommand = new RelayCommand(
                        param => this.JumpTo(),
                        param => true
                    );
                }
                return jumpCommand;
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

        private VDR.Settings settings;
        private VDR.VDR vdr;
        private VDR.Recording recording;
        public VDR.Recording Recording
        {
            get { return recording; }
            set
            {
                recording = value;
                NotifyPropertyChanged("Recording");
            }
        }
        public ViewRecordingPageViewModel(VDR.Settings settings, VDR.Recording recording)
        {
            this.settings = settings;
            Recording = recording;
            vdr = new VDR.VDR(settings);
            List<JumpOption> jos = new List<JumpOption>();
            jos.Add(new JumpOption("+60 " + resourceLoader.GetString("minutes"), 3600));
            jos.Add(new JumpOption("+30 " + resourceLoader.GetString("minutes"), 1800));
            jos.Add(new JumpOption("+15 " + resourceLoader.GetString("minutes"), 900));
            jos.Add(new JumpOption("+10 " + resourceLoader.GetString("minutes"), 600));
            jos.Add(new JumpOption("+5 " + resourceLoader.GetString("minutes"), 300));
            jos.Add(new JumpOption("+3 " + resourceLoader.GetString("minutes"), 180));
            jos.Add(new JumpOption("+2 " + resourceLoader.GetString("minutes"), 120));
            jos.Add(new JumpOption("+1 " + resourceLoader.GetString("minute"), 60));
            jos.Add(new JumpOption("+30 " + resourceLoader.GetString("seconds"), 30));
            jos.Add(new JumpOption("-30 " + resourceLoader.GetString("seconds"), -30));
            jos.Add(new JumpOption("-1 " + resourceLoader.GetString("minute"), -60));
            jos.Add(new JumpOption("-2 " + resourceLoader.GetString("minutes"), -120));
            jos.Add(new JumpOption("-3 " + resourceLoader.GetString("minutes"), -180));
            jos.Add(new JumpOption("-5 " + resourceLoader.GetString("minutes"), -300));
            jos.Add(new JumpOption("-10 " + resourceLoader.GetString("minutes"), -600));
            jos.Add(new JumpOption("-15 " + resourceLoader.GetString("minutes"), -900));
            jos.Add(new JumpOption("-30 " + resourceLoader.GetString("minutes"), -1800));
            jos.Add(new JumpOption("-60 " + resourceLoader.GetString("minutes"), -3600));
            JumpOptions = jos;
            SelectedJumpOption = JumpOptions[4];
            Stream();
        }

        private void JumpTo()
        {
            jumpTo += SelectedJumpOption.Seconds;
            Stream();
        }

        private void Stream()
        {
            jumpTo = (int)CurrentPosition.TotalSeconds + jumpTo;
            if (jumpTo < 0)
                jumpTo = 0;
            string url = settings.BaseURL + "recstream.ts?filename=" + WebUtility.UrlEncode(recording.FileName) + "&preset=" + settings.Profile + "&start=" + jumpTo;
            MediaSource = new Uri(url);
        }
    }
}
