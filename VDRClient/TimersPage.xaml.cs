﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace VDRClient
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class TimersPage : Page
    {
        private MVVM.TimersViewModel viewModel;
        public MVVM.TimersViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                viewModel = value;
                this.DataContext = viewModel;
            }
        }

        public TimersPage()
        {
            this.InitializeComponent();
            SetViewModel();
        }

        private async void SetViewModel()
        {
            if (VDR.Configuration.VDRs.LastUsedSettings != null)
            {
                VDR.VDR vdr = new VDR.VDR(VDR.Configuration.VDRs.LastUsedSettings);
                VDR.XmlApiVersion xmlapiversion = null;
                try
                {
                    xmlapiversion = await vdr.GetXMLAPIVersion();
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
                    MessageDialog dlg = new MessageDialog(msg);
                    await dlg.ShowAsync();
                    return;

                }
                if (xmlapiversion < VDR.Configuration.MinXmlApiVersion)
                {
                    MessageDialog dlg = new MessageDialog("Your XMLAPI Plugin is too old for this App!\r\nYour XMLAPI version = " + xmlapiversion.ToString() + "\r\nRequired version = " + VDR.Configuration.MinXmlApiVersion.ToString());
                    await dlg.ShowAsync();
                    return;
                }
                this.ViewModel = new MVVM.TimersViewModel(VDR.Configuration.VDRs.LastUsedSettings);
            }
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += TimersPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= TimersPage_BackRequested;
            base.OnNavigatedFrom(e);
        }

        private void TimersPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (base.Frame.CanGoBack)
            {
                e.Handled = true;
                base.Frame.GoBack();
            }
        }
    }
}
