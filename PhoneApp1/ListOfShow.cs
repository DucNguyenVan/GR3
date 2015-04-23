using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PhoneApp1
{
   public class ListOfShow
    {
        public string showName;
        public string imgPath;
        public ListOfShow(string imgPath, string name)
        {
            this.showName = name;
            this.imgPath = imgPath;
        }
    }
}
