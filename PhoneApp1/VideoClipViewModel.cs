using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Windows.Media.Editing;

namespace PhoneApp1
{
    public class VideoClip
    {
        public MediaClip Clip { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public string Caption { get; set; }

        public VideoClip(MediaClip clip, BitmapImage thumbnail, string caption)
        {
            Clip = clip;
            Thumbnail = thumbnail;
            Caption = caption;
        }
    }

    public class VideoClips : ObservableCollection<VideoClip>
    {
        public VideoClip SelectedClip { get; set; }
    }
}