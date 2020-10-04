using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Extentions
{
    public static class PositionExtention
    {
        public static MapCoordinate ToMapCoordinate(this Position baseCoordinate)
        {
            return new MapCoordinate(baseCoordinate.Latitude, baseCoordinate.Longitude);
        }
        public static double CalculateDistance(this Position baseCoordinate,MapCoordinate coordinate,GeolocatorUtils.DistanceUnits units)
        {
            return baseCoordinate.CalculateDistance(coordinate.ToPosition(), units);
        }
    }
}
