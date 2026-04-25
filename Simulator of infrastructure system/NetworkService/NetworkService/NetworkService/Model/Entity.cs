using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class Entity : ClassINotifyPropertyChanged
    {
        private int id;
        private string name;
        private EntityType type;
        private double value;
        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public double Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }
        public EntityType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    Type.Type = value.Type;
                    Type.ImgSrc = value.ImgSrc;
                    OnPropertyChanged("Type");
                }
            }
        }
        public bool IsValueValid()
        {
            bool isValid = false;

            if(Value >= 0.34 && Value <= 2.73)
            {
                isValid = true;
            }

            return isValid;
        }
    }
}
