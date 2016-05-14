using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace VDRClient.MVVM
{
    public class RecordingsPageViewModel : INotifyPropertyChanged
    {
        public class RecordingListItem
        {
            public string Name { get; set; }
            public bool DeletedRecordings { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                RecordingListItem itm = (RecordingListItem)obj;
                if (itm == null)
                    return false;
                if (itm.Name == this.Name && itm.DeletedRecordings == this.DeletedRecordings)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (Name + DeletedRecordings.ToString()).GetHashCode();
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

        private List<VDR.Recording> recordings;
        public List<VDR.Recording> Recordings
        {
            get { return recordings; }
            set
            {
                recordings = value;
                NotifyPropertyChanged("Recordings");
            }
        }

        private VDR.Recording selectedRecording;
        public VDR.Recording SelectedRecording
        {
            get { return selectedRecording; }
            set
            {
                selectedRecording = value;
                NotifyPropertyChanged("SelectedRecording");
            }
        }

        private List<RecordingListItem> recordingList;
        public List<RecordingListItem> RecordingList
        {
            get { return recordingList; }
            set
            {
                recordingList = value;
                NotifyPropertyChanged("RecordingList");
            }
        }

        private RecordingListItem selectedRecordingList;
        public RecordingListItem SelectedRecordingList
        {
            get { return selectedRecordingList; }
            set
            {
                selectedRecordingList = value;
                SetRecordings(selectedRecordingList.DeletedRecordings);
                NotifyPropertyChanged("SelectedRecordingList");
            }
        }

        public delegate void ViewRecordingDelegate(VDR.Recording rec);
        public ViewRecordingDelegate ViewRecording;

        private VDR.Settings settings;
        private VDR.VDR vdr;
        public RecordingsPageViewModel(VDR.Settings settings)
        {
            this.settings = settings;
            this.vdr = new VDR.VDR(this.settings);
            List<RecordingListItem> rlist = new List<RecordingListItem>();
            rlist.Add(new RecordingListItem { Name = resourceLoader.GetString("Recordings"), DeletedRecordings = false });
            rlist.Add(new RecordingListItem { Name = resourceLoader.GetString("DeletedRecordings"), DeletedRecordings = true });
            RecordingList = rlist;
            SelectedRecordingList = RecordingList[0];
        }

        private async void SetRecordings(bool del)
        {
            try
            {
                Recordings = await vdr.GetRecordings(del);
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
            if (SelectedRecording != null)
            {
                RecordingContentDialog dlg = new RecordingContentDialog(SelectedRecordingList.DeletedRecordings);
                ContentDialogResult result = await dlg.ShowAsync();
                if(result == ContentDialogResult.Primary)
                {
                    RecordingContentDialog.RecordingAction action = dlg.SelectedAction;
                    if (action == RecordingContentDialog.RecordingAction.delete)
                    {
                        bool deleted = false;
                        try
                        {
                            deleted = await vdr.DeleteRecording(SelectedRecording);
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
                        Debug.WriteLine("Deleted = " + deleted.ToString());
                    }
                    else if (action == RecordingContentDialog.RecordingAction.remove)
                    {
                        bool removed = false;
                        try
                        {
                            removed = await vdr.RemoveRecording(SelectedRecording);
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
                        Debug.WriteLine("Removed = " + removed.ToString());

                    }
                    else if(action == RecordingContentDialog.RecordingAction.undelete)
                    {
                        bool undeleted = false;
                        try
                        {
                            undeleted = await vdr.UndeleteRecording(SelectedRecording);
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
                        Debug.WriteLine("Undeleted = " + undeleted.ToString());
                    }
                    else if(action == RecordingContentDialog.RecordingAction.watch)
                    {
                        if(ViewRecording != null)
                        {
                            ViewRecording(SelectedRecording);
                        }
                    }
                    SetRecordings(SelectedRecordingList.DeletedRecordings);
                }
                SelectedRecording = null;
            }
        }

    }
}
