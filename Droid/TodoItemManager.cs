using System;
using Microsoft.WindowsAzure.MobileServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using MobileAppsFilesSample.Droid;
using System.IO;
using MobileAppsFilesSample.Droid.Helpers;

namespace MobileAppsFilesSample
{
    /// <summary>
    /// Manager classes are an abstraction on the data access layers
    /// </summary>
    public class TodoItemManager
    {
        // Azure
        IMobileServiceSyncTable<TodoItem> todoTable;
        MobileServiceClient client;

        public TodoItemManager()
        {
            client = new MobileServiceClient(
                Constants.ApplicationURL,
                Constants.GatewayURL,
                Constants.ApplicationKey, new LoggingHandler(true));
            
            var store = new MobileServiceSQLiteStoreWithLogging("localstore.db", true, true, true);
            store.ItemChanged += StoreItemChanged;
            store.DefineTable<TodoItem>();

            // FILES: Initialize file sync
            this.client.InitializeFileSync(new TodoItemFileSyncHandler(this), store);

            //Initializes the SyncContext using the default IMobileServiceSyncHandler.
            this.client.SyncContext.InitializeAsync(store);

            this.todoTable = client.GetSyncTable<TodoItem>();
        }

        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                // FILES: Push file changes
                await this.todoTable.PushFileChangesAsync();

                // FILES: Automatic pull
                // A normal pull will automatically process new/modified/deleted files, engaging the file sync handler
                await this.todoTable.PullAsync("todoItems", this.todoTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }
                }
            }
        }

        public async Task<TodoItem> GetTaskAsync(string id)
        {
            try
            {
                return await todoTable.LookupAsync(id);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        public async Task<IEnumerable<TodoItem>> GetTodoItemsAsync()
        {
            try
            {
                return await todoTable.ReadAsync();
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        public async Task SaveTaskAsync(TodoItem item)
        {
            if (item.Id == null)
            {
                await todoTable.InsertAsync(item);
                //TodoViewModel.TodoItems.Add(item);
            }
            else
                await todoTable.UpdateAsync(item);
        }

        public async Task DeleteTaskAsync(TodoItem item)
        {
            try
            {
                //TodoViewModel.TodoItems.Remove(item);
                await todoTable.DeleteAsync(item);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
        }

        internal async Task DownloadFileAsync(MobileServiceFile file)
        {
            await this.todoTable.DownloadFileAsync(file, FileHelper.GetLocalFilePath(file.ParentId, file.Name));
        }

        internal async Task<MobileServiceFile> AddImage(TodoItem todoItem, string imagePath)
        {
            string targetPath = FileHelper.CopyTodoItemFile(todoItem.Id, imagePath);

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

        internal async Task<IEnumerable<MobileServiceFile>> GetImageFiles(TodoItem todoItem)
        {
            // FILES: Get files (local)
            return await this.todoTable.GetFilesAsync(todoItem);
        }

        private async void StoreItemChanged(object sender, ItemChangedEventArgs e)
        {
            if (string.Compare(e.TableName, "TodoItem") == 0)
            {
                TodoItem todo = await this.todoTable.LookupAsync(e.ItemId);

                if (todo != null)
                {
                    if (e.ChangeType == ItemChangeType.AddedOrUpdated)
                    {
                        // Retrieve files
                        await this.todoTable.PullFilesAsync(todo);
                    }
                    else if (e.ChangeType == ItemChangeType.Deleted)
                    {
                        // Purge all files
                        await this.todoTable.PurgeFilesAsync(todo);
                    }
                }
            }
        }
    }
}

