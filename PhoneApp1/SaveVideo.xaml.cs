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
using Windows.Storage;
using Windows.Media.MediaProperties;
using System.Threading;
using Windows.Media.Editing;
using System.IO;

namespace PhoneApp1
{
    public partial class SaveVideo : PhoneApplicationPage
    {
        MediaClip clip;
        public static StorageFile videoStorageFile;
        public StorageFile sampleFile;
        private CancellationTokenSource cts;
        MediaComposition mediaComposition;
        int selectIndex = 0;
        public SaveVideo()
        {
            InitializeComponent();
            MainPage.pageName = MainPage.PageName.SavePage;
            tbl_success.Visibility = Visibility.Collapsed;
            tbl_Loading.Visibility = Visibility.Visible;
            mediaComposition = new MediaComposition();
          //  CheckValidateOfMediaCompotion();
            SetMediaComposition(Preview.selectedShow);
            List<string> source = new List<string>();
            source.Add("HD1080");
            source.Add("HD720");
            source.Add("WVGA");
            lper_quality.ItemsSource = source;
            
        }

        private void Picker(object sender, SelectionChangedEventArgs e)
        {
            var picker = sender as ListPicker;
            selectIndex = picker.SelectedIndex;
            Debug.WriteLine(picker.SelectedItem.ToString());
        }

        private MediaEncodingProfile SelectProfile(int index)
        {
            MediaEncodingProfile profile = new MediaEncodingProfile();
            switch (index)
            {
                case 0:
                    profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
                    Debug.WriteLine(index);
                    break;
                case 1:
                    profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
                    Debug.WriteLine(index);
                    break;
                case 2:
                    profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Wvga);
                    Debug.WriteLine(index);
                    break;
            }
           
            return profile;
        }

        private async void SetMediaComposition(int index)
        {
            Debug.WriteLine("composistion count" + GlobalSettings.compositionList.Count);
            foreach (ClipFileType clipName in GlobalSettings.compositionList[index].clipNameList)
            {
                //dame: PicturesLibrary, VideosLibrary, SavedPictures
                try
                {
                    // var fileClip = await KnownFolders.PicturesLibrary.GetFileAsync(clipName);
                    StorageFile file = await StorageFile.GetFileFromPathAsync(clipName.clipPath);
                    MediaClip clip;
                    Debug.WriteLine("File type:" + file.FileType + file.Name);
                    if (file.FileType != ".mp4" && clipName.time > 0)
                    {
                       // Debug.WriteLine("add frame to video" + video.time);
                        clip = await MediaClip.CreateFromImageFileAsync(file, System.TimeSpan.FromSeconds(clipName.time));
                    }
                    else
                        clip = await MediaClip.CreateFromFileAsync(file);
                    mediaComposition.Clips.Add(clip);
                }
                catch (FileNotFoundException)
                {
                    Debug.WriteLine("clip not found");
                }
            }
        }

        private async void btn_Save_Click_1(object sender, RoutedEventArgs e)
        {
            int per;
            Debug.WriteLine("Save");
            btn_Save.Visibility = Visibility.Collapsed;
            videoStorageFile = await KnownFolders.VideosLibrary.CreateFileAsync("editedVideo.mp4", CreationCollisionOption.GenerateUniqueName);
            var mediaEncodingProfile = SelectProfile(selectIndex);
            //if (GlobalSettings.isRemove)
            //    DisableCurrentMusic();

            //old
            // await mediaComposition.RenderToFileAsync(videoStorageFile, MediaTrimmingPreference.Fast, mediaEncodingProfile);
            //new
            cts = new CancellationTokenSource();
            var progress = new Progress<double>((percent) =>
            {
                Debug.WriteLine("progress:" + percent);
                per = (int)percent;
                tbl_Loading.Text = "Saving  " + (per).ToString() + "%";
            });
            await mediaComposition.RenderToFileAsync(videoStorageFile, MediaTrimmingPreference.Fast, mediaEncodingProfile).AsTask(cts.Token, progress);
           // btn_Save.Visibility = Visibility.Visible;
            tbl_success.Visibility = Visibility.Visible;
            tbl_Loading.Visibility = Visibility.Collapsed;
            GlobalSettings.videoPropertyList[Preview.selectedShow].saveFilePath = videoStorageFile.Path;
            GlobalSettings.WriteThumbnail();
            GlobalSettings.isSaved = true;
        }
        private void DisableCurrentMusic()
        {
            foreach (MediaClip clip in MainPage.mediaComposition.Clips)
            {
                clip.Volume = 0;
            }
        }

        private void btn_Done_Click(object sender, RoutedEventArgs e)
        {
            //k dung goBack vi SavePage co the duoc navigate tu previewPage
          //  NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }

        }
        private void CheckValidateOfMediaCompotion()
        {
            if (MainPage.mediaComposition.Clips.Count == 0)
            {
                gr_check.Visibility = Visibility.Visible;
            }
        }
    }
}