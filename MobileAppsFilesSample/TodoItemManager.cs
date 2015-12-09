// To add offline sync support: add the NuGet package WindowsAzure.MobileServices.SQLiteStore
// to all projects in the solution and uncomment the symbol definition OFFLINE_SYNC_ENABLED
// For Xamarin.iOS, also edit AppDelegate.cs and uncomment the call to SQLitePCL.CurrentPlatform.Init()
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342 
#define OFFLINE_SYNC_ENABLED

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.IO;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Xamarin.Forms;

namespace MobileAppsFilesSample
{
    public partial class TodoItemManager
    {
        MobileServiceClient client;

        IMobileServiceSyncTable<TodoItem> todoTable;

        private TodoItemManager() { }

        public static async Task<TodoItemManager> CreateAsync()
        {
            var result = new TodoItemManager();
			result.client = new MobileServiceClient(Constants.ApplicationURL, new LoggingHandler(true));

            var store = new MobileServiceSQLiteStore("localstore2.db");
            store.DefineTable<TodoItem>();

            // FILES: Initialize file sync
            result.client.InitializeFileSyncContext(new TodoItemFileSyncHandler(result), store);

            //Initializes the SyncContext using the default IMobileServiceSyncHandler.
            await result.client.SyncContext.InitializeAsync(store, StoreTrackingOptions.AllNotifications);

            result.todoTable = result.client.GetSyncTable<TodoItem>();

            result.client.EventManager.Subscribe<StoreOperationCompletedEvent>(result.GeneralEventHandler);

            return result;
        }

        private void GeneralEventHandler(StoreOperationCompletedEvent mobileServiceEvent)
        {
            Debug.WriteLine("Event handled: " + mobileServiceEvent.Operation.RecordId);
        }

        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try {
//                await this.client.SyncContext.PushAsync();

                // FILES: Push file changes
                await this.todoTable.PushFileChangesAsync();

                // FILES: Automatic pull
                // A normal pull will automatically process new/modified/deleted files, engaging the file sync handler
                await this.todoTable.PullAsync("todoItems", this.todoTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc) {
                if (exc.PushResult != null) {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null) {
                foreach (var error in syncErrors) {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null) {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }
                }
            }
        }

        public async Task<IEnumerable<TodoItem>> GetTodoItemsAsync()
        {
            try {
                return await todoTable.OrderBy(item => item.Name).ToListAsync();
            }
            catch (MobileServiceInvalidOperationException msioe) {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e) {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        public async Task SaveTaskAsync(TodoItem item)
        {
            if (item.Id == null) {
                await todoTable.InsertAsync(item);
            }
            else {
                await todoTable.UpdateAsync(item);
            }
        }

        public async Task DeleteTaskAsync(TodoItem item)
        {
            try {
                //TodoViewModel.TodoItems.Remove(item);
                await todoTable.DeleteAsync(item);
            }
            catch (MobileServiceInvalidOperationException msioe) {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e) {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
        }

        internal async Task DownloadFileAsync(MobileServiceFile file)
        {
            var todoItem = await todoTable.LookupAsync(file.ParentId);
            Debug.WriteLine ("++ Downloading file: " + todoItem.Name);

            IPlatform platform = DependencyService.Get<IPlatform>();

            string filePath = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name); 
            await platform.DownloadFileAsync(this.todoTable, file, filePath);
        }

        internal async Task<MobileServiceFile> AddImage(TodoItem todoItem, string imagePath)
        {
            string targetPath = await FileHelper.CopyTodoItemFileAsync(todoItem.Id, imagePath);

            // FILES: Creating/Adding file
            MobileServiceFile file = await this.todoTable.AddFileAsync(todoItem, Path.GetFileName(targetPath));

            // "Touch" the record to mark it as updated
            await this.todoTable.UpdateAsync(todoItem);

            return file;
        }

        internal async Task DeleteImage(TodoItem todoItem, MobileServiceFile file)
        {
            // FILES: Deleting file
            await this.todoTable.DeleteFileAsync(file);

            // "Touch" the record to mark it as updated
            await this.todoTable.UpdateAsync(todoItem);
        }

        internal async Task<IEnumerable<MobileServiceFile>> GetImageFilesAsync(TodoItem todoItem)
        {
            // FILES: Get files (local)
            return await this.todoTable.GetFilesAsync(todoItem);
        }
    }
}
