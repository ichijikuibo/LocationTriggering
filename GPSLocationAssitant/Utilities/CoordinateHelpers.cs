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
        /// <summary>
        /// Method for calculating the distance between 2 gps points
        /// </summary>
        /// <param name="lat1">Latitude of point 1</param>
        /// <param name="lon1">Longitude of Point 1</param>
        /// <param name="lat2">Latitude of point 2</param>
        /// <param name="lon2">Longitude of point 2</param>
        /// <returns></returns>
        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {

             var R = 6372.8; // Distance to centre of the earth in kilometers
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
        /// Returns the north facing bearing between 2 map coordinates in degrees from -0 to 360
        /// </summary>
        /// <param name="lat1">Latitude of point 1</param>
        /// <param name="lon1">Longitude of Point 1</param>
        /// <param name="lat2">Latitude of point 2</param>
        /// <param name="lon2">Longitude of point 2</param>
        /// <returns></returns>        
        public static double GetAngle(double lat1, double lon1, double lat2, double lon2)
        {
            double x = Math.Cos(toRadians(lat1)) * Math.Sin(toRadians(lat2)) - Math.Sin(toRadians(lat1)) * Math.Cos(toRadians(lat2)) * Math.Cos(toRadians(lon2 - lon1));
            double y = Math.Sin(toRadians(lon2 - lon1)) * Math.Cos(toRadians(lat2));

            // Math.Atan2 can return negative value, 0 <= output value < 2*PI expected 
            return (Math.Atan2(y, x) + Math.PI * 2) % (Math.PI * 2) / Math.PI * 180;
        }
        public static double AngleDifference(double angle1, double angle2)
        {
            double diff = (angle2 - angle1 + 180) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }
        public static double NormaliseLongitude(double longitude)
        {
            return (longitude + 540) % 360 - 180;
        }
        public static double NormaliseBearing(double bearing)
        {
            return (bearing + 360) % 360;
        }
        public static PointD getPointOnRect(double angle, double w, double h)
        {
            double TargetAngle = angle;
            double Adj = 0;
            double Opp = 0;
            double WidthAngle = (Math.Atan((w / 2) / (h / 2)) / Math.PI * 180) * 2;
            double HeigthAngle = (360 - WidthAngle * 2) / 2;
            if (TargetAngle > WidthAngle / 2 && TargetAngle <= WidthAngle / 2 + HeigthAngle)//right
            {
                Adj = w / 2;
                TargetAngle = 90 - TargetAngle;
                Opp = Math.Tan(TargetAngle / 180 * Math.PI) * Adj;
                return new PointD(Adj, Opp);

            }
            else if (TargetAngle <= WidthAngle / 2 + WidthAngle + HeigthAngle)//bottom
            {
                Adj = h / -2;
                TargetAngle = 180 - TargetAngle;
                Opp = Math.Tan(TargetAngle / 180 * Math.PI) * -Adj;

                return new PointD(Opp, Adj);
            }
            else if (TargetAngle <= WidthAngle / 2 + HeigthAngle * 2 + WidthAngle)//left
            {
                Adj = w / -2;
                TargetAngle = 270 - TargetAngle;
                Opp = Math.Tan(TargetAngle / 180 * Math.PI) * Adj;
                return new PointD(Adj, Opp);

            }
            else//top
            {
                Adj = h / 2;
                if (TargetAngle > 45) TargetAngle = -360 + TargetAngle;
                Opp = Math.Tan(TargetAngle / 180 * Math.PI) * Adj;
                return new PointD(Opp, Adj);
            }

        }

        public static PointD RotateVector2d(PointD Vector, double degrees)
        {
            double rad = degrees / 180 * Math.PI;
            return new PointD(
                Vector.X * Math.Cos(rad) - Vector.Y * Math.Sin(rad),
                Vector.X * Math.Sin(rad) + Vector.Y * Math.Cos(rad)
                );
        }
        //from https://www.movable-type.co.uk/scripts/latlong.html
        public static PointD DestinationPointFromBearingAndDistance(PointD initialPoint,double kilometres, double bearing )
        {
            double R = 6372.8;
            double d = kilometres;
            double lat1 = toRadians(initialPoint.Y);
            double lon1 = toRadians(initialPoint.X);
            double brng = toRadians(bearing);
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d / R) + Math.Cos(lat1) * Math.Sin(d / R) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2( Math.Sin(brng) * Math.Sin(d / R) * Math.Cos(lat1), Math.Cos(d / R) - Math.Sin(lat1) * Math.Sin(lat2));
            return new PointD(toDegrees(lon2), toDegrees(lat2));
        }
        //from http://csharphelper.com/blog/2016/09/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c/
        public static double FindDistanceToSegment(PointD pt, PointD p1, PointD p2, out PointD closest)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new PointD(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new PointD(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new PointD(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
