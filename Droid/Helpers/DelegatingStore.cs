using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace MobileAppsFilesSample.Droid.Helpers
{
    public class MobileServiceSQLiteStoreWithLogging : MobileServiceSQLiteStore
    {
        private bool logResults;
        private bool logParameters;
        private bool loggingEnabled;

        public event EventHandler<ItemChangedEventArgs> ItemChanged;

        public MobileServiceSQLiteStoreWithLogging(string fileName, bool loggingEnabled = false, bool logResults = false, bool logParameters = false)
            : base(fileName)
        {
            this.loggingEnabled = loggingEnabled;
            this.logResults = logResults;
            this.logParameters = logParameters;
        }

        public async override Task UpsertAsync(string tableName, IEnumerable<Newtonsoft.Json.Linq.JObject> items, bool ignoreMissingColumns)
        {

            if (ignoreMissingColumns && !tableName.StartsWith("__")) // This flag indicates an upsert operation from the server
            {
                foreach (var item in items)
                {
                    string id = item["id"].ToString();

                    var currentRecord = await base.LookupAsync(tableName, id);

                    // We only want to trigger a change notification if the record has been modified
                    if (currentRecord == null || string.Compare(currentRecord[MobileServiceSystemColumns.Version].ToString(), item[MobileServiceSystemColumns.Version].ToString()) != 0)
                    {
                        await base.UpsertAsync(tableName, new[] { item }, ignoreMissingColumns);

                        OnItemChanged(new ItemChangedEventArgs(id, tableName, ItemChangeType.AddedOrUpdated));
                    }
                }
            }
            else
            {
                await base.UpsertAsync(tableName, items, ignoreMissingColumns);
            }
        }

        public async override Task DeleteAsync(string tableName, IEnumerable<string> ids)
        {
            await base.DeleteAsync(tableName, ids);

            foreach (var id in ids)
            {
                OnItemChanged(new ItemChangedEventArgs(id, tableName, ItemChangeType.Deleted));
            }
        }

        private void OnItemChanged(ItemChangedEventArgs itemChangedEventArgs)
        {
            var temp = ItemChanged;
            if (temp != null)
            {
                temp(this, itemChangedEventArgs);
            }
        }
        protected override IList<Newtonsoft.Json.Linq.JObject> ExecuteQuery(string tableName, string sql, IDictionary<string, object> parameters)
        {
            if (loggingEnabled)
            {
                Console.WriteLine(sql);
            }

            if (loggingEnabled && logParameters)
                PrintDictionary(parameters);

            var result = base.ExecuteQuery(tableName, sql, parameters);

            if (loggingEnabled && logResults && result != null)
            {
                foreach (var token in result)
                    Console.WriteLine(token);
            }

            return result;
        }

        protected override void ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            if (loggingEnabled)
            {
                Console.WriteLine(sql);
            }

            if (loggingEnabled && logParameters)
                PrintDictionary(parameters);

            base.ExecuteNonQuery(sql, parameters);
        }

        private void PrintDictionary(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                return;

            foreach (var pair in dictionary)
                Console.WriteLine("{0}:{1}", pair.Key, pair.Value);
        }
    }


    public class ItemChangedEventArgs : EventArgs
    {
        public ItemChangedEventArgs(string itemId, string tableName, ItemChangeType changeType)
        {
            this.ItemId = itemId;
            this.TableName = tableName;
            this.ChangeType = changeType;
        }

        public string ItemId { get; set; }

        public string TableName { get; set; }

        public ItemChangeType ChangeType { get; set; }
    }

    public enum ItemChangeType
    {
        AddedOrUpdated,
        Deleted
    }


    public class LoggingHandler : DelegatingHandler
    {
        private bool logRequestResponseBody;

        public LoggingHandler(bool logRequestResponseBody = false)
        {
            this.logRequestResponseBody = logRequestResponseBody;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Console.WriteLine("Request: {0} {1}", request.Method, request.RequestUri.ToString());

            if (logRequestResponseBody && request.Content != null)
            {
                var requestContent = await request.Content.ReadAsStringAsync();
                Console.WriteLine(requestContent);
            }
            
            Console.WriteLine("HEADERS");

            foreach (var header in request.Headers)
            {
                Console.WriteLine(string.Format("{0}:{1}", header.Key, string.Join(",", header.Value)));
            }

            var response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response: {0}", response.StatusCode);

            if (logRequestResponseBody)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }

            return response;
        }
    }
}