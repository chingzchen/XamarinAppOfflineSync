/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342
 */
#define OFFLINE_SYNC_ENABLED

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
#endif

using System.IO;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Eventing;

namespace ccmobileapppoc
{
    public partial class VideoItemManager
    {
        static VideoItemManager defaultInstance = new VideoItemManager();
        MobileServiceClient client;

#if OFFLINE_SYNC_ENABLED
        IMobileServiceSyncTable<VideoItem> videoITemTable;
#else
        IMobileServiceTable<TodoItem> todoTable;
#endif

        const string offlineDbPath = @"localstudyrecord.db";

        private VideoItemManager()
        {
            this.client = new MobileServiceClient(Constants.ApplicationURL);
           


#if OFFLINE_SYNC_ENABLED
            var store = new MobileServiceSQLiteStore(offlineDbPath);
            store.DefineTable<VideoItem>();

            // Initialize file sync
            this.client.InitializeFileSyncContext(new VideoItemFileSyncHandler(this), store);

            //Initializes the SyncContext using the default IMobileServiceSyncHandler.
            //this.client.SyncContext.InitializeAsync(store);
            this.client.SyncContext.InitializeAsync(store, StoreTrackingOptions.NotifyLocalAndServerOperations);


            this.videoITemTable = client.GetSyncTable<VideoItem>();
#else
            this.todoTable = client.GetTable<TodoItem>();
#endif
        }

        public static VideoItemManager DefaultManager
        {
            get
            {
                return defaultInstance;
            }
            private set
            {
                defaultInstance = value;
            }
        }

        public MobileServiceClient CurrentClient
        {
            get { return client; }
        }

        public bool IsOfflineEnabled
        {
            get { return videoITemTable is Microsoft.WindowsAzure.MobileServices.Sync.IMobileServiceSyncTable<VideoItem>; }
        }

        public async Task<ObservableCollection<VideoItem>> GetVideoItemsAsync(bool syncItems = false)
        {
            try
            {
#if OFFLINE_SYNC_ENABLED
                if (syncItems)
                {
                    await this.SyncAsync();
                }
#endif
                IEnumerable<VideoItem> items = await videoITemTable
                    .Where(videoItem => !videoItem.Done)
                    .ToEnumerableAsync();

                return new ObservableCollection<VideoItem>(items);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"Invalid sync operation: {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        public async Task SaveTaskAsync(VideoItem item)
        {
            if (item.Id == null)
            {
                await videoITemTable.InsertAsync(item);
            }
            else
            {
                await videoITemTable.UpdateAsync(item);
            }
        }

#if OFFLINE_SYNC_ENABLED
        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                await this.client.SyncContext.PushAsync();

                //add file push async
                await this.videoITemTable.PushFileChangesAsync();

                await this.videoITemTable.PullAsync(
                    "allVideoItems",
                    this.videoITemTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"Exception: {0}", ex.Message);
            }
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard the change
                        await error.CancelAndDiscardItemAsync();
                    }

                    Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }
        }

        internal async Task DownloadFileAsync(MobileServiceFile file)
        {
            var todoItem = await videoITemTable.LookupAsync(file.ParentId);
            IPlatform platform = DependencyService.Get<IPlatform>();

            string filePath = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name);
            await platform.DownloadFileAsync(this.videoITemTable, file, filePath);
        }

        internal async Task<MobileServiceFile> AddImage(VideoItem todoItem, string imagePath)
        {
            string targetPath = await FileHelper.CopyVideoItemFileAsync(todoItem.Id, imagePath);
            return await this.videoITemTable.AddFileAsync(todoItem, Path.GetFileName(targetPath));
        }

        internal async Task DeleteImage(VideoItem todoItem, MobileServiceFile file)
        {
            await this.videoITemTable.DeleteFileAsync(file);
        }

        internal async Task<IEnumerable<MobileServiceFile>> GetImageFilesAsync(VideoItem todoItem)
        {
            return await this.videoITemTable.GetFilesAsync(todoItem);
        }
#endif
    }
}
