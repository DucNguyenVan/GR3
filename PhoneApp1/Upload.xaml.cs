using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using Google.Apis.Upload;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using System.Threading;
using Google.Apis.Services;
using System.Reflection;
using Google.Apis.YouTube.v3.Data;
using System.IO;
using Windows.Storage;

namespace PhoneApp1
{
    public partial class Upload : PhoneApplicationPage
    {
        int selectIndex = 0;
        long lenght = 0;
        string text;
      //  StorageFile uploadFile;
        public Upload()
        {
            InitializeComponent();
            MainPage.pageName = MainPage.PageName.UploadPage;
            gr_upload.Visibility = Visibility.Collapsed;
            gr_checkSaveFile.Visibility = Visibility.Collapsed;
            //uploadFile = SaveVideo.videoStorageFile;
            
            //if (uploadFile == null)
            //{
            //    gr_checkSaveFile.Visibility = Visibility.Visible;
            //}
            List<string> source = CreateSource();
            ListBox1.ItemsSource = source;
         //   lper_category.ItemsSource = source;
        }

        private List<string> CreateSource()
        {
            List<string> sourceList = new List<string>();
            sourceList.Add("Film & Animation"); //1
            sourceList.Add("Autos & Vehicles"); //2
            sourceList.Add("Music"); //10
            sourceList.Add("Pets & Animals"); //15
            sourceList.Add("Sports"); //17
            sourceList.Add("Gaming"); //20
            sourceList.Add("People"); // 22
            sourceList.Add("Entertainment"); //24
            sourceList.Add("News & Politics"); //25
            sourceList.Add("Education"); //27
            sourceList.Add("Science & Technology"); //28
            return sourceList;
        }

        private void CategoryPicker(object sender, SelectionChangedEventArgs e)
        {
            var picker = sender as ListPicker;
            selectIndex = picker.SelectedIndex;
            Debug.WriteLine(picker.SelectedItem.ToString());
        }

        private string CatogoryIndex(int index)
        {
            string categoryId = "1";
            switch (index)
            {
                case 0:
                    categoryId = "1";
                    break;
                case 1:
                    categoryId = "2";
                    break;
                case 2:
                    categoryId = "10";
                    break;
                case 3:
                    categoryId = "15";
                    break;
                case 4:
                    categoryId = "17";
                    break;
                case 5:
                    categoryId = "20";
                    break;
                case 6:
                    categoryId = "22";
                    break;
                case 7:
                    categoryId = "24";
                    break;
                case 8:
                    categoryId = "25";
                    break;
                case 9:
                    categoryId = "27";
                    break;
                case 10:
                    categoryId = "28";
                    break;
            }
            return categoryId;
        }

        private async void btn_upload_Click(object sender, RoutedEventArgs e)
        {
          //  if (!GlobalSettings.isSaved) return;
            gr_upload.Visibility = Visibility.Visible;
            // TODO: Add event handler implementation here.
            UserCredential credential;
            //  using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            // {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "418704139389-v3n8m2i1j1k72mvb05jh1dst1sd4ivct.apps.googleusercontent.com",
                    ClientSecret = "iQJ0yc-CylbGI81UbKfCQPr2"
                },
                // This OAuth 2.0 access scope allows an application to upload files to the
                // authenticated user's YouTube channel, but doesn't allow other types of access.
                new[] { YouTubeService.Scope.YoutubeUpload },
                "user",
                CancellationToken.None
            );
            // }


            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = tbx_title.Text;
            video.Snippet.Description = tbx_description.Text;
            video.Snippet.Tags = new string[] { "tag1", "tag2" };
            video.Snippet.CategoryId = CatogoryIndex(selectIndex); // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "unlisted"; // or "private" or "public"
         //   Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
         //   StorageFile sampleFile = await localFolder.GetFileAsync("editedVideo.mp4");
         //   var fileStream = new FileStream()
         //   var uploadFile = SaveVideo.videoStorageFile;
//            uploadFile = Preview.filePreview;
            //using (var fileStream = new FileStream(uploadFile.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            StorageFile file = await StorageFile.GetFileFromPathAsync(GlobalSettings.videoPropertyList[MainPage.videoIndexSelected].saveFilePath);
            Debug.WriteLine("preview selected:" + MainPage.videoIndexSelected);
            Debug.WriteLine("save path" + GlobalSettings.videoPropertyList[MainPage.videoIndexSelected].saveFilePath);
            using (var fileStream = new FileStream(file.Path, FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite))
            {
                tbl_loadingPercent.Visibility = Visibility.Visible;
                tbl_loadingPercent.Text = "Uploading";
                //get lenght of video file to calculate percent
                Stream stream = (await file.OpenReadAsync()).AsStreamForRead();
                tbl_percent.Text = text;
                lenght = stream.Length/1024;
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/mp4");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                await videosInsertRequest.UploadAsync();
                tbl_loadingPercent.Text = "Successful";
                ShowUIWhenUploaded();
              //  gr_upload.Visibility = Visibility.Collapsed;

            }
        }
        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            Debug.WriteLine("status: " + progress.Status);
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    long per = (progress.BytesSent/1024);
                    text = per.ToString();
                    PostMyMessage(text);
                    Debug.WriteLine(text);
                    break;

                case UploadStatus.Failed:
                    Debug.WriteLine(progress.Exception);
                    break;

            }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
            //Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
            Debug.WriteLine("successfully uploaded" + video.Id);
        }

        void SelectionCategoryHandle(object sender, SelectionChangedEventArgs args)
        {
            // ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            var lb = sender as ListBox;
            Debug.WriteLine("index selected:" + lb.SelectedIndex);
            selectIndex = lb.SelectedIndex;
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }

        private void btn_Done_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }

        private void ShowUIWhenUploaded()
        {
            btn_Done.Visibility = Visibility.Visible;
          //  tbl_loadingPercent.Visibility = Visibility.Collapsed;
            tbl_percent.Visibility = Visibility.Collapsed;
            tbl_total.Visibility = Visibility.Collapsed;
        }

        private void PostMyMessage(string text)
        {
            if (this.Dispatcher.CheckAccess())
            {
                tbl_percent.Text = text;
                tbl_total.Text = "/" + lenght.ToString()+" MB";
            }
            else
                this.Dispatcher.BeginInvoke(new Action<string>(PostMyMessage), text);
        }

    }
}