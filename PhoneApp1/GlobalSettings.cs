using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace PhoneApp1
{
    class GlobalSettings
    {
        public static bool isRemove;
        public static bool isSaved;
        public static bool isNewShow = true;
        //public struct Composition
        //{
        //    public int id; //composition id
        //    public List<string> clipNameList ; //clips of composition
        //    public int lenght; // number of clips
        //}
        public static List<string> fileNameStore = new List<string>();
       // public static List<string> thumbnailList = new List<string>();
        public static List<string> saveFileList = new List<string>();
        public static List<Composition> compositionList = new List<Composition>();
        public static List<VideoProperty> videoPropertyList = new List<VideoProperty>();

        private GlobalSettings()
        {

        }

        public static void ReadState()
        {
            IsolatedStorageFile s = IsolatedStorageFile.GetUserStoreForApplication();
            if (s.FileExists("filename.txt"))
            {
                IsolatedStorageFileStream f = s.OpenFile("filename.txt", FileMode.Open,
                    FileAccess.Read, FileShare.Read);
                using (StreamReader r = new StreamReader(f))
                {
                    while (r.Peek() >= 0)
                    {
                        string name = r.ReadLine();
                        AddFileNameToList(name);
                    }
                    r.Close();
                    r.Dispose();
                }
                f.Close();
                f.Dispose();
            }
            s.Dispose();
        }

        public static void WriteState()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            using (var writer = new StreamWriter(
                new IsolatedStorageFileStream("filename.txt", FileMode.Create, FileAccess.Write, storage)))
            {
                foreach (string name in fileNameStore)
                {
                    writer.WriteLine(name);
                }
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
            storage.Dispose();
        }

        public static void ReadThumbnail()
        {
            videoPropertyList.Clear();
            IsolatedStorageFile s = IsolatedStorageFile.GetUserStoreForApplication();
            if (s.FileExists("thumbnail.txt"))
            {
                IsolatedStorageFileStream f = s.OpenFile("thumbnail.txt", FileMode.Open,
                    FileAccess.Read, FileShare.Read);
                using (StreamReader r = new StreamReader(f))
                {
                    while (r.Peek() >= 0)
                    {
                        string name = r.ReadLine();
                        string[] content = new string[3];
                        content = name.Split(',');
                        VideoProperty tempVideo = new VideoProperty();
                        tempVideo.thumbnail = content[0];
                        tempVideo.showName = content[1];
                        tempVideo.saveFilePath = content[2];
                        AddVideoPropertyToList(tempVideo);
                      //  AddThumbnailToList(content[0]);
                      //  AddFileNameToList(content[1]);
                    }
                    r.Close();
                    r.Dispose();
                }
                f.Close();
                f.Dispose();
            }
            s.Dispose();
        }

        public static void WriteThumbnail()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            using (var writer = new StreamWriter(
                new IsolatedStorageFileStream("thumbnail.txt", FileMode.Create, FileAccess.Write, storage)))
            {
                foreach (VideoProperty video in videoPropertyList)
                {
                    //writer.WriteLine(name);
                    writer.Write(video.thumbnail);
                    writer.Write("," + video.showName);
                    writer.Write("," + video.saveFilePath);
                    writer.WriteLine();
                }
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
            storage.Dispose();
        }

        public static void WriteClipName()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            using (var writer = new StreamWriter(
                new IsolatedStorageFileStream("clipname.txt", FileMode.Create, FileAccess.Write, storage)))
            {
                //format: id-lenght-clip's time - clip's path
                foreach (Composition composition in compositionList)
                {
                    //writer.WriteLine(name);
                    writer.Write(composition.id.ToString());
                    writer.Write("," + composition.lenght.ToString());
                    foreach (ClipFileType clipName in composition.clipNameList)
                    {
                        writer.Write("," + clipName.time.ToString());
                        writer.Write("," + clipName.clipPath);
                    }
                    writer.WriteLine();
                }
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
            storage.Dispose();
        }

        public static void ReadClipName()
        {
            compositionList.Clear();
            IsolatedStorageFile s = IsolatedStorageFile.GetUserStoreForApplication();
            if (s.FileExists("clipname.txt"))
            {
                IsolatedStorageFileStream f = s.OpenFile("clipname.txt", FileMode.Open,
                    FileAccess.Read, FileShare.Read);
                using (StreamReader r = new StreamReader(f))
                {
                    while (r.Peek() >= 0)
                    {
                        string temp = r.ReadLine();
                        string[] content = new string[20];
                       // List<string> temp = new List<string>();
                        content = temp.Split(',');
                        Composition comp = new Composition();
                        comp.id = int.Parse(content[0]);
                        comp.lenght = int.Parse(content[1]);
                        int realLenght = comp.lenght * 2; //vi truoc moi ten file lai co 1 time, nen phai duplicate
                        for (int i = 2; i <= realLenght; i+=2) //bonus +2 vi i bat dau tu 2
                        {
                            //neu chi video thi doc dung time, nhung co image thi sai time
                            //solved: dinh menh, ten file co ky tu "-" nen hieu nham luc doc file
                            ClipFileType tempClip = new ClipFileType();
                            tempClip.time = int.Parse(content[i]);
                            tempClip.clipPath = content[i+1];
                            comp.clipNameList.Add(tempClip);
                        }
                        AddCompositionToList(comp);
                    }
                    r.Close();
                    r.Dispose();
                }
                f.Close();
                f.Dispose();
            }
            s.Dispose();
        }


        public static void AddFileNameToList(string name)
        {
            GlobalSettings.fileNameStore.Add(name);
        }

        public static void AddCompositionToList(Composition comp)
        {
            GlobalSettings.compositionList.Add(comp);
        }

        public static void ChangeValueOfCompositionAt(Composition newComp, int i)
        {
            GlobalSettings.compositionList[i] = newComp;
        }

        public static void AddVideoPropertyToList(VideoProperty item)
        {
            GlobalSettings.videoPropertyList.Add(item);
        }
    }
    
}
