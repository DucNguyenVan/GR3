using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;
using Windows.Storage.FileProperties;
using Windows.Storage;

namespace PhoneApp1
{
    public partial class Frame : PhoneApplicationPage
    {
        int wbHeight = 480;
        int wbWitdh = 800;
        TextBox textOfFrame = new TextBox();
        TranslateTransform pos;
        WriteableBitmap frameCover;
       // bool isCreateFrame;
        int time;
        public Frame()
        {
            InitializeComponent();
          //  isCreateFrame = false;
            MainPage.pageName = MainPage.PageName.FramePage;
            pos = new TranslateTransform();
            frameCover = new WriteableBitmap(wbWitdh, wbHeight);
            textOfFrame.FontSize = 30;
            textOfFrame.Opacity = 1;
            pos.X = (wbWitdh - tbl_text.Width) / 2;
            pos.Y = (wbHeight - tbl_text.Height) / 2;
        }

        private void SetTextOfFrame()
        {
            tbl_text.Text = tb_TextOfFrame.Text;
            frameCover.Render(tbl_text, pos);
            frameCover.Invalidate();
        }

        private void Render()
        {
            //frameCover.Render(tb, pos);
            //frameCover.Invalidate();
        }

        private async void SaveToStream()
        {
            //do khi tao frame dat ten trung voi ten cua thumbnail. nen get file bi loan ten file, lay ra anh k dung y muon
            // nen se doi ten file frame theo dinh dang
            // [id's video + image's name] vd: video id 1:  1.1, 1.2, 1.3 m video id 2: 2.1, 2.2, 2.3 
            int name = MainPage.imageList.Count + 1;
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var isfs = new IsolatedStorageFileStream(WelcomePage.currentSelectedIndex +"." + name.ToString() + ".jpg", FileMode.OpenOrCreate, isf))
                {
                    StorageFile sampleFile = null;
                    frameCover.SaveJpeg(isfs, 800, 480, 0, 100);
                    try
                    {
                        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                        sampleFile = await localFolder.GetFileAsync(WelcomePage.currentSelectedIndex + "." + name.ToString() + ".jpg");
                        var bitmap = new BitmapImage();
                        Image img = new Image();
                        bitmap.UriSource = new Uri(sampleFile.Path);
                        img.Source = bitmap;
                        MainPage.AddVideoToList(sampleFile, img, true,time);
                        MainPage.AddMoreImageToList(img);
                        //fuck, di vai. dat cau lenh o back_click thi bi loi fall out of range
                        // vi goi nhieu lenh await, nen thu tu thuc hien se khong lan luot. Vi the phai dat goBack o trong nay
                        if (this.NavigationService.CanGoBack)
                        {
                            this.NavigationService.GoBack();
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //  isFileExist = false;
                        Debug.WriteLine("file not found");

                    }
                }
            }
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            tbl_text.Text = tb_TextOfFrame.Text;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            time = int.Parse(tbl_time.Text);
            if (time <= 0)
            {
                MessageBox.Show("Time must be greater than zero!");
                return;
            }
            CreateFrame();
        }

        private void CreateFrame()
        {
           // isCreateFrame = true;
            SetTextOfFrame();
            Render();
            SaveToStream();
        }

    }
}