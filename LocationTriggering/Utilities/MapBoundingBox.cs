﻿using System;
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
        private bool _crossesInternationalDateLine = false;
        /// <summary>
        /// Construct a bounding box from the northwest and south east map coordinates
        /// </summary>
        /// <param name="northwest">The northwest point of the bounding box</param>
        /// <param name="southeast">The southeast point of the bounding box</param>
        public MapBoundingBox(MapCoordinate northwest, MapCoordinate southeast)
        {
            if (southeast.Longitude - northwest.Longitude > 180)//determine if the bounding box crosses between -180 and 180
            {
                _crossesInternationalDateLine = true;
            }

            _northwest = northwest;
            _southeast = southeast;
            _centre = new MapCoordinate( (_northwest.Latitude + southeast.Latitude) / 2, CoordinateHelpers.NormaliseLongitude((_northwest.Longitude + _southeast.Longitude) / 2));
            _widthDegrees = CoordinateHelpers.NormaliseLongitude(_northwest.Longitude - _southeast.Longitude);
            _heightDegress = _northwest.Latitude - southeast.Latitude;



        }
        /// <summary>
        /// Construct a bounding box from four double representing the northwest and southeast map coordinates
        /// </summary>
        /// <param name="northwest">The northwest point of the bounding box</param>
        /// <param name="southeast">The southeast point of the bounding box</param>
        public MapBoundingBox(double lat1, double lng1, double lat2, double lng2)
        {


            _northwest = new MapCoordinate(lat1, lng1);
            _southeast = new MapCoordinate(lat2, lng2);
            if (southeast.Longitude - northwest.Longitude > 180)//determine if the bounding box crosses between -180 and 180
            {
                _crossesInternationalDateLine = true;
            }
            _centre = new MapCoordinate(CoordinateHelpers.NormaliseLongitude((_northwest.Longitude + _southeast.Longitude) / 2), (_northwest.Latitude + southeast.Latitude) / 2);
            _widthDegrees = CoordinateHelpers.NormaliseBearing(_northwest.Longitude - _southeast.Longitude);
            _heightDegress = _northwest.Latitude - southeast.Latitude;

        }
        /// <summary>
        /// Determine if the specifed point is within the bounding box
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool containsPoint(MapCoordinate point)
        {
            if (_crossesInternationalDateLine)//If the bounding box crosses from -180 to 180 
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
                _height = CoordinateHelpers.Haversine(_northwest.Latitude,middle, _southeast.Latitude, middle); //calculate the distance between the north and south points of the bounding box in metres
            }
            return _height;
        }
        /// <summary>
        /// converts the bounding box to a string "###.###, ###.### - ###.###, ###.###"
        /// </summary>
        /// <returns>String that represnts the bounding box</returns>
        public override string ToString()
        {
            return northwest.ToString() + " - " + _southeast.ToString();
        }
        /// <summary>
        /// Determines if the bounding box refers to the same area
        /// </summary>
        /// <param name="obj">The MapBoundingBox to compare to</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            MapBoundingBox MC = obj as MapBoundingBox;
            return MC.northwest.Equals(northwest) && MC.southeast.Equals(southeast);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// The southeast point of the box
        /// </summary>
        public MapCoordinate southeast { get => _southeast; }
        /// <summary>
        /// The northwest point of the box
        /// </summary>

        public MapCoordinate northwest { get => _northwest; }
        /// <summary>
        /// The width of the box in kilometers
        /// </summary>
        public double width { get => calculateWidth();  }
        /// <summary>
        /// The height of the box in kilometers
        /// </summary>
        public double height { get => calculateHeight(); }
        /// <summary>
        /// The width of the box in deegrees
        /// </summary>
        public double WidthDegrees { get => _widthDegrees;  }
        /// <summary>
        /// The height of the box in degrees
        /// </summary>
        public double HeightDegress { get => _heightDegress;  }
    }
}