using System;
using System.Collections.Generic;
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

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace OfficePoint
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            StartCheckLocationAccess();
        }

        private async void StartCheckLocationAccess()
        {
            bool isAllowed = await Core.Helpers.LocationHelper.CheckLocationAccessAsync();

            allowLocationToggle.IsOn = isAllowed;
            allowLocationToggle.IsEnabled = true;
        }

        private void OnAllowLocationTapped(object sender, TappedRoutedEventArgs e)
        {
            AuthorizeLocationAsync(sender as ToggleSwitch);
        }

        private async void AuthorizeLocationAsync(ToggleSwitch toggle)
        {
            toggle.IsEnabled = false;

            var currentLocation = await Core.Helpers.LocationHelper.GetCurrentLocationAsync();

            toggle.IsEnabled = true;
        }


        //this method is goint to walk u through Onedrive authentication process
        private async void StartOneDriveAuthorization(Button button)
        {
            button.IsEnabled = false;

            Core.Authorization.OneDriveCredentialInformation credentials = Core.Helpers.SettingsHelper.GetOneDriveCredentials();

            if (credentials == null)
            {
                credentials = await Core.Helpers.AuthorizationHelper.InitializeOneDriveUserAsync();


                //AccessToken and RefereshToken are very important  that's why in enterpise application tokens expire very quickly 
                Core.Helpers.SettingsHelper.SaveOneDriveCredentials(credentials.AccessToken, credentials.RefreshToken);
            }

            button.IsEnabled = credentials == null;

        }

        private void OnAuthorizeOneDriveButtonClick(object sender, RoutedEventArgs e)
        {
            StartOneDriveAuthorization(sender as Button);

        }

        private void OnAuthorizeSharePointClick(object sender, RoutedEventArgs e)
        {
            StartSharePointAuthorization(sender as Button);
        }

        private async void StartSharePointAuthorization(Button button)
        {
            bool isAuthorized = false;
            button.IsEnabled = false;

            string siteUrl = sharePointSiteUrlTextBox.Text.Trim();
            string userName = sharePointUserNameTextBox.Text.Trim();
            string password = sharePointPasswordTextBox.Password.Trim();

            Core.Authorization.SharePointCredentialInformation credentials = Core.Helpers.SettingsHelper.GetSharePointCredentials();

            if (credentials == null)
            {
                isAuthorized = await Core.Helpers.ContextHelper.ValidateCredentialsAsync(siteUrl, userName, password);

                if (isAuthorized)
                {
                    Core.Helpers.SettingsHelper.SaveSharePointCredentials(siteUrl, userName, password);
                }

            }

            button.IsEnabled = credentials == null;
        }

        private void OnAuthorizeAzureNotificationsClick(object sender, RoutedEventArgs e)
        {
            StartAzureNotificationAuthorization(sender as Button);
        }

        private async void StartAzureNotificationAuthorization(Button button)
        {
            button.IsEnabled = false;

            var contactId = await Core.Helpers.ChannelManager.GetContactIdAsync();

            UserInformation user = new UserInformation()
            {
                ContactId = contactId,
                UserId = new Guid(contactId),
                Region = "US",
            };

            Core.Helpers.ChannelManager.RegisterChannel(user);

            button.IsEnabled = true;
        }
    }
}
