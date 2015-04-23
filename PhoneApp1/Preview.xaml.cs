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
        bool isStream;
        public static List<Image> previewImageList = new List<Image>();
        string defaultName = "FMovie";
        public Preview()
        {
            InitializeComponent();
            MainPage.pageName = MainPage.PageName.PreviewPage;
            sliderStream.Visibility = Visibility.Collapsed;
            btn_save.Visibility = Visibility.Collapsed;
            btn_share.Visibility = Visibility.Collapsed;
            btn_replay.Visibility = Visibility.Collapsed;
            GenerateFileV3();
         //   GenerateVideoForPreview();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            sliderStream.Visibility = Visibility.Collapsed;
            btn_save.Visibility = Visibility.Collapsed;
            btn_share.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //khi back lai mainpage can phai giai phong ListBox1
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
            ShowUI();
            isStream = true;
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

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            btn_replay.Visibility = Visibility.Visible;
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
           // media_preview.SetSource( MainPage.mediaComposition.GeneratePreviewMediaStreamSource((int)media_preview.ActualWidth, (int)media_preview.ActualHeight));
        }

        private async void GenerateFileV3()
        {
            int per;
            StorageFile sampleFile = null;
            Debug.WriteLine("isnewshow" + GlobalSettings.isNewShow+" "+MainPage.mediaComposition.Clips.Count);
            //do mediacomposition.clip o trong ham async, nen khi navigatate tu Main -> Preview thi clips van chua duoc add het
            // nen doi khi clips.count van bang 0, mac du thuc te khac 0
            if (GlobalSettings.isNewShow)
            {
                Debug.WriteLine(" new render");
                if (GlobalSettings.isRemove)
                    DisableCurrentMusic();

               // int name = GlobalSettings.fileNameStore.Count + 1;
                int name = GlobalSettings.videoPropertyList.Count + 1;
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                sampleFile = await localFolder.CreateFileAsync("previewVideo.mp4", CreationCollisionOption.ReplaceExisting);
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
                //GlobalSettings.isNewShow = false;
                //GlobalSettings.AddFileNameToList(sampleFile.Name);
                //GlobalSettings.WriteState();
                //AddThumbnailToPreviewVideo(MainPage.imageList[0]);

               // btn_show.Visibility = Visibility.Visible;
                filePreview = sampleFile;
                VideoPlayback();
                GlobalSettings.isNewShow = false;
                //GlobalSettings.AddFileNameToList(sampleFile.Name);
                //GlobalSettings.WriteState();
                AddThumbnailToPreviewVideo(MainPage.imageList[0]);
            }
            else
            {
                Debug.WriteLine("re- render");
                //chi render, khong luu file moi
                if (GlobalSettings.isRemove)
                    DisableCurrentMusic();

               // int name = GlobalSettings.fileNameStore.Count + 1;
                int name = GlobalSettings.videoPropertyList.Count + 1;
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                sampleFile = await localFolder.CreateFileAsync("previewVideo.mp4", CreationCollisionOption.ReplaceExisting);
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
                //GlobalSettings.isNewShow = false;
                //GlobalSettings.AddFileNameToList(sampleFile.Name);
                //GlobalSettings.WriteState();
                //AddThumbnailToPreviewVideo(MainPage.imageList[0]);

              //  btn_show.Visibility = Visibility.Visible;
                filePreview = sampleFile;
                VideoPlayback();
                //GlobalSettings.isNewShow = false;

            }
            // ShowSourceOfListBox();
           // ShowUI();
        }

        private void DisableCurrentMusic()
        {
            foreach (MediaClip clip in MainPage.mediaComposition.Clips)
            {
                clip.Volume = 0;
            }
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
            btn_save.Visibility = Visibility.Visible;
            btn_back.Visibility = Visibility.Visible;
            tbl_Loading.Visibility = Visibility.Collapsed;
            tbl_anoun.Visibility = Visibility.Collapsed;
            btn_replay.Visibility = Visibility.Collapsed;
        }

        //private async void SetFileToPreView(string fileName)
        //{
        //    media_preview.Stop();
        //    //set source to null to play another video
        //    media_preview.Source = null;
        //    //   posTimer.Stop();
        //    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //    var sampleFile = await localFolder.GetFileAsync(fileName);
        //    filePreview = null;
        //    filePreview = sampleFile;
        //    VideoPlayback();
        //}

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
                    tempVideo.showName = "FMovie " + (GlobalSettings.videoPropertyList.Count + 1).ToString();
                    GlobalSettings.AddVideoPropertyToList(tempVideo);
                    GlobalSettings.WriteThumbnail();
                }
            }

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

        private void btn_replay_Click(object sender, RoutedEventArgs e)
        {
            btn_replay.Visibility = Visibility.Collapsed;
            media_preview.Position = TimeSpan.Zero;
            media_preview.Play();
        }

    }
}