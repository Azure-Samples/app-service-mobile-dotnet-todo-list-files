using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;

namespace donnam_testforms
{
    public class TodoItemFileSyncHandler : IFileSyncHandler
    {
        private readonly TodoItemManager todoItemManager;

        public TodoItemFileSyncHandler(TodoItemManager itemManager)
        {
            this.todoItemManager = itemManager;
        }

        public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            IMobileServiceFileDataSource source = new PathMobileServiceFileDataSource(FileHelper.GetLocalFilePathAsync(metadata.ParentDataItemId, metadata.FileName));

            return Task.FromResult(source);
        }

        public async Task ProcessFileSynchronizationAction(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                await FileHelper.DeleteLocalFileAsync(file);
            }
            else // Create or update. We're aggressively downloading all files.
            {
                await this.todoItemManager.DownloadFileAsync(file);
            }
        }
    }
}