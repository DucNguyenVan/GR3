using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneApp1.Resources;
using System.IO.IsolatedStorage;
using Windows.Media.Editing;
using System.Windows.Threading;
using Windows.Storage;
using Windows.Media.Transcoding;
using System.Diagnostics;
using System.IO;
using Windows.Media.MediaProperties;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Streams;
using Windows.Foundation;

using System.Reflection;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Phone.Tasks;
using System.Threading;
using Windows.Storage.FileProperties;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Info;


namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        const int TIME_PER_IMAGE = 3;
        MediaClip clip;
        public static MediaComposition mediaComposition;
        public static int videoIndexSelected = 0;
      //  public StorageFile fileUpload;
     //   Stream videoStream;
    //    MediaTranscoder transcoder = new MediaTranscoder();
        //private enum VideoState
        //{
        //    AllowEditing, Playing
        //}
        public enum PageName
        {
            MainPage, MusicPage, SavePage, UploadPage, PreviewPage, FramePage
        }
        public struct MyVideo
        {
            public StorageFile file;
            public Image image;
            public bool isFrame;
            public int time;
        };
        public static List<MyVideo> videoList;
        public static PageName pageName;
        private IEnumerable<string> supportedFileTypes = new List<string> { ".mp4", ".jpg" };
       // private IEnumerable<string> supportedMusicFileTypes = new List<string> { ".mp3" };
        private readonly VideoClips videoClips;
        //public static List<Image> imageList = new List<Image>();
        public static List<Image> imageList;
     //   List<StorageFile> storageFileList = new List<StorageFile>();
        int currentSelectedIndex = 0;
        bool isLow;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.InitializeComponent();
            gr_swap.Visibility = Visibility.Collapsed;
            imageList = new List<Image>();
            //GlobalSettings.ReadState();
            //read da bao gom ca addToList
            GlobalSettings.ReadThumbnail();
            GlobalSettings.ReadClipName();
            //  this.NavigationCacheMode = NavigationCacheMode.Required;
            var app = Application.Current as App;
          //  videoStream = new System.IO.MemoryStream();
            mediaComposition = new MediaComposition();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            //  IsolatedStorageFile
            videoClips = new VideoClips();
            videoList = new List<MyVideo>();
            //khoi tao la videoIndex = lastIndex cua videopropertylist
            //sau do neu ktra la edit thi gan lai la index cua edit
            videoIndexSelected = GlobalSettings.videoPropertyList.Count;
            if (WelcomePage.isEdit)
            {
                GlobalSettings.isNewShow = false;
                videoIndexSelected = WelcomePage.currentSelectedIndex;
                SetMediaComposition(MainPage.videoIndexSelected);
            }
            DataContext = videoClips;
            isLow = IsLowMemDevice;
            Debug.WriteLine("Low mem:" + isLow);
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var app = App.Current as App;
            if (app.FilePickerContinuationArgs != null && pageName == PageName.MainPage)
            {
                this.ContinueFileOpenPicker(app.FilePickerContinuationArgs);
            }
            //after add new FRAME, re-show listbox
            if (pageName == PageName.FramePage)
            {
                ListBox1.ItemsSource = null;
                ListBox1.ItemsSource = imageList;
            }

        }

        private void btn_addClip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Add event handler implementation here.
            pageName = PageName.MainPage;
            FileOpenPicker openPicker = new FileOpenPicker();
            foreach (var type in supportedFileTypes)
            {
                openPicker.FileTypeFilter.Add(type);
            }
            //  openPicker.PickSingleFileAndContinue();
            openPicker.PickMultipleFilesAndContinue();

        }

        private void btn_music_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Music.xaml", UriKind.RelativeOrAbsolute));
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (
                  args.Files != null &&
                args.Files.Count > 0)
            {
                foreach (StorageFile file in args.Files)
                {
                   // fileUpload = file;
                    // ProcessFile(file);
                    if (file.FileType != ".mp4")
                    {
                        // ProcessFileAsImage(file);
                        clip = await MediaClip.CreateFromImageFileAsync(file, TimeSpan.FromSeconds(TIME_PER_IMAGE));
                        mediaComposition.Clips.Add(clip);
                        // Create thumbnail
                        var bitmap = new BitmapImage();
                        Image img = new Image();
                        bitmap.UriSource = new Uri(file.Path);
                        img.Source = bitmap;
                        AddMoreImageToList(img);
                        AddVideoToList(file, img, true, TIME_PER_IMAGE);
                    }
                    else
                    {
                        //   ProcessFileAsVideo(file);
                        clip = await MediaClip.CreateFromFileAsync(file);
                        mediaComposition.Clips.Add(clip);
                        // Create thumbnail
                        var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
                        Stream stream = thumbnail.AsStream();
                        var image = new BitmapImage();
                        Image img = new Image();
                        img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        //image.SetSource(thumbnail);
                        image.SetSource(stream);
                        Thickness margin = img.Margin;
                        margin.Left = 4;
                        margin.Right = 4;
                        margin.Top = 4;
                        margin.Bottom = 4;
                        img.Margin = margin;
                        img.Source = image;
                        // Add to our viewmodel
                        videoClips.Add(new VideoClip(clip, image, file.Name));
                        //AddMoreFileToList(file);
                        AddMoreImageToList(img);
                        AddVideoToList(file, img);
                    }

                }
                ListBox1.ItemsSource = null;
                ListBox1.ItemsSource = imageList;
            }
        }

        private void ProcessFile(StorageFile file)
        {
            if (file.FileType != ".mp4")
            {
                ProcessFileAsImage(file);
            }
            else
                ProcessFileAsVideo(file);
        }

        private async void ProcessFileAsVideo(StorageFile file)
        {
            clip = await MediaClip.CreateFromFileAsync(file);
            mediaComposition.Clips.Add(clip);
            // Create thumbnail
            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
            Stream stream = thumbnail.AsStream();
            var image = new BitmapImage();
            Image img = new Image();
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //image.SetSource(thumbnail);
            image.SetSource(stream);
            Thickness margin = img.Margin;
            margin.Left = 4;
            margin.Right = 4;
            margin.Top = 4;
            margin.Bottom = 4;
            img.Margin = margin;
            img.Source = image;
            // Add to our viewmodel
            videoClips.Add(new VideoClip(clip, image, file.Name));
          //  AddMoreFileToList(file);
            AddMoreImageToList(img);
            AddVideoToList(file, img);
        }

        private async void ProcessFileAsImage(StorageFile file)
        {
            clip = await MediaClip.CreateFromImageFileAsync(file, TimeSpan.FromSeconds(TIME_PER_IMAGE));
            mediaComposition.Clips.Add(clip);
            // Create thumbnail
            var bitmap = new BitmapImage();
            Image img = new Image();
            bitmap.UriSource = new Uri(file.Path);
            img.Source = bitmap;
            AddMoreImageToList(img);
            AddVideoToList(file, img, true, TIME_PER_IMAGE);
        }

        public static void AddMoreImageToList(Image img)
        {
            imageList.Add(img);
            //  Preview.previewImageList.Add(img);
        }

        //private void AddMoreFileToList(StorageFile file)
        //{
        //    storageFileList.Add(file);
        //}

        public static void AddVideoToList(StorageFile file, Image image)
        {
            MyVideo myVideo = new MyVideo();
            myVideo.file = file;
            myVideo.image = image;
            videoList.Add(myVideo);
        }

        public static void AddVideoToList(StorageFile file, Image image, bool isFrame, int time)
        {
            MyVideo myVideo = new MyVideo();
            myVideo.file = file;
            myVideo.image = image;
            myVideo.isFrame = isFrame;
            myVideo.time = time;
            videoList.Add(myVideo);
        }

        void SelectionThumbnailHandle(object sender, SelectionChangedEventArgs args)
        {
            // ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            var lbi = sender as ListBox;
            if (lbi.SelectedIndex >= 0)
                currentSelectedIndex = lbi.SelectedIndex;
            //textBlock1.Text = "   You selected " + lbi.Content.ToString() + ".";
            ShowNavigationBar(true);
        }

        private void SwapVideo(int x, int y)
        {

            //swap to save file later
            MyVideo temp = new MyVideo();
            temp = videoList[x];
            videoList[x] = videoList[y];
            videoList[y] = temp;

            //swap to preview
            Image tmpImg = new Image();
            tmpImg = imageList[x];
            imageList[x] = imageList[y];
            imageList[y] = tmpImg;
            ListBox1.ItemsSource = null;
            ListBox1.ItemsSource = imageList;
        }

        //private void btn_Read_Click(object sender, RoutedEventArgs e)
        //{
        //    var shareFile = SaveVideo.videoStorageFile;
        //    if (shareFile != null)
        //        ShowShareMediaTask(shareFile.Path);

        //}
        void ShowShareMediaTask(string path)
        {
            ShareMediaTask shareMediaTask = new ShareMediaTask();
            shareMediaTask.FilePath = path;
            shareMediaTask.Show();
        }

        private async void ArrangeVideoToComposition()
        {
            mediaComposition.Clips.Clear();
            //save composition
            Composition comp = new Composition();
            foreach (MyVideo video in videoList)
            {
                MediaClip clip;
                int tempTime = 0;
                if (video.isFrame)
                {
                    Debug.WriteLine("add frame to video" + video.time);
                    clip = await MediaClip.CreateFromImageFileAsync(video.file, System.TimeSpan.FromSeconds(video.time));
                    tempTime = video.time;
                }
                else
                    clip = await MediaClip.CreateFromFileAsync(video.file);
                mediaComposition.Clips.Add(clip);
                ClipFileType tempClip = new ClipFileType();
                tempClip.clipPath = video.file.Path;
                tempClip.time = tempTime;
                comp.clipNameList.Add(tempClip);
            }
            comp.lenght = MainPage.mediaComposition.Clips.Count;
            if (WelcomePage.isEdit)
            {
                Debug.WriteLine("Arrange edit");
                comp.id = MainPage.videoIndexSelected + 1;
                GlobalSettings.ChangeValueOfCompositionAt(comp, MainPage.videoIndexSelected);
                GlobalSettings.WriteClipName();
            }
            else
            {
                Debug.WriteLine("Arrange new");
                comp.id = GlobalSettings.videoPropertyList.Count + 1;
                GlobalSettings.AddCompositionToList(comp);
                GlobalSettings.WriteClipName();
            }
        }

        private void ShowSelectedComposition(int index)
        {

        }

        private async void SetMediaComposition(int index)
        {
            foreach (ClipFileType clipName in GlobalSettings.compositionList[index].clipNameList)
            {
                //dame: PicturesLibrary, VideosLibrary, SavedPictures
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(clipName.clipPath);
                    MediaClip clip;
                    Debug.WriteLine("File type:" + file.FileType + file.Name);
                    if (file.FileType != ".mp4" && clipName.time > 0)
                    {
                        // Debug.WriteLine("add frame to video" + video.time);
                        clip = await MediaClip.CreateFromImageFileAsync(file, System.TimeSpan.FromSeconds(clipName.time));
                        var bitmap = new BitmapImage();
                        Image img = new Image();
                        bitmap.UriSource = new Uri(file.Path);
                        img.Source = bitmap;
                        AddMoreImageToList(img);
                        AddVideoToList(file, img, true, TIME_PER_IMAGE);
                    }
                    else
                    {
                        clip = await MediaClip.CreateFromFileAsync(file);
                        var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
                        Stream stream = thumbnail.AsStream();
                        var image = new BitmapImage();
                        Image img = new Image();
                        img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        //image.SetSource(thumbnail);
                        image.SetSource(stream);
                        Thickness margin = img.Margin;
                        margin.Left = 4;
                        margin.Right = 4;
                        margin.Top = 4;
                        margin.Bottom = 4;
                        img.Margin = margin;
                        img.Source = image;
                        // Add to our viewmodel
                        videoClips.Add(new VideoClip(clip, image, file.Name));
                       // AddMoreFileToList(file);
                        AddMoreImageToList(img);
                        AddVideoToList(file, img);
                    }
                    mediaComposition.Clips.Add(clip);
                }
                catch (FileNotFoundException)
                {
                    Debug.WriteLine("clip not found");
                }
                ListBox1.ItemsSource = null;
                ListBox1.ItemsSource = imageList;
            }
        }

        public static bool IsLowMemDevice
        {
            get
            {
                if (!isLowMemDevice.HasValue)
                {
                    try
                    {
                        // check the working set limit 
                        long result = (long)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");
                        isLowMemDevice = result < 94371840L;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // OS does not support this call => indicates a 512 MB device
                        isLowMemDevice = false;
                    }
                }
                return isLowMemDevice.Value;
            }
        }
        private static bool? isLowMemDevice;

        private void ShowNavigationBar(bool isShow)
        {
            if (isShow)
            {
                gr_swap.Visibility = Visibility.Visible;
            }
            else
            {
                gr_swap.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_preview_Click(object sender, RoutedEventArgs e)
        {
            //tai 1 life circle, chi 1 listbox duoc add 1 item duy nhat. Neu listbox khac cung muon add item do vao,thi se gay ra loi.
            // vi vay phai clear 1 listbox roi gan vao listbox khac
            ListBox1.ItemsSource = null;
            if (mediaComposition.Clips.Count != 0)
                ArrangeVideoToComposition();
            NavigationService.Navigate(new Uri("/Preview.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btn_prew_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedIndex <= 0) return;
            SwapVideo(currentSelectedIndex, --currentSelectedIndex);
            ListBox1.SelectedIndex = currentSelectedIndex;

        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedIndex >= imageList.Count - 1) return;
            SwapVideo(currentSelectedIndex, ++currentSelectedIndex);
            ListBox1.SelectedIndex = currentSelectedIndex;
        }

        private void btn_first_Click(object sender, RoutedEventArgs e)
        {
            SwapVideo(currentSelectedIndex, 0);
            currentSelectedIndex = 0;
            ListBox1.SelectedIndex = currentSelectedIndex;
        }

        private void btn_last_Click(object sender, RoutedEventArgs e)
        {
            SwapVideo(currentSelectedIndex, imageList.Count - 1);
            currentSelectedIndex = imageList.Count - 1;
            ListBox1.SelectedIndex = currentSelectedIndex;
        }

        private void btn_newVideo_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.isNewShow = true;
            ListBox1.ItemsSource = null;
            pageName = MainPage.PageName.MainPage;
           // mediaComposition.Clips.Clear();
            imageList.Clear();
            videoList.Clear();
        }

        private void btn_frame_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Frame.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            //if (this.NavigationService.CanGoBack)
            //{
            //    this.NavigationService.GoBack();
            //}
            NavigationService.Navigate(new Uri("/WelcomePage.xaml", UriKind.RelativeOrAbsolute));
        }


    }
}