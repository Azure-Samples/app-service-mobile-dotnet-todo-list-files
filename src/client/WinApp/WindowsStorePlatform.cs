using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MobileAppsFilesSample;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;

[assembly: Xamarin.Forms.Dependency(typeof(WinApp.WindowsStorePlatform))]
namespace WinApp
{
    public class WindowsStorePlatform : IPlatform
    {
        public Task DownloadFileAsync<T>(IMobileServiceSyncTable<T> table, MobileServiceFile file, string filename)
        {
            //var path = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name);
            //await table.DownloadFileAsync(file, path);
            throw new NotImplementedException();
        }

        public Task<IMobileServiceFileDataSource> GetFileDataSource(MobileServiceFileMetadata metadata)
        {
            //var filePath = await FileHelper.GetLocalFilePathAsync(metadata.ParentDataItemId, metadata.FileName);
            //return new PathMobileServiceFileDataSource(filePath);
            throw new NotImplementedException();
        }

        public async Task<string> GetTodoFilesPathAsync()
        {
            var root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            var filePath = "TodoItemFiles";

            var storageFolder = await StorageFolder.GetFolderFromPathAsync(root);
            var result = await storageFolder.TryGetItemAsync(filePath);

            if (result == null) {
                result = await storageFolder.CreateFolderAsync(filePath);
            }

            return result.Path;
        }

        public async Task<string> TakePhotoAsync(object context)
        {
            try {
                CameraCaptureUI dialog = new CameraCaptureUI();
                Size aspectRatio = new Size(16, 9);
                dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
                return file.Path;
            }
            catch (TaskCanceledException) {
                return null;
            }
        }
    }
}
