using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Xamarin.Forms;

namespace MobileAppsFilesSample
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
            IPlatform platform = DependencyService.Get<IPlatform>();

            return platform.GetFileDataSource(metadata);
        }

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
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