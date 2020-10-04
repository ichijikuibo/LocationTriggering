using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace LocationTriggering.Extentions
{
    public static class MapCoordinateExtention
    {
        public static Position ToPosition(this MapCoordinate baseCoordinate)
        {
            return new Plugin.Geolocator.Abstractions.Position(baseCoordinate.Latitude, baseCoordinate.Longitude);
        }
        public static MapCoordinate FromPosition(Position coordinate)
        {
            return new MapCoordinate(coordinate.Longitude, coordinate.Latitude);
        }
        public static Location ToLocation(this MapCoordinate baseCoordinate)
        {
            return new Location(baseCoordinate.Latitude, baseCoordinate.Longitude);
        }
        public static MapCoordinate FromLocation(Location coordinate)
        {
            return new MapCoordinate(coordinate.Longitude, coordinate.Latitude);
        }
        public static double DistanceTo(this MapCoordinate baseCoordinate, Position coordinate)
        {
            return baseCoordinate.DistanceTo(new MapCoordinate(coordinate.Latitude, coordinate.Longitude));
        }
        public static double DistanceTo(this MapCoordinate baseCoordinate, Location coordinate)
        {
            return baseCoordinate.DistanceTo(new MapCoordinate(coordinate.Latitude, coordinate.Longitude));
        }
        public static double BearingFrom(this MapCoordinate baseCoordinate, Location coordinate)
        {
            return baseCoordinate.BearingFrom(new MapCoordinate(coordinate.Latitude, coordinate.Longitude));
        }
        public static double BearingFrom(this MapCoordinate baseCoordinate, Position coordinate)
        {
            return baseCoordinate.BearingFrom(new MapCoordinate(coordinate.Latitude, coordinate.Longitude));
        }
        public static double BearingTo(this MapCoordinate baseCoordinate, Location coordinate)
        {
            return baseCoordinate.BearingTo(new MapCoordinate(coordinate.Latitude, coordinate.Longitude));
        }
        public static double BearingTo(this MapCoordinate baseCoordinate, Position coordinate)
        {
            return baseCoordinate.BearingTo(new MapCoordinate(coordinate.Latitude, coordinate.Longitude));
        }

    }
}
