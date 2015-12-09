using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xamarin.Media;

[assembly: Xamarin.Forms.Dependency(typeof(MobileAppsFilesSample.Droid.DroidPlatform))]
namespace MobileAppsFilesSample.Droid
{
    public class DroidPlatform : IPlatform
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
            var uiContext = context as Context;
            if (uiContext != null) {
                var mediaPicker = new MediaPicker(uiContext);
                var photo = await mediaPicker.TakePhotoAsync(new StoreCameraMediaOptions());

                return photo.Path;
            }

            return null;
        }

        public string GetTodoFilesPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filesPath = Path.Combine(appData, "TodoItemFiles");

            if (!Directory.Exists(filesPath)) {
                Directory.CreateDirectory(filesPath);
            }

            return filesPath;
        }
    }
}