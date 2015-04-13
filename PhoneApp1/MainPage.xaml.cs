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


namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        MediaClip clip;
        DispatcherTimer posTimer;
        public static MediaComposition mediaComposition;
        private bool isPlaying;
        StorageFile sourceFile;
        public StorageFile fileUpload;
        Stream videoStream;
        MediaTranscoder transcoder = new MediaTranscoder();
        Windows.Storage.StorageFile _InputFile = null;
        Windows.Storage.StorageFile _OutputFile = null;
        public static IsolatedStorageFile isf;
        public static IsolatedStorageFileStream isfs;
        private enum VideoState
        {
            AllowEditing, Playing
        }
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
        public static List<MyVideo> videoList = new List<MyVideo>();
        private VideoState currentState;
        public static PageName pageName;
        private IEnumerable<string> supportedFileTypes = new List<string> { ".mp4" };
        private IEnumerable<string> supportedMusicFileTypes = new List<string> { ".mp3" };
        private readonly VideoClips videoClips;
        public static List<Image> imageList = new List<Image>();
        List<StorageFile> storageFileList = new List<StorageFile>();
        int currentSelectedIndex = 0;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.InitializeComponent();
            GlobalSettings.ReadState();
            //read da bao gom ca addToList
            GlobalSettings.ReadThumbnail();
            GlobalSettings.ReadClipName();
            Debug.WriteLine(pageName);
            //  this.NavigationCacheMode = NavigationCacheMode.Required;
            var app = Application.Current as App;
            videoStream = new System.IO.MemoryStream();
            mediaComposition = new MediaComposition();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            //  IsolatedStorageFile
            videoClips = new VideoClips();
            DataContext = videoClips;

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
            //after add new frame, re-show listbox
            //TEST
            // ShowSelectedComposition(Preview.selectedShow);
            ListBox1.ItemsSource = null;
            ListBox1.ItemsSource = imageList;

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

        private void btn_Save_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //ReArrange Video in VideoList
            ArrangeVideoToComposition(false);
            NavigationService.Navigate(new Uri("/SaveVideo.xaml", UriKind.RelativeOrAbsolute));
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (
                  args.Files != null &&
                args.Files.Count > 0)
            {
                foreach (StorageFile file in args.Files)
                {
                    fileUpload = file;
                    Debug.WriteLine(fileUpload.Path);
                    clip = await MediaClip.CreateFromFileAsync(fileUpload);
                    sourceFile = fileUpload;
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
                    margin.Left = 13;
                    img.Margin = margin;
                    img.Source = image;
                    // stackPanelContainer.Children.Add(img);

                    // Add to our viewmodel
                    videoClips.Add(new VideoClip(clip, image, file.Name));
                    AddMoreFileToList(file);
                    AddMoreImageToList(img);
                    AddVideoToList(file, img);
                }
                ListBox1.ItemsSource = null;
                ListBox1.ItemsSource = imageList;
            }
        }

        private void btn_Upload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Upload.xaml", UriKind.RelativeOrAbsolute));
        }

        public static void AddMoreImageToList(Image img)
        {
            imageList.Add(img);
            //  Preview.previewImageList.Add(img);
        }

        private void AddMoreFileToList(StorageFile file)
        {
            storageFileList.Add(file);
        }

        public static void AddVideoToList(StorageFile file, Image image)
        {
            MyVideo myVideo = new MyVideo();
            myVideo.file = file;
            myVideo.image = image;
            videoList.Add(myVideo);
            Debug.WriteLine("video list false:" + videoList.Count);
        }

        public static void AddVideoToList(StorageFile file, Image image, bool isFrame, int time)
        {
            MyVideo myVideo = new MyVideo();
            myVideo.file = file;
            myVideo.image = image;
            myVideo.isFrame = isFrame;
            myVideo.time = time;
            videoList.Add(myVideo);
            Debug.WriteLine("video list true:" + videoList.Count);
        }

        void SelectionThumbnailHandle(object sender, SelectionChangedEventArgs args)
        {
            // ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            var lbi = sender as ListBox;
            Debug.WriteLine("index selected:" + lbi.SelectedIndex);
            if (lbi.SelectedIndex >= 0)
                currentSelectedIndex = lbi.SelectedIndex;
            //textBlock1.Text = "   You selected " + lbi.Content.ToString() + ".";
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

        private void btn_Read_Click(object sender, RoutedEventArgs e)
        {
            var shareFile = SaveVideo.videoStorageFile;
            if (shareFile != null)
                ShowShareMediaTask(shareFile.Path);

        }
        void ShowShareMediaTask(string path)
        {
            ShareMediaTask shareMediaTask = new ShareMediaTask();
            shareMediaTask.FilePath = path;
            shareMediaTask.Show();
        }

        private async void ArrangeVideoToComposition(bool isNewShow)
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
            comp.id = GlobalSettings.fileNameStore.Count + 1;
            if (isNewShow)
            {
                GlobalSettings.AddCompositionToList(comp);
                GlobalSettings.WriteClipName();
            }
        }

        private void btn_preview_Click(object sender, RoutedEventArgs e)
        {
            //tai 1 life circle, chi 1 listbox duoc add 1 item duy nhat. Neu listbox khac cung muon add item do vao,thi se gay ra loi.
            // vi vay phai clear 1 listbox roi gan vao listbox khac
            ListBox1.ItemsSource = null;
            if (mediaComposition.Clips.Count != 0)
                ArrangeVideoToComposition(true);
            else
                ArrangeVideoToComposition(false);
            NavigationService.Navigate(new Uri("/Preview.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btn_prew_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedIndex <= 0) return;
            SwapVideo(currentSelectedIndex, --currentSelectedIndex);
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedIndex >= imageList.Count - 1) return;
            SwapVideo(currentSelectedIndex, ++currentSelectedIndex);
        }

        private void btn_first_Click(object sender, RoutedEventArgs e)
        {
            SwapVideo(currentSelectedIndex, 0);
            currentSelectedIndex = 0;
        }

        private void btn_last_Click(object sender, RoutedEventArgs e)
        {
            SwapVideo(currentSelectedIndex, imageList.Count - 1);
            currentSelectedIndex = imageList.Count - 1;
        }

        private void btn_newVideo_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.isNewShow = true;
            ListBox1.ItemsSource = null;
            pageName = MainPage.PageName.MainPage;
            mediaComposition.Clips.Clear();
            imageList.Clear();
            videoList.Clear();
        }

        private void btn_frame_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Frame.xaml", UriKind.RelativeOrAbsolute));
        }


    }
}