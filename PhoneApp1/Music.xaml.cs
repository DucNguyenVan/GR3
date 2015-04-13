using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.ApplicationModel.Activation;
using System.Diagnostics;
using Windows.Media.Editing;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace PhoneApp1
{
    public partial class Music : PhoneApplicationPage
    {
        private IEnumerable<string> supportedMusicFileTypes = new List<string> { ".mp3" };
        bool isAddSound;
        public Music()
        {
            InitializeComponent();
            isAddSound = false;
            //neu dat tai day thi se nhay vao continiuefilePicker
            MainPage.pageName = MainPage.PageName.MusicPage;
            CheckValidateOfMediaCompotion();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var app = App.Current as App;
          
            if (app.FilePickerContinuationArgs != null && MainPage.pageName == MainPage.PageName.MusicPage && isAddSound)
            {
                this.ContinueMusicFileOpenPicker(app.FilePickerContinuationArgs);
            }
        }

        public async void ContinueMusicFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            Debug.WriteLine("Pick music");
            if (
                 args.Files != null &&
               args.Files.Count > 0)
            {
                foreach (StorageFile musicFile in args.Files)
                {
                    var file = musicFile;
                    var audio = await BackgroundAudioTrack.CreateFromFileAsync(file);
                    MainPage.mediaComposition.BackgroundAudioTracks.Add(audio);
                }
            }
        }

        private void btn_Done_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(chk_removeMusic.IsChecked);
            if (chk_removeMusic.IsChecked == true)
            {
                GlobalSettings.isRemove = true;
            }
            else
                GlobalSettings.isRemove = false;
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }

        private void btn_addMusic_Click(object sender, RoutedEventArgs e)
        {
            //MainPage.pageName = MainPage.PageName.MusicPage;
            isAddSound = true;
            FileOpenPicker openPicker = new FileOpenPicker();
            foreach (var type in supportedMusicFileTypes)
            {
                openPicker.FileTypeFilter.Add(type);
            }
            openPicker.PickSingleFileAndContinue();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Add code to perform some action here.
            Debug.WriteLine(chk_removeMusic.IsChecked);
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