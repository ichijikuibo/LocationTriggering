using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LocationTriggering.Utilities
{
    public class MapBoundingBox : object
    {
        private MapCoordinate _northwest; //The northwest corner;
        private MapCoordinate _southeast;//The southeast corner;
        private double _width = -1; //the width of the boundingbox in meters only calculated when requested;
        private double _height = -1; //the heigth of the boundingbox in meters only calculated when requested;
        private double _widthDegrees = -1; //the width of the boundingbox in meters;
        private double _heightDegress = -1; //the heigth of the boundingbox in meters;
        private bool _crossesInternationalDateLine = false;
        public MapBoundingBox(MapCoordinate northwest, MapCoordinate southeast)
        {
            if (southeast.Longitude - northwest.Longitude > 180)
            {
                _crossesInternationalDateLine = true;
            }

            _northwest = northwest;
            _southeast = southeast;

            _widthDegrees = CoordinateHelpers.NormaliseLongitude(_northwest.Longitude - _southeast.Longitude);
            _heightDegress = _northwest.Latitude - southeast.Latitude;



        }
        public MapBoundingBox(double lat1, double lng1, double lat2, double lng2)
        {


            _northwest = new MapCoordinate(lat1, lng1);
            _southeast = new MapCoordinate(lat2, lng2);
            if (southeast.Longitude - northwest.Longitude > 180)
            {
                _crossesInternationalDateLine = true;
            }

            _widthDegrees = CoordinateHelpers.NormaliseBearing(_northwest.Longitude - _southeast.Longitude);
            _heightDegress = _northwest.Latitude - southeast.Latitude;

        }
        public bool containsPoint(MapCoordinate point)
        {
            if (_crossesInternationalDateLine)
            {
                if ((point.Longitude >= _northwest.Longitude ||
                    point.Longitude <= _southeast.Longitude) &&
                    point.Latitude <= _northwest.Latitude &&
                    point.Latitude >= _southeast.Latitude)
                    return true;
            }
            else {
                if (point.Longitude >= _northwest.Longitude &&
                    point.Longitude <= _southeast.Longitude &&
                    point.Latitude <= _northwest.Latitude &&
                    point.Latitude >= _southeast.Latitude)
                    return true;
            }
            return false;
        }
        private double calculateWidth()
        {
            if (_width == -1)//if width hasn't already been calculated: calculate and store;
            {
                double middle = (_northwest.Latitude + _southeast.Latitude) / 2;//The latitude midpoint
                _width = CoordinateHelpers.Haversine(middle, _northwest.Longitude, middle,_southeast.Longitude); //calculate the diistance between the west and east points of the bounding box in metres
            }
            return _width;
        }
        private double calculateHeight()
        {
            if (_height == -1)//if height hasn't already been calculated: calculate and store;
            {
                double middle = (_northwest.Longitude + _southeast.Longitude) / 2;//The longitude midpoint
                _height = CoordinateHelpers.Haversine(_northwest.Latitude,middle, _southeast.Latitude, middle); //calculate the diistance between the north and south points of the bounding box in metres
            }
            return _height;
        }
        public override string ToString()
        {
            return northwest.ToString() + " - " + _southeast.ToString();
        }
        public override bool Equals(object obj)
        {
            MapBoundingBox MC = obj as MapBoundingBox;
            return MC.northwest.Equals(northwest) && MC.southeast.Equals(southeast);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public MapCoordinate southeast { get => _southeast; }
        public MapCoordinate northwest { get => _northwest; }
        public double width { get => calculateWidth();  }
        public double height { get => calculateHeight(); }
        public double WidthDegrees { get => _widthDegrees;  }
        public double HeightDegress { get => _heightDegress;  }
    }
}
