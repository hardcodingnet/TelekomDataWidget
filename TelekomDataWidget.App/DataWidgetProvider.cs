using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Util;
using Android.Widget;
using AndroidX.Work;
using TelekomDataWidget.App.Helper;
using Xamarin.Essentials;

namespace TelekomDataWidget.App
{
    [BroadcastReceiver(Label = "Telekom Datenverbrauch")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/data_widget")]
    public class DataWidgetProvider : AppWidgetProvider
    {
        #region Fields

        public const string ActionUpdateAndToast = "ACTION_UPDATE_TOAST";

        #endregion

        #region Properties

        #endregion

        #region Methods

        public override void OnEnabled(Context context)
        {
            EnqueueUpdate();
        }

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            if (DataStore.IsDataFileAvailable())
            {
                DataStore store = DataStore.GetFromDataFile();

                if (store != null && DateTime.Now < store.DataAmountValidUntil)
                {
                    Log.Debug("DataWidgetProvider", "have valid file");
                    WidgetUpdater updater = new WidgetUpdater(context);
                    updater.Set(store.UsedDataAmountBytes, store.TotalDataAmountBytes, store.DataAmountValidRemainingSeconds);
                }
                else
                    SetWifiView(context, appWidgetManager);
            }
            else
                SetWifiView(context, appWidgetManager);
        }

        public override void OnDisabled(Context context)
        {
            WorkManager.Instance.CancelUniqueWork("sync");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            switch (intent.Action)
            {
                case ActionUpdateAndToast:
                    Log.Debug("DataWidgetProvider", $"{Connectivity.NetworkAccess}, [{String.Join(",", Connectivity.ConnectionProfiles)}]");
                    if (!WidgetConnectivity.IsMobileOnlyConnection)
                    {
                        Toast.MakeText(context, "Mobilfunkverbindung benötigt!", ToastLength.Short).Show();
                        return;
                    }

                    EnqueueUpdate();
                    Toast.MakeText(context, "Daten werden abgerufen...", ToastLength.Short).Show();
                    break;

                default:
                    base.OnReceive(context, intent);
                    break;
            }
        }

        private void SetWifiView(Context context, AppWidgetManager manager)
        {
            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.wifi_view);

            Intent intent = new Intent(context, Java.Lang.Class.FromType(typeof(DataWidgetProvider)));
            intent.SetAction(ActionUpdateAndToast);

            widgetView.SetOnClickPendingIntent(Resource.Id.wifi_view, PendingIntent.GetBroadcast(context, 0, intent, 0));

            ComponentName thisWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(DataWidgetProvider)).Name);
            manager.UpdateAppWidget(thisWidget, widgetView);
        }

        private void EnqueueUpdate()
        {
            Constraints c = new Constraints.Builder()
                .SetRequiredNetworkType(NetworkType.Connected)
                .Build();

            var reqBuilder = OneTimeWorkRequest.Builder.From<WidgetUpdateWorker>()
                .SetBackoffCriteria(BackoffPolicy.Linear, TimeSpan.FromMinutes(5))
                .SetConstraints(c);

            OneTimeWorkRequest req = reqBuilder.Build();

            WorkManager.Instance.EnqueueUniqueWork("sync", ExistingWorkPolicy.Replace, req);
        }

        #endregion
    }
}