using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace LocationTriggering.Extentions
{
    public static class LocationExtention
    {
        public static MapCoordinate ToMapCoordinate(this Location baseCoordinate)
        {
            return new MapCoordinate(baseCoordinate.Latitude, baseCoordinate.Longitude);
        }
        public static double CalculateDistance(this Location baseCoordinate, MapCoordinate coordinate, DistanceUnits units)
        {
            return baseCoordinate.CalculateDistance(coordinate.ToLocation(), units);
        }
    }
}