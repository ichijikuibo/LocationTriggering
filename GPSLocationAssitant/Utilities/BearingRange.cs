using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    public class BearingRange:Object
    {
        private double _start;
        private double _end;
        private double _range;
        public BearingRange(double start,double end)
        {
            _start = start;
            _end = end;
            if (_start > _end)
            {
                double temp = _start;
                _start = _end;
                _end = temp;
            }
            if (_start < 0) _start = 360 + _start;
            if (_end < 0) _end = 360 + _end;
            if (_end - _start > 180)
            {
                _range = 360 - _end + _start;
            }
            else
            {
                _range = _end - _start;
            }
        }

        public double Start { get => _start; }
        public double Range { get => _range;  }
        public double End { get => _end; }

        public bool ContainsBearing(double Bearing)
        {
            if (_end - _start > 180)
            {
                if (Bearing > _end || Bearing < _start)
                {
                    return true;
                }
            }
            else
            {
                if (Bearing > _start && Bearing < _end)
                {
                    return true;
                }
            }
            return false;
        }
        public override string ToString()
        {
            return _start + " - " + _end; 
        }
        public override bool Equals(object obj)
        {
            if (!(obj is BearingRange)) return false;
            BearingRange BR = obj as BearingRange;
            return BR.Start == _start && BR.End == _end;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
