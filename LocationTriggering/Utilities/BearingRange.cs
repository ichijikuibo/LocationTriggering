using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    public class BearingRange: object
    {
        private double _start;
        private double _end;
        private double _range;
        private double _centre;
        /// <summary>
        /// Default construct takes 2 bearings normalises them to 0-360 and determines range and centre
        /// </summary>
        /// <param name="start">The start bearing of the range</param>
        /// <param name="end">The end bearing of the range</param>
        public BearingRange(double start,double end)
        {
            _start = CoordinateHelpers.NormaliseBearing(start);
            _end = CoordinateHelpers.NormaliseBearing(end);
            //if the _start are equal then the range covers the full 360 degrees
            if (_start == _end) _range = 360;
            _range = CoordinateHelpers.NormaliseBearing(end- start);
            _centre = CoordinateHelpers.NormaliseBearing(_start + _range / 2);
        }
        /// <summary>
        /// The normalised starting bearing of the range
        /// </summary>
        public double Start { get => _start; }
        /// <summary>
        /// The range of the bearing from 0-360
        /// </summary>
        public double Range { get => _range;  }
        /// <summary>
        /// The normalised ending bearing of the range
        /// </summary>
        public double End { get => _end; }
        /// <summary>
        /// The centre point of the range
        /// </summary>
        public double Centre { get => _centre; set => _centre = value; }

        /// <summary>
        /// Returns true of the specified bearing is within the range
        /// </summary>
        /// <param name="Bearing">The bearing to check if the range contains</param>
        /// <returns>True of false depending on result of the method </returns>
        public bool ContainsBearing(double Bearing)
        {
            Bearing = CoordinateHelpers.NormaliseBearing(Bearing);
            if (_range!= _end-_start)//if the range passes 0 then a different method to determine if the bearing is within the range is required
            {
                if (Bearing >= Start || Bearing <= End) return true;
            }
            else
            {
                if(Bearing>=Start&& Bearing<=End) return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true if the specifed bearing range overlaps with the range
        /// </summary>
        /// <param name="bearingRange">A bearing range to test for overlaps with</param>
        /// <returns>True of false depending on result of the method </returns>
        public bool OverlapsWith(BearingRange bearingRange)
        {
            //if there is an overlap then either the start or the end of the either range must be within the range of the other
            if (bearingRange.ContainsBearing(Start)) return true;
            if (bearingRange.ContainsBearing(Centre)) return true;
            if (bearingRange.ContainsBearing(End)) return true;

            if (ContainsBearing(bearingRange.Start)) return true;
            if (ContainsBearing(bearingRange.Centre)) return true;
            if (ContainsBearing(bearingRange.End)) return true;

            return false;
        }
        /// <summary>
        /// Converts the range to a string format ###.### - ###.###
        /// </summary>
        /// <returns>String that represents the range of the bearings</returns>
        public override string ToString()
        {
            return _start + " - " + _end; 
        }
        /// <summary>
        /// Tests if the bearing is equal to a specified bearing
        /// </summary>
        /// <param name="obj">A Bearing range to test for equality</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BearingRange)) return false; //return false if the object isn't a bearing range
            BearingRange BR = obj as BearingRange;
            return BR.Start == _start && BR.End == _end;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
