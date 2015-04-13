using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;
using Windows.Storage;
using System.Threading;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using System.Windows.Threading;
using Windows.Storage.FileProperties;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PhoneApp1
{
    public partial class Preview : PhoneApplicationPage
    {
        DispatcherTimer posTimer;
        private CancellationTokenSource cts;
        public static StorageFile filePreview;
        public static int selectedShow = 0;
        string fileName;
        private int selectIndex;
        public static List<Image> previewImageList = new List<Image>();
        public Preview()
        {
            InitializeComponent();
            MainPage.pageName = MainPage.PageName.PreviewPage;
            sliderStream.Visibility = Visibility.Collapsed;
            btn_save.Visibility = Visibility.Collapsed;
            btn_share.Visibility = Visibility.Collapsed;
            gr_VideoList.Visibility = Visibility.Collapsed;
            ListBox1.ItemsSource = null;
            GenerateFileV3();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //khi back lai mainpage can phai giai phong ListBox1
            ListBox1.ItemsSource = null;
        }

        private void SetSliderSize()
        {
            Debug.WriteLine("set slider");
            sliderStream.Minimum = 0;
            sliderStream.Maximum = MainPage.mediaComposition.Duration.TotalSeconds;
            sliderStream.Value = media_preview.Position.TotalSeconds;
            sliderStream.Visibility = Visibility.Visible;
        }

        private void sliderStream_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine("Select slider");
            media_preview.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {

            Debug.WriteLine("stream");
            // Reinit pos slider
            sliderStream.Minimum = 0;
            sliderStream.Maximum = MainPage.mediaComposition.Duration.TotalSeconds;
            sliderStream.Value = media_preview.Position.TotalSeconds;
            sliderStream.Visibility = Visibility.Visible;

            // Start times which updates the Slider
            // You should actually use Element binding wiht a Converter here
            posTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            posTimer.Tick += (o, o1) =>
            {
                sliderStream.ValueChanged -= sliderStream_ValueChanged;
                sliderStream.Value = media_preview.Position.TotalSeconds;
                sliderStream.ValueChanged += sliderStream_ValueChanged;
            };
            posTimer.Start();
            // this.textBlock1.Text = msg;
        }

        private async void VideoPlayback()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists("bai8.mp4"))
                {
                    Debug.WriteLine("file open");
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream("bai8.mp4", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, isf))
                    {
                        Stream stream = (await filePreview.OpenReadAsync()).AsStreamForRead();
                        stream.CopyTo(isfs);
                        this.media_preview.SetSource(isfs);
                        isfs.Close();
                        stream.Dispose();
                        stream.Close();
                    }
                }
                else
                {
                    Debug.WriteLine("file create");
                    using (var isfs = new IsolatedStorageFileStream("bai8.mp4", FileMode.CreateNew, isf))
                    {
                        Stream stream = (await filePreview.OpenReadAsync()).AsStreamForRead();
                        stream.CopyTo(isfs);
                        this.media_preview.SetSource(isfs);
                        isfs.Close();
                        stream.Dispose();
                        stream.Close();
                    }
                }
                isf.Dispose();
            }
            media_preview.Play();
            sliderStream.Visibility = Visibility.Visible;
        }

        private void GenerateVideoForPreview()
        {
            MainPage.mediaComposition.GeneratePreviewMediaStreamSource((int)media_preview.ActualWidth, (int)media_preview.ActualHeight);
        }

        private async void GenerateFileV3()
        {
            int per;
            StorageFile sampleFile = null;

            if (GlobalSettings.isNewShow && MainPage.mediaComposition.Clips.Count != 0)
            {
                btn_show.Visibility = Visibility.Collapsed;
                int name = GlobalSettings.fileNameStore.Count + 1;
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                sampleFile = await localFolder.CreateFileAsync(name.ToString() + ".mp4", CreationCollisionOption.ReplaceExisting);
                var mediaEncodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Wvga);
                cts = new CancellationTokenSource();
                var progress = new Progress<double>((percent) =>
                {
                    per = (int)percent;
                    tbl_Loading.Text = "Prepairing  " + (per).ToString() + "%";
                });
                await MainPage.mediaComposition.RenderToFileAsync(sampleFile, MediaTrimmingPreference.Fast, mediaEncodingProfile).AsTask(cts.Token, progress);
                fileName = sampleFile.Name;
                Debug.WriteLine(sampleFile.Name);
                GlobalSettings.isNewShow = false;
                GlobalSettings.AddFileNameToList(sampleFile.Name);
                GlobalSettings.WriteState();
                AddThumbnailToPreviewVideo(MainPage.imageList[0]);

                btn_show.Visibility = Visibility.Visible;
                filePreview = sampleFile;
                VideoPlayback();
            }
            // ShowSourceOfListBox();
            ShowUI();
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
            //  NavigationService.Navigate(new Uri("/Upload.xaml", UriKind.RelativeOrAbsolute));
        }

        private bool CheckValidateOfMediaCompotion()
        {
            if (MainPage.mediaComposition.Clips.Count == 0)
            {
                gr_check.Visibility = Visibility.Visible;
                return false;
            }
            return true;
        }

        private void ShowUI()
        {
            // sliderStream.Visibility = Visibility.Visible;
            btn_back.Visibility = Visibility.Visible;
            tbl_Loading.Visibility = Visibility.Collapsed;
            tbl_anoun.Visibility = Visibility.Collapsed;
        }

        private async void SetFileToPreView(string fileName)
        {
            media_preview.Stop();
            //set source to null to play another video
            media_preview.Source = null;
            //   posTimer.Stop();
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var sampleFile = await localFolder.GetFileAsync(fileName);
            filePreview = null;
            filePreview = sampleFile;
            VideoPlayback();
        }

        private void GenerateUI()
        {
            if (GlobalSettings.isNewShow)
            {
                gr_VideoList.Visibility = Visibility.Collapsed;
            }
            else
            {
                gr_VideoList.Visibility = Visibility.Visible;
            }
        }

        private void ShowSourceOfListBox()
        {
            ListBox1.ItemsSource = null;
            ListBox1.ItemsSource = previewImageList;
            Debug.WriteLine("imagelist number" + previewImageList.Count);
            //ListBox1.ItemsSource = GlobalSettings.fileNameStore;
        }

        public static void AddThumbnailToPreviewVideo(Image img)
        {
            TranslateTransform pos = new TranslateTransform();
            WriteableBitmap wb = new WriteableBitmap(img, pos);
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // int name = GlobalSettings.thumbnailList.Count + 1;
                int name = GlobalSettings.videoPropertyList.Count + 1;
                using (var isfs = new IsolatedStorageFileStream(name.ToString() + ".jpg", FileMode.OpenOrCreate, isf))
                {
                    wb.SaveJpeg(isfs, 800, 480, 0, 100);
                    VideoProperty tempVideo = new VideoProperty();
                    tempVideo.thumbnail = name.ToString();
                    GlobalSettings.AddVideoPropertyToList(tempVideo);
                    GlobalSettings.WriteThumbnail();
                }
            }

        }

        public async void GetAllThumbnail()
        {
            previewImageList.Clear();
            //   GlobalSettings.ReadThumbnail();
            foreach (VideoProperty video in GlobalSettings.videoPropertyList)
            {
                StorageFile sampleFile = null;
                Debug.WriteLine(video.thumbnail);
                try
                {
                    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    sampleFile = await localFolder.GetFileAsync(video.thumbnail.ToString() + ".jpg");
                    var bitmap = new BitmapImage();
                    Image img = new Image();
                    bitmap.UriSource = new Uri(sampleFile.Path);
                    img.Source = bitmap;
                    previewImageList.Add(img);
                    Debug.WriteLine("file exist");
                }
                catch (FileNotFoundException)
                {
                    //  isFileExist = false;
                    Debug.WriteLine("File not exist");

                }
            }
            ShowSourceOfListBox();
            Debug.WriteLine("imagelist number after get thumbnail" + previewImageList.Count);
        }

        private void SetPropertiesForVideo()
        {
        }

        void SelectionVideoHandle(object sender, SelectionChangedEventArgs args)
        {
            // Debug.WriteLine("select");
            // ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            var lb = sender as ListBox;
            Debug.WriteLine("index selected:" + lb.SelectedIndex);
            if (lb.SelectedIndex >= 0)
            {
                selectIndex = lb.SelectedIndex;
                Debug.WriteLine(GlobalSettings.fileNameStore[selectIndex]);
                SetFileToPreView(GlobalSettings.fileNameStore[selectIndex]);
                Preview.selectedShow = selectIndex;
                if (GlobalSettings.videoPropertyList[selectedShow].saveFilePath == "")
                {
                    btn_save.Visibility = Visibility.Visible;
                    btn_share.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btn_save.Visibility = Visibility.Collapsed;
                    btn_share.Visibility = Visibility.Visible;
                }
            }

        }



        private void btn_show_Click(object sender, RoutedEventArgs e)
        {
            gr_VideoList.Visibility = Visibility.Visible;
            GetAllThumbnail();
         //   GlobalSettings.ReadThumbnail();
            // ShowSourceOfListBox();
            Storyboard_show.Begin();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            media_preview.Stop();
            media_preview.Source = null;
            NavigationService.Navigate(new Uri("/SaveVideo.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btn_share_Click(object sender, RoutedEventArgs e)
        {
            media_preview.Stop();
            media_preview.Source = null;
            NavigationService.Navigate(new Uri("/Upload.xaml", UriKind.RelativeOrAbsolute));
        }

    }
}