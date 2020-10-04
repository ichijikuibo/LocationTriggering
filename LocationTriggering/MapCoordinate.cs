using LocationTriggering.Utilities;
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
            if (double.IsNaN(newLatitude)|| double.IsNaN(newLogitude)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            if (double.IsInfinity(newLatitude) || double.IsInfinity(newLogitude)) throw new InvalidCoordinateException("Invalid Coordinate both latitude and longitude must have a value");
            _latitude = newLatitude;
            _longitude = newLogitude;
            if(newLatitude>90|| newLatitude<-90|| newLogitude<-180|| newLogitude>180)NormaliseCoordinate();
        }
        /// <summary>
        /// Uses a haversine function to get the distance between this point and returns in the specified DistanceUnit
        /// </summary>
        /// <param name="destinationPoint">The point calcualte the distance to</param>
        /// <returns>Distance in specified DistanceUnit default kilometres</returns>        
        public double DistanceTo(MapCoordinate destinationPoint, DistanceUnit unit = DistanceUnit.Kilometres)
        {
            return DistanceUnitConversion.FromKilometres(CoordinateHelpers.Haversine(_latitude, _longitude, destinationPoint.Latitude, destinationPoint.Longitude), unit);
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
        /// <param name="distance">Distance to measure in the specifed DistanceUnit</param>
        /// <param name="bearing">Direction in degrees(0-360)</param>
        /// <param name="unit">The distance unit that the distance is in default: kilometres</param>
        /// <returns>Point that is "distance" away in the direction of "bearing" </returns>
        public MapCoordinate PointAtDistanceAndBearing(double distance,double bearing, DistanceUnit unit=DistanceUnit.Kilometres)
        {
            MapCoordinate result = CoordinateHelpers.DestinationPointFromBearingAndDistance(this, DistanceUnitConversion.ToKilometres(distance, unit), bearing);
            return result;
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
        /// Coverts MapCoorddinate to a string format "Latitude, Longitude"
        /// </summary>
        /// <returns>String with coorinates in deciaml degrees</returns>
        public override string ToString()
        {
            return _latitude + ", " + _longitude;
        }
        private void NormaliseCoordinate()
        {
            _latitude = CoordinateHelpers.NormaliseLongitude(_latitude);
            bool flip = false;
            if (_latitude > 90.0)
            {
                _latitude = 180.0 - _latitude;
                flip = true;
            }
            else if (_latitude < -90.0)
            {
                _latitude = -180.0 - _latitude;
                flip = true;
            }
            if (flip)
            {
                _longitude += _longitude > 0 ? -180.0 : 180.0;
            }
            _longitude = CoordinateHelpers.NormaliseLongitude(_longitude);

        }
    }
}
