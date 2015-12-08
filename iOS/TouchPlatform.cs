using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xamarin.Media;

[assembly: Xamarin.Forms.Dependency(typeof(MobileAppsFilesSample.iOS.TouchPlatform))]
namespace MobileAppsFilesSample.iOS
{
    class TouchPlatform : IPlatform
    {
        public async Task DownloadFileAsync<T>(IMobileServiceSyncTable<T> table, MobileServiceFile file, string filename)
        {
            var path = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name);
            await table.DownloadFileAsync(file, path);
        }

        public async Task<IMobileServiceFileDataSource> GetFileDataSource(MobileServiceFileMetadata metadata)
        {
            var filePath = await FileHelper.GetLocalFilePathAsync(metadata.ParentDataItemId, metadata.FileName);
            return new PathMobileServiceFileDataSource(filePath);
        }

        public async Task<string> TakePhotoAsync(object context)
        {
            var mediaPicker = new MediaPicker();
            var mediaFile = await mediaPicker.PickPhotoAsync();
            return mediaFile.Path;
        }

        public string GetTodoFilesPath()
        {
            string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TodoItemFiles");

            if (!Directory.Exists(filesPath)) {
                Directory.CreateDirectory(filesPath);
            }

            return filesPath;
        }
    }
}
