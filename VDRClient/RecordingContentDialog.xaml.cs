using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Inhaltsdialog" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace VDRClient
{
    public sealed partial class RecordingContentDialog : ContentDialog
    {
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        public enum RecordingAction { watch, delete, undelete, remove};

        public RecordingAction SelectedAction { get; private set; }

        private bool deleted;
        public RecordingContentDialog(bool deleted)
        {
            this.InitializeComponent();
            this.deleted = deleted;
            watchRadioButton.Content = resourceLoader.GetString("Watch");
            deleteRadioButton.Content = resourceLoader.GetString("Delete");
            removeRadioButton.Content = resourceLoader.GetString("Remove");
            removeRadioButton.Visibility = Visibility.Collapsed;
            if(deleted)
            {
                watchRadioButton.Visibility = Visibility.Collapsed;
                removeRadioButton.Visibility = Visibility.Visible;
                deleteRadioButton.Content = resourceLoader.GetString("Undelete");
                deleteRadioButton.IsChecked = true;
            }
            else
            {
                watchRadioButton.IsChecked = true;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if(rb != null)
            {
                if (rb.Tag.ToString() == "watch")
                {
                    SelectedAction = RecordingAction.watch;
                }
                else if (rb.Tag.ToString() == "del")
                {
                    if(deleted)
                    {
                        SelectedAction = RecordingAction.undelete;
                    }
                    else
                    {
                        SelectedAction = RecordingAction.delete;
                    }
                }
                else if(rb.Tag.ToString() == "remove")
                {
                    SelectedAction = RecordingAction.remove;
                }
            }
        }
    }
}
