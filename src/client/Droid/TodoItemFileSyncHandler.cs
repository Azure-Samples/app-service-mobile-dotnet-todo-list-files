using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.Mobile.Files;

namespace MobileAppsFilesSample.Droid
{
    public class TodoItemFileSyncHandler : IFileSyncHandler
    {
        private readonly TodoItemManager todoItemManager;

        public TodoItemFileSyncHandler(TodoItemManager itemManager)
        {
            this.todoItemManager = itemManager;
        }

        public Task<IMobileServiceFileDataSource> GetDataSource(Microsoft.WindowsAzure.MobileServices.Files.Metadata.MobileServiceFileMetadata metadata)
        {
            IMobileServiceFileDataSource source = new PathMobileServiceFileDataSource(FileHelper.GetLocalFilePath(metadata.ParentDataItemId, metadata.FileName));

            return Task.FromResult(source);
        }

        public async Task ProcessFileSynchronizationAction(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                FileHelper.DeleteLocalFile(file);
            }
            else // Create or update. We're aggressively downloading all files.
            {
                await this.todoItemManager.DownloadFileAsync(file);
            }
        }
    }
}