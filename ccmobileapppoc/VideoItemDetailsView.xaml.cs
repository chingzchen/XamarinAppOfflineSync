using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices.Files;

namespace ccmobileapppoc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoItemDetailsView : ContentPage
    {
        private VideoItemManager manager;

        public VideoItem videoItem { get; set; }
        public ObservableCollection<VideoItemImage> Images { get; set; }

        public VideoItemDetailsView(VideoItem videoItem, VideoItemManager manager)
        {
            InitializeComponent();
            this.Title = videoItem.Name;

            this.videoItem = videoItem;
            this.manager = manager;

            this.Images = new ObservableCollection<VideoItemImage>();
            this.BindingContext = this;
        }

        public async Task LoadImagesAsync()
        {
            IEnumerable<MobileServiceFile> files = await this.manager.GetImageFilesAsync(videoItem);
            this.Images.Clear();

            foreach (var f in files)
            {
                var todoImage = new VideoItemImage(f, this.videoItem);
                this.Images.Add(todoImage);
            }
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            IPlatform mediaProvider = DependencyService.Get<IPlatform>();
            string sourceImagePath = await mediaProvider.TakePhotoAsync(App.UIContext);

            if (sourceImagePath != null)
            {
                MobileServiceFile file = await this.manager.AddImage(this.videoItem, sourceImagePath);

                var image = new VideoItemImage(file, this.videoItem);
                this.Images.Add(image);
            }
        }
    }
}
