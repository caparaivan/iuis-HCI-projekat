using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace NetworkService.Model
{
    public class CircleMarker : ClassINotifyPropertyChanged
    {
        private double cmValue;
        private string cmDate;
        private string cmTime;
        private Brush cmColor;

        private double cmYPosition;

        public CircleMarker()
        {
            cmValue = 1;
        }

        public CircleMarker(int cmValue, string cmDate, string cmTime)
        {
            CmValue = cmValue;
            CmDate = cmDate;
            CmTime = cmTime;
        }

        public double CmValue
        {
            get { return cmValue; }
            set
            {
                cmValue = value;
                OnPropertyChanged("CmValue");
            }
        }

        public string CmDate
        {
            get { return cmDate; }
            set
            {
                cmDate = value;
                OnPropertyChanged("CmDate");
            }
        }

        public string CmTime
        {
            get { return cmTime; }
            set
            {
                cmTime = value;
                OnPropertyChanged("CmTime");
            }
        }

        public Brush CmColor
        {
            get { return cmColor; }
            set
            {
                cmColor = value;
                OnPropertyChanged("CmColor");
            }
        }


        public double CmYPosition
        {
            get { return cmYPosition; }
            set
            {
                cmYPosition = value;
                OnPropertyChanged("CmYPosition");
            }
        }

    }
}