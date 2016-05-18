using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class NewTimerPage : Page
    {
        private MVVM.NewTimerPageViewModel viewModel;
        public MVVM.NewTimerPageViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                viewModel = value;
                DataContext = viewModel;
            }
        }

        public NewTimerPage()
        {
            this.InitializeComponent();
            this.ViewModel = new MVVM.NewTimerPageViewModel(VDR.Configuration.VDRs.LastUsedSettings);
            this.ViewModel.NavigateBack = NewTimerPage_BackRequested;
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += NewTimerPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= NewTimerPage_BackRequested;
            base.OnNavigatedFrom(e);
        }

        private void NewTimerPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (base.Frame.CanGoBack)
            {
                if(e != null)
                    e.Handled = true;
                base.Frame.GoBack();
            }
        }
    }
}
