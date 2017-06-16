using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Xamarin.Forms;

namespace ccmobileapppoc
{
    public class VideoItemFileSyncHandler : IFileSyncHandler
    {
        private readonly VideoItemManager todoItemManager;

        public VideoItemFileSyncHandler(VideoItemManager itemManager)
        {
            this.todoItemManager = itemManager;
        }

        /// <summary>
        /// Get the Sata Soruce
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();
            return platform.GetFileDataSource(metadata);
        }

        /// <summary>
        /// File Sync
        /// </summary>
        /// <param name="file"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                await FileHelper.DeleteLocalFileAsync(file);
            }
            else
            { 
                // downloading all files.
                await this.todoItemManager.DownloadFileAsync(file);
            }
        }
    }
}
