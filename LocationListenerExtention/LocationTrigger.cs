using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using LocationTriggering;
using Xamarin.Essentials;
using LocationTriggering.Utilities;

namespace LocationTriggering.Extentions
{
    public static class LocationTriggerExtention
    {
        public static double DistanceTo(this LocationTrigger baseTrigger,Position position)
        {
            return baseTrigger.DistanceTo(position.ToMapCoordinate());
        }
        public static double DistanceTo(this LocationTrigger baseTrigger, Location position)
        {
            return baseTrigger.DistanceTo(position.ToMapCoordinate());
        }
        public static double ClosestDistanceTo(this LocationTrigger baseTrigger, Position position)
        {
            return baseTrigger.ClosestDistanceTo(position.ToMapCoordinate());
        }
        public static double ClosestDistanceTo(this LocationTrigger baseTrigger, Location position)
        {
            return baseTrigger.ClosestDistanceTo(position.ToMapCoordinate());
        }
        public static MapCoordinate ClosestPointTo(this LocationTrigger baseTrigger, Position position)
        {
            return baseTrigger.ClosestPointTo(position.ToMapCoordinate());
        }
        public static MapCoordinate ClosestPointTo(this LocationTrigger baseTrigger, Location position)
        {
            return baseTrigger.ClosestPointTo(position.ToMapCoordinate());
        }
        public static double BearingFrom(this LocationTrigger baseTrigger, Location position)
        {
            return baseTrigger.BearingFrom(position.ToMapCoordinate());
        }
        public static double BearingFrom(this LocationTrigger baseTrigger, Position position)
        {
            return baseTrigger.BearingFrom(position.ToMapCoordinate());
        }
        public static BearingRange BearingRangeFrom(this LocationTrigger baseTrigger, Position position)
        {
            return baseTrigger.BearingRangeFrom(position.ToMapCoordinate());
        }
        public static BearingRange BearingRangeFrom(this LocationTrigger baseTrigger, Location position)
        {
            return baseTrigger.BearingRangeFrom(position.ToMapCoordinate());
        }
        public static bool ContainsPoint(this LocationTrigger baseTrigger, Position position)
        {
            return baseTrigger.ContainsPoint(position.ToMapCoordinate());
        }
        public static bool ContainsPoint(this LocationTrigger baseTrigger, Location position)
        {
            return baseTrigger.ContainsPoint(position.ToMapCoordinate());
        }

    }
}
