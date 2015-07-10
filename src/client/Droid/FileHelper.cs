using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace MobileAppsFilesSample.Droid
{
    public class FileHelper
    {
        public static string CopyTodoItemFile(string itemId, string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            string targetPath = GetLocalFilePath(itemId, fileName);

            File.Copy(filePath, targetPath);

            return targetPath;
        }

        public static string GetLocalFilePath(string itemId, string fileName)
        {
            string recordFilesPath = Path.Combine(GetFilesDirectory(), itemId);
            
            if (!Directory.Exists(recordFilesPath))
            {
                Directory.CreateDirectory(recordFilesPath);
            }

            return Path.Combine(recordFilesPath, fileName);
        }

        public static void DeleteLocalFile(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile file)
        {
            string localPath = GetLocalFilePath(file.ParentId, file.Name);

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }
        }

        private static string GetFilesDirectory()
        {
            string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TodoItemFiles");

            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            return filesPath;
        }
    }
}