using System;
using System.Linq;
using Xamarin.Essentials;

namespace TelekomDataWidget.App.Helper
{
    public static class WidgetConnectivity
    {
        #region Fields

        #endregion

        #region Properties

        public static bool IsMobileOnlyConnection => Connectivity.NetworkAccess == NetworkAccess.Internet && Connectivity.ConnectionProfiles.Contains(ConnectionProfile.Cellular) && !Connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi);

        #endregion

        #region Methods

        #endregion
    }
}