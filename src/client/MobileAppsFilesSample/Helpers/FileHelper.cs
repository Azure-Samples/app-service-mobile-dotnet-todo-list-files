using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PCLStorage;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileAppsFilesSample
{
    public class FileHelper
    {
        public static async Task<string> CopyTodoItemFileAsync(string itemId, string filePath)
        {
            IFolder localStorage = FileSystem.Current.LocalStorage;

            string fileName = Path.GetFileName(filePath);
            string targetPath = await GetLocalFilePathAsync(itemId, fileName);

            var sourceFile = await localStorage.GetFileAsync(filePath);
            var sourceStream = await sourceFile.OpenAsync(FileAccess.Read);

            var targetFile = await localStorage.CreateFileAsync(targetPath, CreationCollisionOption.ReplaceExisting);

            using (var targetStream = await targetFile.OpenAsync(FileAccess.ReadAndWrite)) {
                await sourceStream.CopyToAsync(targetStream);
            }

            return targetPath;
        }

        public static async Task<string> GetLocalFilePathAsync(string itemId, string fileName)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();

            string rootPath = await platform.GetTodoFilesPathAsync();

            var folder = await FileSystem.Current.GetFolderFromPathAsync(rootPath);

            var checkExists = await folder.CheckExistsAsync(itemId);
            if (checkExists == ExistenceCheckResult.NotFound) {
                await folder.CreateFolderAsync(itemId, CreationCollisionOption.ReplaceExisting);
            }

            var fullPath = Path.Combine(rootPath, itemId, fileName);
            return fullPath;
        }

        public static async Task DeleteLocalFileAsync(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile fileName)
        {
            string localPath = await GetLocalFilePathAsync(fileName.ParentId, fileName.Name);
            var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(localPath);

            if (checkExists == ExistenceCheckResult.FileExists) {
                var file = await FileSystem.Current.LocalStorage.GetFileAsync(localPath);
                await file.DeleteAsync();
            }
        }
    }
}