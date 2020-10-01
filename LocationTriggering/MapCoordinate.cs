using LocationTriggering.Utilities;
using Plugin.Geolocator.Abstractions;
using System;
namespace LocationTriggering
{
    /// <summary>
    /// Stores a GPS point using 2 doubles and contains methods for calculations on GPS Points
    /// </summary>
    public class MapCoordinate : object
    {
        private double _latitude; //Vertical/South - North/Y location in decimal degrees with a range of -90 to 90 
        private double _longitude; //Horizontal/West - East/X location in decimal degrees with a range of -180 to 180

        /// <summary>
        /// Latitude value in decimal degrees
        /// </summary>
        public double Latitude { get => _latitude; }
        /// <summary>
        /// Longitude value in decimal degrees
        /// </summary>
        public double Longitude { get => _longitude; }

        /// <summary>
        /// Default constructor initilises the class with new latitude and longitude figures.
        /// </summary>
        /// <param name="newLogitude">Logitude in deciaml degrees</param>
        /// <param name="newLatitude">Latitude in deciaml degrees</param>
        public MapCoordinate(double newLatitude, double newLogitude)
        {
            if (newLatitude < -90 || newLatitude > 90) throw new InvalidCoordinateException("Invalid Coordinate latitude must be greater or equal to -90 or less then or equal to 90");
            if (double.IsNaN(newLatitude)|| double.IsNaN(newLogitude)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            if (double.IsInfinity(newLatitude) || double.IsInfinity(newLogitude)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            _latitude = newLatitude;
            _longitude = CoordinateHelpers.NormaliseLongitude(newLogitude);
        }

        public MapCoordinate(Position location)
        {
            if (location.Latitude < -90 || location.Latitude > 90) throw new InvalidCoordinateException("Invalid Coordinate latitude must be greater or equal to -90 or less then or equal to 90");
            if (double.IsNaN(location.Latitude) || double.IsNaN(location.Longitude)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            if (double.IsInfinity(location.Latitude) || double.IsInfinity(location.Longitude)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            _latitude = location.Latitude;
            _longitude = CoordinateHelpers.NormaliseLongitude(location.Longitude);
        }
        public MapCoordinate(PointD point)
        {
            if (point.Y < -90 || point.Y > 90) throw new InvalidCoordinateException("Invalid Coordinate latitude must be greater or equal to -90 or less then or equal to 90");
            if (double.IsNaN(point.X) || double.IsNaN(point.Y)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            if (double.IsInfinity(point.Y) || double.IsInfinity(point.X)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            _latitude = point.Y;
            _longitude = CoordinateHelpers.NormaliseLongitude(point.X);
        }

        /// <summary>
        /// Uses a haversine function to get the distance between this point and another in meters
        /// </summary>
        /// <param name="destinationPoint">The point calcualte the distance to</param>
        /// <returns>Distance in kilometers</returns>        
        public double DistanceTo(MapCoordinate destinationPoint)
        {
            return CoordinateHelpers.Haversine(_latitude, _longitude, destinationPoint.Latitude, destinationPoint.Longitude);
        }
        /// <summary>
        /// uses a haversine function to get the distance between this point and another in feet
        /// </summary>
        /// <param name="destinationPoint">The point calcualte the distance to</param>
        /// <returns>Distance in feet</returns>
        public double DistanceToFeet(MapCoordinate destinationPoint)
        {
            return CoordinateHelpers.Haversine(_latitude, _longitude, destinationPoint.Latitude, destinationPoint.Longitude) * 1000 * 3.280839895;
        }
        /// <summary>
        /// uses a haversine function to get the distance between this point and another in miles
        /// </summary>
        /// <param name="destinationPoint">The point calcualte the distance to</param>
        /// <returns>Distance in miles</returns>
        public double DistanceToMiles(MapCoordinate destinationPoint)
        {
            return CoordinateHelpers.Haversine(_latitude, _longitude, destinationPoint.Latitude, destinationPoint.Longitude) * 0.62137119;
        }
        /// <summary>
        /// Returns the bearing in degrees from the current point to a destination point
        /// </summary>
        /// <param name="destinationPoint">The point calcualte the bearing to</param>
        /// <returns>Bearing in degrees</returns>
        public double BearingTo(MapCoordinate destinationPoint)
        {
            return CoordinateHelpers.GetAngle(Latitude, Longitude, destinationPoint.Latitude, destinationPoint.Longitude);
        }
        /// <summary>
        /// Returns the bearing in degrees to the current point from an origin point
        /// </summary>
        /// <param name="destinationPoint">The point calcualte the bearing from</param>
        /// <returns>Bearing in degrees</returns>        
        public double BearingFrom(MapCoordinate originPoint)
        {
            return CoordinateHelpers.GetAngle(originPoint.Latitude, originPoint.Longitude, Latitude, Longitude);
        }

        /// <summary>
        /// Find a point that is a specified distance away from the current point in a specifed bearing
        /// </summary>
        /// <param name="distance">Distance to measure in kilonetres</param>
        /// <param name="bearing">Direction in degrees(0-360)</param>
        /// <returns>Point that is "distance" away in the direction of "bearing" </returns>
        public MapCoordinate PointAtDistanceAndBearing(double distance,double bearing)
        {
            PointD result = CoordinateHelpers.DestinationPointFromBearingAndDistance(new PointD(Longitude,Latitude), distance, bearing);
            return new MapCoordinate(result.Y, result.X);
        }
        /// <summary>
        /// Check if the current point refers to the same location as other MapCoorddinate 
        /// </summary>
        /// <param name="otherMapCoorddinate">Coordinate to check for equality to</param>
        /// <returns>True if Coordinates are equal</returns>
        //
        public bool Equals(MapCoordinate otherMapCoordinate)
        {
            if ((_latitude == 90 && otherMapCoordinate.Latitude == 90) || (_latitude == -90 && otherMapCoordinate.Latitude == -90)) return true;
            return _latitude == otherMapCoordinate.Latitude && CoordinateHelpers.NormaliseLongitude(_longitude) == CoordinateHelpers.NormaliseLongitude(otherMapCoordinate.Longitude);
        }

        /// <summary>
        /// Converts the MapCoordinate to PointD with X being the Longitude and Y being the latitude
        /// </summary>
        /// <returns>PointD with the same latitude and longitude</returns>
        public PointD ToPointD()
        {
            return new PointD(Longitude, Latitude);
        }
        /// <summary>
        /// Converts the MapCoordinate to Geolocator Position
        /// </summary>
        /// <returns>A location with the same latitude and longitude</returns>
        public Position ToPosition()
        {
            return new Position(Latitude, Longitude);
        }
        /// <summary>
        /// Coverts MapCoorddinate to a string format "Latitude, Longitude"
        /// </summary>
        /// <returns>String with coorinates in deciaml degrees</returns>
        public override string ToString()
        {
            return _latitude + ", " + _longitude;
        }
    }
}
