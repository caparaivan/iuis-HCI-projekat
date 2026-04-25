using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetworkService.Model
{
    public class Measurement
    {
        public string Time { get; set; }
        public string Date { get; set; }
        public double Value { get; set; }
        public Measurement(string date, string time, int value)
        {
            Time = time;
            Date = date;
            Value = value;
        }
    }
}