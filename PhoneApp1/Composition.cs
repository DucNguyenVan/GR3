using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneApp1
{
    class Composition
    {
        //private struct ClipFileType
        //{
        //    public string clipPath; //path of clip
        //    public int time; // 0=video, #0 = image, time of frame
        //}
        public int id; //composition id
        public List<ClipFileType> clipNameList = new List<ClipFileType>(); //clips of composition
        public int lenght; // number of clips
        public Composition()
        {

        }
    }
    class ClipFileType
    {
        public string clipPath; //path of clip
        public int time; // 0=video, #0 = image, time of frame
        public ClipFileType()
        {

        }
    }
}
