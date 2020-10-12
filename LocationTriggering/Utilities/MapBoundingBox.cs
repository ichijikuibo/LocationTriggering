using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LocationTriggering.Utilities
{
    /// <summary>
    /// A class used to create a bounding box around a polygon
    /// </summary>
    public class MapBoundingBox : object
    {
        private MapCoordinate _northwest; //The northwest corner;
        private MapCoordinate _southeast;//The southeast corner;
        private double _width = -1; //the width of the boundingbox in meters only calculated when requested;
        private double _height = -1; //the heigth of the boundingbox in meters only calculated when requested;
        private double _widthDegrees = -1; //the width of the boundingbox in meters;
        private double _heightDegress = -1; //the heigth of the boundingbox in meters;
        private MapCoordinate _centre;
        private bool _crossesSouthPole = false;
        private bool _crossesNorthPole = false;
        private bool _crossesDateLine = false;
        /// <summary>
        /// Construct a bounding box from the northwest and south east map coordinates
        /// </summary>
        /// <param name="northwest">The northwest point of the bounding box</param>
        /// <param name="southeast">The southeast point of the bounding box</param>
        public MapBoundingBox(MapCoordinate northwest, MapCoordinate southeast, bool dateLine = false,bool southPole=false,bool northPole=false)
        {
            _crossesSouthPole = southPole;
            _crossesNorthPole = northPole;
            _crossesDateLine = dateLine;
            _northwest = northwest;
            _southeast = southeast;
            CalculateProperties();

        }
        /// <summary>
        /// Construct a bounding box from four double representing the northwest and southeast map coordinates
        /// </summary>
        /// <param name="northwest">The northwest point of the bounding box</param>
        /// <param name="southeast">The southeast point of the bounding box</param>
        public MapBoundingBox(double lat1, double lng1, double lat2, double lng2, bool dateLine = false, bool southPole = false, bool northPole = false)
        {
            _crossesSouthPole = southPole;
            _crossesNorthPole = northPole;
            _crossesDateLine = dateLine;

            _northwest = new MapCoordinate(lat1, lng1);
            _southeast = new MapCoordinate(lat2, lng2);
            CalculateProperties();

        }
        private void CalculateProperties()
        {

            if (_crossesSouthPole || _crossesNorthPole)
                _widthDegrees = 180;
            else
                _widthDegrees = CoordinateHelpers.AngleDifference(_northwest.Longitude , _southeast.Longitude);
            if (_crossesSouthPole)
            {
                _centre = new MapCoordinate((_northwest.Latitude + (-180 - _southeast.Latitude)) / 2, (_northwest.Longitude + _southeast.Longitude) / 2);
                _heightDegress = 90 + _northwest.Latitude + 90 + _southeast.Latitude;
            }
            else if (_crossesNorthPole)
            {
                _centre = new MapCoordinate((_northwest.Latitude + (180 - _southeast.Latitude)) / 2, (_northwest.Longitude + _southeast.Longitude) / 2);
                _heightDegress = 90 - _northwest.Latitude + 90 - _southeast.Latitude;
            }
            else if (_crossesDateLine)
            {
                _centre = new MapCoordinate((_northwest.Latitude + _southeast.Latitude) / 2, _northwest.Longitude + CoordinateHelpers.AngleDifference(_northwest.Longitude, _southeast.Longitude)/2);
                _heightDegress = _northwest.Latitude - _southeast.Latitude;
            }
            else
            {
                _centre = new MapCoordinate((_northwest.Latitude + _southeast.Latitude) / 2, (_northwest.Longitude + _southeast.Longitude) / 2);
                _heightDegress = _northwest.Latitude - _southeast.Latitude;
            }
        }
        /// <summary>
        /// Determine if the specifed point is within the bounding box
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(MapCoordinate point)
        {
            if (_crossesNorthPole || _crossesSouthPole) return ContainsPointPoles(point);
            if (_crossesDateLine)//If the bounding box crosses from -180 to 180 
            {
                if ((point.Longitude >= _northwest.Longitude ||
                    point.Longitude <= _southeast.Longitude) &&
                    point.Latitude <= _northwest.Latitude &&
                    point.Latitude >= _southeast.Latitude)
                    return true;
            }
            else
            {
                if (point.Longitude >= _northwest.Longitude &&
                    point.Longitude <= _southeast.Longitude &&
                    point.Latitude <= _northwest.Latitude &&
                    point.Latitude >= _southeast.Latitude)
                    return true;
            }
            return false;
        }
        private bool ContainsPointPoles(MapCoordinate point)
        {
            if (_crossesNorthPole)
            {
                if (point.Longitude < 0)
                {
                    return point.Latitude >= _northwest.Latitude;
                }
                else
                {
                    return point.Latitude >= _southeast.Latitude;
                }
            }
            else
            {
                if (point.Longitude >= 0)
                {
                    return point.Latitude <= _northwest.Latitude;
                }
                else
                {
                    return point.Latitude <= _southeast.Latitude;
                }
            }
        }
        //public virtual bool ContainsPointPoles(MapCoordinate point)
        //{
        //    // if (_crossesInternationalDateLine && X < 0) X = 360 + X;
        //    // Get the angle between the point and the
        //    // first and last vertices.
        //    MapCoordinate[] Points = new MapCoordinate[4];
        //    Points[0] = new MapCoordinate(_northwest.Latitude, 0);
        //    Points[1] = new MapCoordinate(_northwest.Latitude,90);
        //    Points[2] = new MapCoordinate(_southeast.Latitude, 180);
        //    Points[3] = new MapCoordinate(_southeast.Latitude, -90);
        //    int max_point = Points.Length - 1;
        //    double total_angle = CoordinateHelpers.GetAngle(Points[max_point].Latitude, Points[max_point].Longitude,point.Latitude, point.Longitude,Points[0].Latitude, Points[0].Longitude);
        //    // Add the angles from the point
        //    // to each other pair of vertices.
        //    for (int i = 0; i < max_point; i++)
        //    {
        //        double angle1 = CoordinateHelpers.GetAngle(Points[i].Latitude, Points[i].Longitude, point.Latitude, point.Longitude, Points[i + 1].Latitude, Points[i + 1].Longitude);
        //        total_angle += angle1;
        //    }

        //    // The total angle should be 2 * PI or -2 * PI if 6.2831853071795862 -6.2831853071795853
        //    // the point is in the polygon and close to zero
        //    // if the point is outside the polygon.
        //    if(_crossesNorthPole) return (total_angle < -0.000001);
        //    else return (total_angle > 0.000001);
        //    // return (Math.Abs(total_angle) > 0.000001);
        //}
        /// <summary>
        /// Calculate a bearing range from the bounding box from the specifed point
        /// </summary>
        /// <param name="point">The point to calculate the range from</param>
        /// <returns>A BearingRange that covers the full bounding box </returns>
        public virtual BearingRange BearingRangeFrom(MapCoordinate point)
        {
            double centreBearing = point.BearingTo(_centre);

            double[] Differences = new double[4]; 
            double NWAngle = CoordinateHelpers.GetAngle(point.Latitude, point.Longitude, _northwest.Latitude, _northwest.Longitude);
            double SWAngle = CoordinateHelpers.GetAngle(point.Latitude, point.Longitude, _southeast.Latitude, _northwest.Longitude);
            double SEAngle = CoordinateHelpers.GetAngle(point.Latitude, point.Longitude, _southeast.Latitude, _southeast.Longitude);
            double NEAngle = CoordinateHelpers.GetAngle(point.Latitude, point.Longitude, _northwest.Latitude, _southeast.Longitude);
             Differences[0] = CoordinateHelpers.AngleDifference(centreBearing, NWAngle);
             Differences[1] = CoordinateHelpers.AngleDifference(centreBearing, SWAngle);
             Differences[2] = CoordinateHelpers.AngleDifference(centreBearing, SEAngle);
            Differences[3] = CoordinateHelpers.AngleDifference(centreBearing, NEAngle);
            
            double start = Differences.Min() + centreBearing;
            double end = Differences.Max() +  centreBearing;


            return new BearingRange(start, end);
        }
        /// <summary>
        /// Determine the width of the bounding box
        /// </summary>
        /// <returns>The width in kilometers</returns>
        private double calculateWidth()
        {
            if (_width == -1)//if width hasn't already been calculated: calculate and store;
            {
                double middle = (_northwest.Latitude + _southeast.Latitude) / 2;//The latitude midpoint
                _width = CoordinateHelpers.Haversine(middle, _northwest.Longitude, middle,_southeast.Longitude); //calculate the diistance between the west and east points of the bounding box in kilometers
            }
            return _width;
        }
        /// <summary>
        /// Determine the heigth of the bounding box
        /// </summary>
        /// <returns>The heigth in kilometers</returns>
        private double calculateHeight()
        {
            if (_height == -1)//if height hasn't already been calculated: calculate and store;
            {
                double middle = (_northwest.Longitude + _southeast.Longitude) / 2;//The longitude midpoint
                if(_crossesSouthPole||_crossesNorthPole)
                    _height = CoordinateHelpers.Haversine(_northwest.Latitude,middle, _southeast.Latitude, -middle); //calculate the distance between the north and south points of the bounding box in metres
                else _height = CoordinateHelpers.Haversine(_northwest.Latitude, middle, _southeast.Latitude, middle); //calculate the distance between the north and south points of the bounding box in metres
            }
            return _height;
        }
        /// <summary>
        /// converts the bounding box to a string "###.###, ###.### - ###.###, ###.###"
        /// </summary>
        /// <returns>String that represnts the bounding box</returns>
        public override string ToString()
        {
            return _northwest.ToString() + " - " + _southeast.ToString();
        }
        /// <summary>
        /// Determines if the bounding box refers to the same area
        /// </summary>
        /// <param name="obj">The MapBoundingBox to compare to</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            MapBoundingBox MC = obj as MapBoundingBox;
            return MC.Northwest.Equals(Northwest) && MC.Southeast.Equals(Southeast);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// The southeast point of the box
        /// </summary>
        public MapCoordinate Southeast { get => _southeast; }
        /// <summary>
        /// The northwest point of the box
        /// </summary>

        public MapCoordinate Northwest { get => _northwest; }
        /// <summary>
        /// The width of the box in kilometers
        /// </summary>
        public double Width { get => calculateWidth();  }
        /// <summary>
        /// The height of the box in kilometers
        /// </summary>
        public double Height { get => calculateHeight(); }
        /// <summary>
        /// The width of the box in deegrees
        /// </summary>
        public double WidthDegrees { get => _widthDegrees;  }
        /// <summary>
        /// The height of the box in degrees
        /// </summary>
        public double HeightDegrees { get => _heightDegress;  }
        public MapCoordinate Centre { get => _centre; }
    }
}
