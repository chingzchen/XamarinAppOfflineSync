using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xamarin.Media;

[assembly: Xamarin.Forms.Dependency(typeof(ccmobileapppoc.iOS.TouchPlatform))]
namespace ccmobileapppoc.iOS
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
            try
            {
                var mediaPicker = new MediaPicker();
                var mediaFile = await mediaPicker.PickPhotoAsync();
                return mediaFile.Path;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public Task<string> GetTodoFilesPathAsync()
        {
            string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TodoItemFiles");

            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            return Task.FromResult(filesPath);
        }
    }

}