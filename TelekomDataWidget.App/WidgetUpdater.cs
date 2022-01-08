using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TelekomDataWidget.App
{
    public class WidgetUpdater
    {
        #region Types

        private class TrafficAmount
        {
            public string Value { get; }
            public string Unit { get; }

            public TrafficAmount(string value, string unit)
            {
                Value = value;
                Unit = unit;
            }
        }

        #endregion

        #region Fields

        private readonly Context _context;

        #endregion

        #region Methods

        public WidgetUpdater(Context context)
        {
            _context = context;
        }

        public void Set(long used, long total, long remainingSeconds)
        {
            long totalSeconds = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) * 24 * 60 * 60;

            TrafficAmount usedAmount = Format(used, 2, true);
            TrafficAmount totalAmount = Format(total, 1, false);

            AppWidgetManager manager = AppWidgetManager.GetInstance(_context);
            ComponentName thisWidget = new ComponentName(_context, Java.Lang.Class.FromType(typeof(DataWidgetProvider)).Name);

            var appWidgetIds = manager.GetAppWidgetIds(thisWidget);

            foreach (int id in appWidgetIds)
            {
                var widgetView = GetWidgetRemoteViews(used, total, remainingSeconds, usedAmount, totalAmount, totalSeconds,
                    new Android.Graphics.Color(ContextCompat.GetColor(_context, Resource.Color.blackTranslucent)),
                    new Android.Graphics.Color(ContextCompat.GetColor(_context, Resource.Color.colorAccent)),
                    new Android.Graphics.Color(ContextCompat.GetColor(_context, Resource.Color.colorPrimary)));

                manager.UpdateAppWidget(id, widgetView);
            }

            Log.Debug("WidgetUpdater", $"updated widget: {usedAmount.Value} {usedAmount.Unit} / {totalAmount.Value} {totalAmount.Unit}");
        }

        private RemoteViews GetWidgetRemoteViews(long used, long total, long remainingSeconds, TrafficAmount usedAmount, TrafficAmount totalAmount, long totalSeconds, Color backgroundColor, Color dataGaugeColor, Color timeGaugeColor)
        {
            var widgetView = new RemoteViews(_context.PackageName, Resource.Layout.image_view);

            DataGaugeView v = new DataGaugeView(_context);

            v.BackgroundColor = backgroundColor;
            v.DataGaugeColor = dataGaugeColor;
            v.TimeGaugeColor = timeGaugeColor;

            v.UsedAmoutText = usedAmount.Value;
            v.UsedAmoutUnitText = usedAmount.Unit;
            v.TotalAmoutText = totalAmount.Value;
            v.TotalAmoutUnitText = totalAmount.Unit;

            v.UsedAmountPercent = used / (float) total * 100f;
            v.TimePercent = (totalSeconds - remainingSeconds) / (float) totalSeconds * 100f;

            int layoutSize = 500;
            var measure = View.MeasureSpec.MakeMeasureSpec(layoutSize, MeasureSpecMode.Exactly);
            v.Measure(measure, measure);
            v.Layout(0, 0, layoutSize, layoutSize);

            Bitmap bitmap = Bitmap.CreateBitmap(layoutSize, layoutSize, Bitmap.Config.Argb8888);
            v.Draw(new Canvas(bitmap));

            widgetView.SetImageViewBitmap(Resource.Id.image_view_image, bitmap);

            Intent intent = new Intent(_context, Java.Lang.Class.FromType(typeof(DataWidgetProvider)));
            intent.SetAction(DataWidgetProvider.ActionUpdateAndToast);

            widgetView.SetOnClickPendingIntent(Resource.Id.image_view, PendingIntent.GetBroadcast(_context, 0, intent, 0));
            return widgetView;
        }

        private static TrafficAmount Format(long byteNumber, int decimals, bool exactly)
        {
            const long KB = 1024;
            const long MB = 1024 * 1024;
            const long GB = 1024 * 1024 * 1024;

            if (byteNumber < MB)
                return new TrafficAmount(((int)Math.Round((double)byteNumber / KB)).ToString(), "KB");

            if (byteNumber < GB)
                return new TrafficAmount(((int) Math.Round((double)byteNumber / MB)).ToString(), "MB");

            var gigBytes = Math.Round((double)byteNumber/GB, decimals);
            string formatString = $"{{0:0.{new String(exactly ? '0' :'#', decimals)}}}";

            if (gigBytes > 10)
                formatString = exactly ? $"{{0:00.0}}" : $"{{0:00.#}}";

            return new TrafficAmount(string.Format(formatString, gigBytes).Replace(',', '.'), "GB");
        }

        #endregion
    }
}