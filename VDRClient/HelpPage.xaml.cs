using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public sealed partial class HelpPage : Page
    {
        public HelpPage()
        {
            this.InitializeComponent();
            showLog();
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += HelpPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= HelpPage_BackRequested;
            base.OnNavigatedFrom(e);
        }

        private void HelpPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (base.Frame.CanGoBack)
            {
                e.Handled = true;
                base.Frame.GoBack();
            }
        }

        private async void showLog()
        {
            string log = await LogWriter.ReadLogFile();
            this.LogTextBox.Text = log;
        }

        private async void DeleteLogButton_Click(object sender, RoutedEventArgs e)
        {
            await LogWriter.DeleteLogfile();
            showLog();
        }

        private async void MailLinkButton_Click(object sender, RoutedEventArgs e)
        {
            string to = "vdrclient@s51.org";
            string subject = "Support VDR Client";
            try
            {
                StorageFile file = await LogWriter.GetLogFile();
                var rastream = RandomAccessStreamReference.CreateFromFile(file);
                EmailMessage mail = new EmailMessage();
                mail.To.Add(new EmailRecipient(to));
                mail.Subject = subject;
                mail.Attachments.Add(new EmailAttachment(file.Name, rastream));
                //mail.CC = , mail.Bcc = , mail.Attachments =
                await EmailManager.ShowComposeNewEmailAsync(mail);
            }
            catch { }
        }
    }
}
