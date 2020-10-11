using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    /// <summary>
    /// Contains static methods to help with making calculations with gps coordinates
    /// </summary>
    public static class CoordinateHelpers
    {
        
        const double RADIUS_EARTH = 6371;//the mean radius of the earth in KM used for distance calculations
        /// <summary>
        /// Method for calculating the distance between 2 gps points only accurate to 4 S.f. 
        /// </summary>
        /// <param name="lat1">Latitude of point 1</param>
        /// <param name="lon1">Longitude of Point 1</param>
        /// <param name="lat2">Latitude of point 2</param>
        /// <param name="lon2">Longitude of point 2</param>
        /// <returns>Distance in Kilometres</returns>
        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {

             var R = RADIUS_EARTH; 
            var dLat = toRadians(lat2 - lat1);
            var dLon = toRadians(lon2 - lon1);
            lat1 = toRadians(lat1);
            lat2 = toRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * 2 * Math.Asin(Math.Sqrt(a));
        }
        /// <summary>
        /// Convert degrees in radians
        /// </summary>
        /// <param name="angle">the angle in degrees</param>
        /// <returns></returns>
        public static double toRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns></returns>
        public static double toDegrees(double angle)
        {
            return angle/ Math.PI * 180.0;
        }
        /// <summary>
        /// Returns the north facing bearing between 2 map coordinates in degrees from 0 to 360
        /// </summary>
        /// <param name="lat1">Latitude of point 1</param>
        /// <param name="lon1">Longitude of Point 1</param>
        /// <param name="lat2">Latitude of point 2</param>
        /// <param name="lon2">Longitude of point 2</param>
        /// <returns>Bearing in degrees 0-360 </returns>        
        public static double GetAngle(double lat1, double lon1, double lat2, double lon2)
        {
            //convert degrees to radians
            lat1 = toRadians(lat1);
            lon1 = toRadians(lon1);
            lat2 = toRadians(lat2);
            lon2 = toRadians(lon2);
            double y = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) -
                      Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);
            double a = Math.Atan2(y, x);
            //convert result back to degrees and normalise
            double brng = (a * 180 / Math.PI + 360) % 360; 
            return brng;
        }
        public static double GetAngle(double latA, double lonA, double latB, double lonB, double latC, double lonC)
        {
            double Angle1 = GetAngle(latB, lonB, latA, lonA);
            double Angle2 = GetAngle(latB, lonB, latC, lonC);
            double Difference = AngleDifference(Angle1, Angle2);
            return Difference;
        }
        /// <summary>
        /// Calculate the difference between 2 angles
        /// </summary>
        /// <param name="angle1">First angle to compare</param>
        /// <param name="angle2">Second angle to commpare</param>
        /// <returns>angle between 0 and 360</returns>
        public static double AngleDifference(double angle1, double angle2)
        {
            double diff = (angle2 - angle1 + 180) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }
        /// <summary>
        /// Normalise a longitude value to within -180 and 180
        /// </summary>
        /// <param name="longitude">Longitude value to normalise</param>
        /// <returns>Longitude in deciaml degree between -180 and 180</returns>
        /// 



        public static double NormaliseLongitude(double longitude)
        {
            double result = (longitude + 540) % 360 - 180;
            if (result == -180) result = 180;
            return result;
        }
        /// <summary>
        /// Normalise a bearing to a valuie with 0 and 360
        /// </summary>
        /// <param name="bearing">the bearing to be normalised</param>
        /// <returns>A bearing value between 0 and 360</returns>
        public static double NormaliseBearing(double bearing)
        {
            return (bearing + 360) % 360;
        }
        /// <summary>
        /// Get a point on the perimeter at a specifed entrance angle of a rectangle based on the width and height
        /// </summary>
        /// <param name="angle">The entrance angle </param>
        /// <param name="w">The width of the rectangle</param>
        /// <param name="h">The height of the rectangle</param>
        /// <returns>a point on the perimeter of the rectangle</returns>
        //public static PointD getPointOnRect(double angle, double w, double h)
        //{
        //    double TargetAngle = angle;
        //    double Adj = 0;
        //    double Opp = 0;
        //    double WidthAngle = (Math.Atan((w / 2) / (h / 2)) / Math.PI * 180) * 2;
        //    double HeigthAngle = (360 - WidthAngle * 2) / 2;
        //    if (TargetAngle > WidthAngle / 2 && TargetAngle <= WidthAngle / 2 + HeigthAngle)//right
        //    {
        //        Adj = w / 2;
        //        TargetAngle = 90 - TargetAngle;
        //        Opp = Math.Tan(TargetAngle / 180 * Math.PI) * Adj;
        //        return new PointD(Adj, Opp);

        //    }
        //    else if (TargetAngle <= WidthAngle / 2 + WidthAngle + HeigthAngle)//bottom
        //    {
        //        Adj = h / -2;
        //        TargetAngle = 180 - TargetAngle;
        //        Opp = Math.Tan(TargetAngle / 180 * Math.PI) * -Adj;

        //        return new PointD(Opp, Adj);
        //    }
        //    else if (TargetAngle <= WidthAngle / 2 + HeigthAngle * 2 + WidthAngle)//left
        //    {
        //        Adj = w / -2;
        //        TargetAngle = 270 - TargetAngle;
        //        Opp = Math.Tan(TargetAngle / 180 * Math.PI) * Adj;
        //        return new PointD(Adj, Opp);

        //    }
        //    else//top
        //    {
        //        Adj = h / 2;
        //        if (TargetAngle > 45) TargetAngle = -360 + TargetAngle;
        //        Opp = Math.Tan(TargetAngle / 180 * Math.PI) * Adj;
        //        return new PointD(Opp, Adj);
        //    }

        //}
        /// <summary>
        /// Perfforma a 2d vector rotations
        /// </summary>
        /// <param name="Vector">The vecotr to rotate</param>
        /// <param name="degrees">the angle in degrees to rotate</param>
        /// <returns>A vector rotated by the specifed degrees</returns>
        //public static PointD RotateVector2d(PointD Vector, double degrees)
        //{
        //    double rad = degrees / 180 * Math.PI;
        //    return new PointD(
        //        Vector.X * Math.Cos(rad) - Vector.Y * Math.Sin(rad),
        //        Vector.X * Math.Sin(rad) + Vector.Y * Math.Cos(rad)
        //        );
        //}
        //from https://www.movable-type.co.uk/scripts/latlong.html
        /// <summary>
        /// Determine a point that is a specfied distacne away in the specified direction
        /// </summary>
        /// <param name="initialPoint">Starting point</param>
        /// <param name="kilometres">Distacne in kilometres</param>
        /// <param name="bearing">The direction to move</param>
        /// <returns>The latitude and logitude of the new point PointD.Y = latitude PointD.X = longitude</returns>
        public static MapCoordinate DestinationPointFromBearingAndDistance(MapCoordinate initialPoint,double kilometres, double bearing )
        {
            double R = RADIUS_EARTH;
            double d = kilometres;
            double lat1 = toRadians(initialPoint.Latitude);
            double lon1 = toRadians(initialPoint.Longitude);
            double brng = toRadians(bearing);
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d / R) + Math.Cos(lat1) * Math.Sin(d / R) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2( Math.Sin(brng) * Math.Sin(d / R) * Math.Cos(lat1), Math.Cos(d / R) - Math.Sin(lat1) * Math.Sin(lat2));
            return new MapCoordinate(toDegrees(lat2), toDegrees(lon2));
        }
        //from http://csharphelper.com/blog/2016/09/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c/
        /// <summary>
        /// Calculate the distance to a line segment from a point
        /// </summary>
        /// <param name="pt">Point to calculate from</param>
        /// <param name="p1">Starting point of the line</param>
        /// <param name="p2">Ending point of the line</param>
        /// <param name="closest">Output: the closest point determined by the method</param>
        /// <returns></returns>
        public static double FindDistanceToSegment2(MapCoordinate pt, MapCoordinate p1, MapCoordinate p2, out MapCoordinate closest)
        {
            double dx = /*AngleSubtract*/(p2.Longitude - p1.Longitude);
            double dy = /*AngleSubtract*/(p2.Latitude - p1.Latitude);
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = AngleDifference(pt.Longitude , p1.Longitude);
                dy = AngleDifference(pt.Latitude , p1.Latitude);
                return Math.Sqrt(AngleAddition(dx * dx , dy * dy));
            }

            //// Calculate the t that minimizes the distance.
            //double t = ((pt.Longitude - p1.Longitude) + dx + (pt.Latitude - p1.Latitude) * dy) /
            //    (dx * dx + dy * dy);
            double t = ((pt.Latitude - p1.Latitude) * dx + (pt.Longitude - p1.Longitude) * dy) /
    (dx * dx + dy * dy);
            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new MapCoordinate(p1.Latitude, p1.Longitude);
            }
            else if (t > 1)
            {
                closest = new MapCoordinate(p2.Latitude, p2.Longitude);
            }
            else
            {
                closest = new MapCoordinate(/*AngleAddition*/(p1.Latitude + t * dy), /*AngleAddition*/(p1.Longitude + t * dx));
            }

            return Haversine(pt.Latitude, pt.Longitude, closest.Latitude, closest.Longitude);
        }
        public static double FindDistanceToSegment(MapCoordinate pt, MapCoordinate p1, MapCoordinate p2, out MapCoordinate closest)
        {
            double dx = AngleSubtract(p2.Longitude , p1.Longitude);
            double dy = AngleSubtract(p2.Latitude , p1.Latitude);
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.Longitude - p1.Longitude;
                dy = pt.Latitude - p1.Latitude;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = (AngleSubtract(pt.Longitude , p1.Longitude) * dx + AngleSubtract(pt.Latitude , p1.Latitude) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new MapCoordinate(p1.Latitude,p1.Longitude);
                dx = pt.Longitude - p1.Longitude;
                dy = pt.Latitude - p1.Latitude;
            }
            else if (t > 1)
            {
                closest = new MapCoordinate(p2.Latitude,p2.Longitude );
                dx = pt.Longitude - p2.Longitude;
                dy = pt.Latitude - p2.Latitude;
            }
            else
            {
                closest = new MapCoordinate(p1.Latitude + t * dy, p1.Longitude + t * dx);
                dx = pt.Longitude - closest.Longitude;
                dy = pt.Latitude - closest.Latitude;
            }

            return Haversine(pt.Latitude, pt.Longitude, closest.Latitude, closest.Longitude);
        }
        public static double AngleSubtract(double angle1, double angle2)
        {
            double diff = (angle1 - angle2 + 180) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }
        public static double AngleAddition(double angle1, double angle2)
        {
            double diff = (angle1 + angle2 + 180) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }
    }
}
