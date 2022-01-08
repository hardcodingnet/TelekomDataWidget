using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TelekomDataWidget.App
{
    public sealed class DataStore
    {
        #region Fields

        private const string DataServiceUrl = "http://pass.telekom.de/api/service/generic/v1/status";
        private const string DataFileName = "data.json";

        #endregion

        #region Properties

        public long UsedDataAmountBytes { get; }
        public long TotalDataAmountBytes { get; }
        public DateTime NextUpdate { get; }
        public DateTime LastUpdated { get; }
        public DateTime DataAmountValidUntil { get; }
        public long DataAmountValidRemainingSeconds { get; }

        #endregion

        #region Methods

        public DataStore(string jsonData)
        {
            JObject data = JObject.Parse(jsonData);

            UsedDataAmountBytes = data["usedVolume"].Value<long>();
            TotalDataAmountBytes = data["initialVolume"].Value<long>();
            LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(data["usedAt"].Value<long>());
            NextUpdate = LastUpdated.AddSeconds(data["nextUpdate"].Value<long>());
            DataAmountValidUntil = DateTime.Now.AddSeconds(data["remainingSeconds"].Value<long>());
            DataAmountValidRemainingSeconds = data["remainingSeconds"].Value<long>();
        }

        [JsonConstructor]
        public DataStore(long usedDataAmountBytes, long totalDataAmountBytes, DateTime nextUpdate, DateTime lastUpdated, DateTime dataAmountValidUntil, long dataAmountValidRemainingSeconds)
        {
            UsedDataAmountBytes = usedDataAmountBytes;
            TotalDataAmountBytes = totalDataAmountBytes;
            NextUpdate = nextUpdate;
            LastUpdated = lastUpdated;
            DataAmountValidUntil = dataAmountValidUntil;
            DataAmountValidRemainingSeconds = dataAmountValidRemainingSeconds;
        }

        public void SafeToDataFile()
        {
            var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), DataFileName);
            FileInfo file = new FileInfo(fileName);

            using (var writer = file.CreateText())
            {
                writer.Write(JsonConvert.SerializeObject(this));
            }
        }

        public static async Task<DataStore> GetFromWebService()
        {
            try
            {
                Log.Debug("DataStore", "trying to get web data...");
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0");
                    var response = await client.GetAsync(DataServiceUrl);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Log.Verbose("DataStore", $"got web data: {responseContent}");

                    return new DataStore(responseContent);
                }
            }
            catch(Exception ex)
            {
                Log.Debug("DataStore", ex.ToString());
                return null;
            }
        }

        public static bool IsDataFileAvailable()
        {
            var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), DataFileName);
            return new FileInfo(fileName).Exists;
        }

        public static DataStore GetFromDataFile()
        {
            var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), DataFileName);
            FileInfo file = new FileInfo(fileName);

            try
            {
                using (var reader = file.OpenText())
                {
                    return JsonConvert.DeserializeObject<DataStore>(reader.ReadToEnd());
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}