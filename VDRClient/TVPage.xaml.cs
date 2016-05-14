using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class TVPage : Page
    {
        private MainPage mainpage;
        private MVVM.TVPageViewModel viewModel;
        public MVVM.TVPageViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                DataContext = viewModel;
            }
        }

        private DispatcherTimer pointer_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };

        private bool mobileViewState = false;
        SimpleOrientationSensor sensor;
        InputPane inputPane;

        public TVPage()
        {
            this.InitializeComponent();
            sensor = SimpleOrientationSensor.GetDefault();
            inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;
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
                catch(Exception ex)
                {
                    string msg = ex.Message;
                    if(ex.InnerException != null)
                    {
                        msg += "\r\n" + ex.InnerException.Message;
                    }
                    LogWriter.WriteToLog(msg);
                    LogWriter.WriteLogToFile();
                    MessageDialog dlg = new MessageDialog(msg);
                    await dlg.ShowAsync();
                    return;

                }
                if(xmlapiversion < VDR.Configuration.MinXmlApiVersion)
                {
                    MessageDialog dlg = new MessageDialog("Your XMLAPI Plugin is too old for this App!\r\nYour XMLAPI version = " + xmlapiversion.ToString() + "\r\nRequired version = " + VDR.Configuration.MinXmlApiVersion.ToString());
                    await dlg.ShowAsync();
                    return;
                }
                ViewModel = new MVVM.TVPageViewModel(VDR.Configuration.VDRs.LastUsedSettings);
                ViewModel.HandleMobileView = HandleMobileView;
            }
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            channelsGrid.Height = double.NaN;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            channelsGrid.Height = Windows.UI.Xaml.Window.Current.Bounds.Height - sender.OccludedRect.Height;
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainpage = (MainPage)e.Parameter;
            BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += TVPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= TVPage_BackRequested;
            base.OnNavigatedFrom(e);
        }

        private void TVPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (mobileViewState)
            {
                mediaPlayer.IsFullScreen = false;
                col1.Width = new GridLength(0);
                col2.Width = new GridLength(1, GridUnitType.Star);
                mediaPlayer.Source = null;
                mobileViewState = false;
                e.Handled = true;
                if (sensor != null)
                    sensor.OrientationChanged -= Sensor_OrientationChanged;

            }
            else
            {
                App.Current.Exit();
            }
        }

        private void mediaPlayer_IsFullScreenChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (mediaPlayer.IsFullScreen)
            {
                mainpage.FullScreen(true);
                viewBox.Margin = new Thickness(0, 0, 0, 0);
                col1.Width = new GridLength(1, GridUnitType.Star);
                col2.Width = new GridLength(0);
                row1.Height = new GridLength(1, GridUnitType.Star);
                row2.Height = new GridLength(0);
                mainGrid.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Landscape;
                }
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                pointer_timer.Tick += Pointer_timer_Tick;
                this.PointerMoved += TVPage_PointerMoved;
                pointer_timer.Start();

            }
            else
            {
                mainpage.FullScreen(false);
                mainGrid.Background = new SolidColorBrush(Color.FromArgb(255, 5, 0, 53));
                viewBox.Margin = new Thickness(10, 10, 10, 0);
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    col1.Width = new GridLength(1, GridUnitType.Star);
                    col2.Width = new GridLength(0);
                    row1.Height = GridLength.Auto;
                    row2.Height = new GridLength(1, GridUnitType.Star);
                    Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Portrait;
                }
                else
                {
                    col1.Width = new GridLength(1, GridUnitType.Star);
                    col2.Width = new GridLength(450);
                    row1.Height = GridLength.Auto;
                    row2.Height = new GridLength(1, GridUnitType.Star);
                }
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                pointer_timer.Tick -= Pointer_timer_Tick;
                this.PointerMoved -= TVPage_PointerMoved;
                pointer_timer.Stop();
            }
        }

        private void TVPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            pointer_timer.Start();
        }

        private void Pointer_timer_Tick(object sender, object e)
        {
            Window.Current.CoreWindow.PointerCursor = null;
        }

        private void HandleMobileView()
        {
            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
            {
                col1.Width = new GridLength(1, GridUnitType.Star);
                col2.Width = new GridLength(0);
                row1.Height = GridLength.Auto;
                row2.Height = new GridLength(1, GridUnitType.Star);
                mobileViewState = true;
                if (sensor != null)
                    sensor.OrientationChanged += Sensor_OrientationChanged;
            }
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
    }
}
