using System;
using System.Collections.Generic;
using System.Text;
using LocationTriggering;
using LocationTriggering.Utilities;

namespace SampleAndTesting.Data
{
    class SampleLocation : LocationTrigger
    {
        private string _summary;
        private string _details;
        private string _picture;
        private string _thumbnail;
        private string _direction;
        public SampleLocation(string name, string picture, string thumbnail) : base(name)
        {
            _picture = picture;
            _thumbnail = thumbnail;
        }
        public void UpdateDirection(MapCoordinate position)
        {
            double bearing = position.BearingTo(Centre);
            int direction = (int)Math.Round(bearing / 22.5, 0);
            switch(direction)
            {
                case 0: case 16: _direction = "N";break;
                case 1: _direction = "NNE";break;
                case 2: _direction = "NE"; break;
                case 3: _direction = "ENE"; break;
                case 4: _direction = "E"; break;
                case 5: _direction = "ESE"; break;
                case 6: _direction = "SE"; break;
                case 7: _direction = "SSE"; break;
                case 8: _direction = "S"; break;
                case 9: _direction = "SSW"; break;
                case 10: _direction = "SW"; break;
                case 11: _direction = "WSW"; break;
                case 12: _direction = "W"; break;
                case 13: _direction = "WNW"; break;
                case 14: _direction = "NW"; break;
                case 15: _direction = "NNW"; break;
            }
            OnPropertyChanged("Direction");
        }

        public string Summary { get => _summary; set => _summary = value; }
        public string Details { get => _details; set => _details = value; }
        public string Picture { get => _picture; set => _picture = value; }
        public string Thumbnail { get => _thumbnail; set => _thumbnail = value; }
        public string Direction { get => _direction;}
    }
}
