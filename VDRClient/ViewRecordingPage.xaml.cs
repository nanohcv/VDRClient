using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
    public sealed partial class ViewRecordingPage : Page
    {
        private MVVM.ViewRecordingPageViewModel viewModel;
        public MVVM.ViewRecordingPageViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                viewModel = value;
                DataContext = viewModel;
            }
        }

        private DispatcherTimer pointer_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        SimpleOrientationSensor sensor;
        public ViewRecordingPage()
        {
            this.InitializeComponent();
            sensor = SimpleOrientationSensor.GetDefault();
            if (sensor != null)
                sensor.OrientationChanged += Sensor_OrientationChanged;
        }

        private async void Sensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            if (args.Orientation == SimpleOrientation.Rotated90DegreesCounterclockwise)
            {

                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mediaPlayer.IsFullScreen = true;
                });
            }
            else
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mediaPlayer.IsFullScreen = false;
                });
            }
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            VDR.Recording rec = e.Parameter as VDR.Recording;
            ViewModel = new MVVM.ViewRecordingPageViewModel(VDR.Configuration.VDRs.LastUsedSettings, rec);
            BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += ViewRecordingPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= ViewRecordingPage_BackRequested;
            base.OnNavigatedFrom(e);
        }

        private void ViewRecordingPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (sensor != null)
            {
                mediaPlayer.IsFullScreen = false;
                sensor.OrientationChanged -= Sensor_OrientationChanged;
            }
            if (base.Frame.CanGoBack)
            {
                e.Handled = true;
                base.Frame.GoBack();
            }
        }

        private void mediaPlayer_IsFullScreenChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if(mediaPlayer.IsFullScreen)
            {
                JumpOptionsGrid.Visibility = Visibility.Collapsed;
                TitleGrid.Visibility = Visibility.Collapsed;
                Global.MainPage.FullScreen(true);
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Landscape;
                }
                pointer_timer.Tick += Pointer_timer_Tick;
                this.PointerMoved += ViewRecordingPage_PointerMoved;
                pointer_timer.Start();
            }
            else
            {
                JumpOptionsGrid.Visibility = Visibility.Visible;
                TitleGrid.Visibility = Visibility.Visible;
                Global.MainPage.FullScreen(false);
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Portrait;
                }
                pointer_timer.Tick -= Pointer_timer_Tick;
                this.PointerMoved -= ViewRecordingPage_PointerMoved;
                pointer_timer.Stop();
            }
        }

        private void ViewRecordingPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            pointer_timer.Start();
        }

        private void Pointer_timer_Tick(object sender, object e)
        {
            Window.Current.CoreWindow.PointerCursor = null;
        }
    }
}
