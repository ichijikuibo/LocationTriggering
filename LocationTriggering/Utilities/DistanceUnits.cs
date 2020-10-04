using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    public enum DistanceUnit
    {
        Kilometres,
        Miles,
        Metres,
        Feet,
        Yards,
        NauticalMiles
    }
    public static class DistanceUnitConversion
    {
        public static double FromKilometres(double kilometres,DistanceUnit unit)
        {
            double result = kilometres;
            switch(unit)
            {
                case DistanceUnit.Kilometres:
                    break;
                case DistanceUnit.Metres:
                    result = kilometres *(1/1000);
                    break;
                case DistanceUnit.Miles:
                    result = kilometres * (1 / 1.609344);
                    break;
                case DistanceUnit.Feet:
                    result = kilometres * (1000/0.3048);
                    break;
                case DistanceUnit.Yards:
                    result = kilometres * (1000 / 0.9144);
                    break;
                case DistanceUnit.NauticalMiles:
                    result = kilometres * (1/ 1.852);
                    break;
            }
            return result;
        }
        public static double ToKilometres(double length, DistanceUnit unit)
        {
            double result = length;
            switch (unit)
            {
                case DistanceUnit.Kilometres:
                    break;
                case DistanceUnit.Metres:
                    result = length * 1000;
                    break;
                case DistanceUnit.Miles:
                    result = length * 1.609344;
                    break;
                case DistanceUnit.Feet:
                    result = length * 0.3048/1000;
                    break;
                case DistanceUnit.Yards:
                    result = length *  0.9144/1000;
                    break;
                case DistanceUnit.NauticalMiles:
                    result = length * 1.852;
                    break;
            }
            return result;
        }
    }
}

