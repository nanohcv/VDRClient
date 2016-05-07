using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace VDRClient
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            if(!VDR.Configuration.VDRs.Load())
            {
                this.mainFrame.Navigate(typeof(SettingsPage), this);
            }
            else
            {
                this.mainFrame.Navigate(typeof(TVPage), this);
            }
        }

        public void FullScreen(bool fullscreen)
        {
            if (fullscreen)
            {
                row1.Height = new GridLength(0);
                mainSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                mainSplitView.IsPaneOpen = false;
            }
            else
            {
                row1.Height = GridLength.Auto;
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    mainSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    mainSplitView.IsPaneOpen = false;
                }
                else
                {
                    mainSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                }
            }
        }

        private void hamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = this.mainSplitView.IsPaneOpen ? false : true;
        }

        private void SettingsButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.mainFrame.Navigate(typeof(SettingsPage), this);
            this.SettingsButton.IsChecked = false;
        }

        private void TVButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.mainFrame.Navigate(typeof(TVPage), this);
            this.TVButton.IsChecked = false;
        }

        private async void SwitchButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            SettingsContentDialog dlg = new SettingsContentDialog(VDR.Configuration.VDRs);
            ContentDialogResult result = await dlg.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                VDR.Configuration.VDRs.LastUsedSettings = dlg.SelectedSetting;
                VDR.Configuration.VDRs.Save();
                this.mainFrame.Navigate(typeof(TVPage), this);
            }
            this.SwitchButton.IsChecked = false;
        }

        private void EPGButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.mainFrame.Navigate(typeof(EPGPage), this);
            this.EPGButton.IsChecked = false;
        }
    }
}
