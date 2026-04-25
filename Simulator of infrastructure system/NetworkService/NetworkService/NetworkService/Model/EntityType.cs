using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
   
    public class EntityType : ClassINotifyPropertyChanged
    {
        private string type;
        private string imgSrc = "";
        public string Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }
        public string ImgSrc
        {
            get { return imgSrc; }
            set
            {
                if (imgSrc != value)
                {
                    imgSrc = value;
                    OnPropertyChanged("ImgSrc");
                }
            }
        }
    }
}
