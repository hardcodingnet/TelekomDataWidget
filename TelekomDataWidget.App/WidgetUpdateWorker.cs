using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Util;
using AndroidX.Work;
using Java.Util.Concurrent;
using TelekomDataWidget.App.Helper;

namespace TelekomDataWidget.App
{
    public class WidgetUpdateWorker : Worker
    {
        #region Fields

        private readonly Context _context;

        #endregion

        #region Methods

        public WidgetUpdateWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
            _context = context;
        }

        public override Result DoWork()
        {
            Log.Debug("WidgetUpdateWorker", "starting worker");

            if (WidgetConnectivity.IsMobileOnlyConnection)
            {
                Log.Debug("WidgetUpdateWorker", "calling web service...");
                DataStore store = DataStore.GetFromWebService().Result;

                if (store != null)
                {
                    Log.Debug("WidgetUpdateWorker", "got valid data");
                    store.SafeToDataFile();

                    WidgetUpdater updater = new WidgetUpdater(_context);
                    updater.Set(store.UsedDataAmountBytes, store.TotalDataAmountBytes, store.DataAmountValidRemainingSeconds);

                    if (store.NextUpdate < DateTime.Now)
                        EnqueueUpdate(600);
                    else
                        EnqueueUpdate((long) store.NextUpdate.Subtract(DateTime.Now).TotalSeconds + 600);

                    Log.Debug("WidgetUpdateWorker", "finished successfully");
                    return Result.InvokeSuccess();
                }

                Log.Debug("WidgetUpdateWorker", "finished retry");
                return Result.InvokeRetry();
            }

            Log.Debug("WidgetUpdateWorker", "finished retry");
            return Result.InvokeRetry();
        }

        private void EnqueueUpdate(long delay)
        {
            Constraints c = new Constraints.Builder()
                .SetRequiredNetworkType(NetworkType.Connected)
                .Build();

            OneTimeWorkRequest req = OneTimeWorkRequest.Builder.From<WidgetUpdateWorker>()
                .SetInitialDelay(delay, TimeUnit.Seconds)
                .SetConstraints(c)
                .SetBackoffCriteria(BackoffPolicy.Linear, TimeSpan.FromMinutes(5))
                .Build();

            WorkManager.Instance.EnqueueUniqueWork("sync", ExistingWorkPolicy.Replace, req);
        }

        #endregion
    }
}