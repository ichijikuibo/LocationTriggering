using LocationTriggering.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace LocationTriggering
{
    /// <summary>
    /// Root class for storing the locations that is returned in the GPS events
    /// </summary>
    public abstract class LocationTrigger : INotifyPropertyChanged
    {
        
        private MapBoundingBox _boundingBox;
        private List<MapCoordinate> _points = new List<MapCoordinate>();
        private MapCoordinate _centre;  //Calculated centre property that is assigned a value when _points is updated
        private MapCoordinate _distanceCalcualtedFrom;
        private double _distance;
        private double _bearingFrom;
        private MapCoordinate _bearingCalculatedFrom;
        private bool _clockwise = false;
        private bool _crossesSouthPole = false;
        private bool _crossesNorthPole = false;
        private bool _crossesDateLine = false;


        protected string _locationID;

        public event PropertyChangedEventHandler PropertyChanged;

        public string LocationID { get => _locationID;}
        /// <summary>
        /// A MapCoordinate that contains the centre point of the location 
        /// </summary>
        public MapCoordinate Centre { get => _centre; private set { _centre = value; } }
        /// <summary>
        /// A bounding box that contains all the points
        /// </summary>
        public MapBoundingBox BoundingBox { get => _boundingBox;}
        /// <summary>
        /// Returns the current number of points in the location
        /// </summary>
        /// <returns></returns>
        public int NumberOfPoints { get=> _points.Count;  }


        public IReadOnlyList<MapCoordinate> Points { get => _points.AsReadOnly(); }
        public double LastDistance { get=>_distance; private set { _distance = value; OnPropertyChanged(); } }

        public double LastBearing { get => _bearingFrom; private set => _bearingFrom = value; }

        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="id">a unique identifier for the location</param>
        protected LocationTrigger(string id)
        {
            _locationID = id;
        }
        /// <summary>
        /// Constructor that creates a location from an xmlnode 
        /// <Location ID="1"><Polygon><point long="-7.319639897566911" Lat="54.9971575645664"/></Polygon></Location>
        /// </summary>
        /// <param name="node"></param>
        protected LocationTrigger(XmlNode node)
        {
            foreach (XmlAttribute XMLA in node.Attributes)
            {
                if (XMLA.Name.ToLower() == "id") _locationID = XMLA.InnerText;
            }        
            
            foreach (XmlNode XN in node.ChildNodes)
            {
                if (XN.Name.ToLower() == "centrepoint")
                {
                    double lat = 0;
                    double lng = 0;
                    foreach(XmlAttribute point in XN.Attributes)
                    {                        
                        if (point.Name.ToLower() == "lng" || point.Name.ToLower() == "longitude") lng = double.Parse(point.InnerText);
                        if (point.Name.ToLower() == "lat" || point.Name.ToLower() == "latitude") lat = double.Parse(point.InnerText);
                    }
                    _centre = new MapCoordinate(lat, lng);
                }
                if (XN.Name.ToLower() == "polygon")
                {
                    for (int i = 0; i < XN.ChildNodes.Count; i++)
                    {
                        double lat = 0;
                        double lng = 0;
                        foreach (XmlAttribute point in XN.Attributes)
                        {

                            if (point.Name.ToLower() == "lng" || point.Name.ToLower() == "longitude") lng = double.Parse(point.InnerText);
                            if (point.Name.ToLower() == "lat" || point.Name.ToLower() == "latitude") lat = double.Parse(point.InnerText);
                        }
                        _points.Add(new MapCoordinate(lat,lng));

                    }
                }
            }
            CalculateProperties();
        }
        protected LocationTrigger(string id,string coordinates,char latLngSplit=',', char pointSplit=' ',bool longitudeFirst = false)
        {
            _locationID = id;
            if (pointSplit == '\n')
            {
                coordinates = coordinates.TrimEnd(pointSplit).TrimEnd('\r').TrimEnd(pointSplit).Replace("\r", "\n").Replace("\n\n", "\n");
            }
            else
            {
                coordinates = coordinates.TrimEnd(pointSplit);
            }
            string[] splitCoordinates = coordinates.Split(pointSplit);
            foreach (string s in splitCoordinates)
            {
                string latLng = s;
                if (latLngSplit != ' ') latLng = s.Replace(" ", "");
                string[] splitCoordinate = latLng.Split(latLngSplit);

                MapCoordinate newCoordinate;
                if(longitudeFirst)
                    newCoordinate = new MapCoordinate(double.Parse(splitCoordinate[1]), double.Parse(splitCoordinate[0]));
                else newCoordinate = new MapCoordinate(double.Parse(splitCoordinate[0]), double.Parse(splitCoordinate[1]));
                if (!Contains(newCoordinate))_points.Add(newCoordinate);
            }
            CalculateProperties();
        }


        /// <summary>
        /// Creates a new location with an id and a list of MapCoordinates
        /// </summary>
        /// <param name="id">The id of the new location</param>
        /// <param name="points">A list of MapCoordinates to add to the new location</param>
        protected LocationTrigger(string id, IEnumerable<MapCoordinate> points)
        {

            _locationID = id;
            _points.AddRange(points);
            CalculateProperties();
        }
        /// <summary>
        /// Gets the point at index, returns null if invalid
        /// </summary>
        /// <param name="index">0 based index of the MapCoordinate</param>
        /// <returns></returns>
        public MapCoordinate GetPoint(int index)
        {
            if (_points.Count > index)
                return _points[index];
            else return null;
        }
        protected bool Contains(MapCoordinate point)
        {
            foreach (MapCoordinate MC in _points)
            {
                if (MC.Equals(point)) return true;
            }
            return false;
        }
        /// <summary>
        /// Gets all the points of this location that are within boundingBox
        /// </summary>
        /// <param name="boundingBox">The bounding box to search for points in </param>
        /// <returns></returns>
        public MapCoordinate[] GetPointsInBoundingBox(MapBoundingBox boundingBox)
        {
            List<MapCoordinate> result = new List<MapCoordinate>();
            foreach(MapCoordinate MC in _points)
            {
                if (boundingBox.ContainsPoint(MC)) result.Add(MC);
            }
            if(result.Count>0)
                return result.ToArray();
            return null;
        }
        /// <summary>
        /// Return an array of MapCoordinates from this location that are contained in the Polygon
        /// </summary>
        /// <param name="polygon">The polygo to check for points in</param>
        /// <returns></returns>
        //public MapCoordinate[] GetPointsInPolygon(Polygon polygon)
        //{
        //    List<MapCoordinate> result = new List<MapCoordinate>();
        //    foreach (MapCoordinate MC in _points)
        //    {
        //        if (polygon.PointInPolygon(MC.Longitude, MC.Latitude)) result.Add(MC);
        //    }
        //    if (result.Count > 0)
        //        return result.ToArray();
        //    return null;
        //}
        /// <summary>
        /// Returns true if any of the poitns of the 2 locations are within the polygon for the other
        /// </summary>
        /// <param name="location">Location to check for overlaps with</param>
        /// <returns></returns>
        public bool OverlapsWith(LocationTrigger location)
        {
            if (HasAPointIn(location.Points)) return true;
            if (location.HasAPointIn(Points)) return true;
            MapCoordinate MC = this.ClosestPointTo(new MapCoordinate(Centre.Latitude,Centre.Longitude));
            if (ContainsPoint(MC)) return true;
            return false;
        }
        public bool HasAPointIn(IEnumerable<MapCoordinate> polygon)
        {
            foreach (MapCoordinate point in Points)
            {
                if (ContainsPoint(point)) return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true if any of the poitns of the 2 locations are within the polygon for the other
        /// </summary>
        /// <param name="polygon">Polygon to check for overlaps with</param>
        /// <returns></returns>
        //public bool OverlapsWith(Polygon polygon)
        //{
        //    if (polygon.HasAPointIn(_polygon)) return true;
        //    if (_polygon.HasAPointIn(polygon)) return true;
        //    MapCoordinate MC = this.ClosestPointTo(new MapCoordinate(polygon.Centre.Y, polygon.Centre.X));
        //    if (polygon.PointInPolygon(MC.Longitude, MC.Latitude))return true;
        //    return false;
        //}
        /// <summary>
        /// Creates a new MapCoordinate and adds it to the location
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void AddPoint(double latitude,double longitude)
        {
            AddPoint(new MapCoordinate(latitude,longitude));
        }
        /// <summary>
        /// Adds a Mapcoordinate to the location 
        /// </summary>
        /// <param name="newCoordinate">The MapCoordinate to add</param>
        public void AddPoint(MapCoordinate newCoordinate)
        {
            _points.Add(newCoordinate);            
            CalculateProperties();
        }
        /// <summary>
        /// Add a range of MapCoordinates
        /// </summary>
        /// <param name="points">A list of MapCoordinates to add</param>
        public void AddRange(IEnumerable<MapCoordinate> points)
        {
            _points.AddRange(points);
            CalculateProperties();
        }
        /// <summary>
        /// Removes the MapCoordinate at index
        /// </summary>
        /// <param name="index">0 based index of the point to be removed</param>
        public void RemovePoint(int index)
        {
            _points.RemoveAt(index);
            CalculateProperties();
        }
        /// <summary>
        /// Removes the MapCoordinate from the location
        /// </summary>
        /// <param name="mapCoordinate"></param>
        public void RemovePoint(MapCoordinate mapCoordinate)
        {
            _points.Remove(mapCoordinate);
            CalculateProperties();
        }
        /// <summary>
        /// Remove a range of poitns from the location
        /// </summary>
        /// <param name="start">0 based index to start removing at</param>
        /// <param name="count">The number of points to remove</param>
        public void RemoveRange(int start,int count)
        {
            _points.RemoveRange(start, count);
            CalculateProperties();
        }
        /// <summary>
        /// Clears the points and resets the calcualted properties
        /// </summary>
        public void ClearPoints()
        {
            _boundingBox = null;
            _points.Clear();
        }
        /// <summary>
        /// First checks if the point is within the bounding box of the location if true it then tests if it is within the polygon
        /// </summary>
        /// <param name="point">The point being checked</param>
        /// <returns></returns>
        public virtual bool ContainsPoint(MapCoordinate point)
        {
            if (_boundingBox==null||_boundingBox.ContainsPoint(point))
            {
                if (_clockwise)
                    return (AngleSum(point) < -0.000001);
                else
                    return (AngleSum(point) > 0.000001);
            }
            return false;
        }
       private double AngleSum2(MapCoordinate point)
        {
            // if (_crossesInternationalDateLine && X < 0) X = 360 + X;
            // Get the angle between the point and the
            // first and last vertices.
            int max_point = Points.Count - 1;
            double total_angle = CoordinateHelpers.GetAngle(
                Points[max_point].Latitude, CoordinateHelpers.AngleDifference(_centre.Longitude, Points[max_point].Longitude)/2,//_centre.Longitude+CoordinateHelpers.AngleDifference(_centre.Longitude,Points[max_point].Longitude),
                point.Latitude,  CoordinateHelpers.AngleDifference(_centre.Longitude, point.Longitude)/2,
                Points[0].Latitude, + CoordinateHelpers.AngleDifference(_centre.Longitude, Points[0].Longitude)/2);
            // Add the angles from the point
            // to each other pair of vertices.
            for (int i = 0; i < max_point; i++)
            {
                double angle1 = CoordinateHelpers.GetAngle(Points[i].Latitude, CoordinateHelpers.AngleDifference(_centre.Longitude, Points[i].Longitude)/2 ,
                    point.Latitude, CoordinateHelpers.AngleDifference(_centre.Longitude, point.Longitude) /2,
                    Points[i + 1].Latitude, CoordinateHelpers.AngleDifference(_centre.Longitude, Points[i + 1].Longitude)/2 ) ;
                total_angle += angle1;
            }

            // The total angle should be 2 * PI or -2 * PI if 6.2831853071795862 -6.2831853071795853
            // the point is in the polygon and close to zero
            // if the point is outside the polygon.
            return total_angle;

        }
        //30, -10
        //35,45
        //40, 90
        //35,135
        //30, -170
        //-30, -170
        //-35,135
        //-40, 90
        //-35,45
        //-30, -10

        private double AngleSum(MapCoordinate point)
        {
            // if (_crossesInternationalDateLine && X < 0) X = 360 + X;
            // Get the angle between the point and the
            // first and last vertices.
            int max_point = Points.Count - 1;
            double total_angle = CoordinateHelpers.GetAngle(
                Points[max_point].Latitude, Points[max_point].Longitude,//_centre.Longitude+CoordinateHelpers.AngleDifference(_centre.Longitude,Points[max_point].Longitude),
                point.Latitude, point.Longitude,
                Points[0].Latitude, Points[0].Longitude);
            // Add the angles from the point
            // to each other pair of vertices.
            for (int i = 0; i < max_point; i++)
            {
                double angle1 = CoordinateHelpers.GetAngle(Points[i].Latitude, Points[i].Longitude, point.Latitude, point.Longitude, Points[i + 1].Latitude, Points[i + 1].Longitude);
                total_angle += angle1;
            }

            // The total angle should be 2 * PI or -2 * PI if 6.2831853071795862 -6.2831853071795853
            // the point is in the polygon and close to zero
            // if the point is outside the polygon.
            return total_angle;

        }
        /// <summary>
        /// Returns the distance from the centre of this location to the specified point
        /// override if you want to chang this to ClosestDistanceTo
        /// </summary>
        /// <param name="point">Point to measure distance from</param>
        /// <returns>Distance in kilometres</returns>
        public virtual double DistanceTo(MapCoordinate point, DistanceUnit unit = DistanceUnit.Kilometres)
        {
            if (_distanceCalcualtedFrom==null||!point.Equals(_distanceCalcualtedFrom))
            {
                LastDistance = _centre.DistanceTo(point, unit);
                _distanceCalcualtedFrom = point;
            }
            return LastDistance;
        }
        /// <summary>
        /// Returns the distance from the centre of this location to the centre of the specified location
        /// </summary>
        /// <param name="point">Location to measure distance from</param>
        /// <returns>Distance in meters</returns>
        public virtual double DistanceTo(LocationTrigger point, DistanceUnit unit = DistanceUnit.Kilometres)
        {
            return DistanceTo(point.Centre, unit);
        }
        /// <summary>
        /// Returns the closest distance to the boundary of this location to the specified point
        /// </summary>
        /// <param name="point">Location to measure distance from</param>
        /// <returns>Distance in kilometres</returns>
        public double ClosestDistanceTo(MapCoordinate point, DistanceUnit unit = DistanceUnit.Kilometres)
        {
            return (point.DistanceTo(ClosestPointTo(point), unit));
        }
        /// <summary>
        /// Returns the closest distance to the boundary of this location to the boundary of the specified location
        /// </summary>
        /// <param name="point">Location to measure distance from</param>
        /// <returns>Distance in kilometres</returns>
        public double ClosestDistanceTo(LocationTrigger location, DistanceUnit unit = DistanceUnit.Kilometres)
        {
            MapCoordinate ClosestPoint1 = ClosestPointTo(location.Centre);
            MapCoordinate ClosestPoint2 = location.ClosestPointTo(ClosestPoint1);
            return ClosestPoint1.DistanceTo(ClosestPoint2, unit);
        }
        /// <summary>
        /// Returns the point closest tot the specified point on the location's boundary
        /// </summary>
        /// <param name="point">Point to measure distance from</param>
        /// <returns>Position of the closest point in decimal degrees</returns>
        //public MapCoordinate ClosestPointTo(MapCoordinate point)
        //{
        //    PointD closestPoint = Polygon.ClosestPointTo(new PointD(point.Longitude, point.Latitude));
        //    return new MapCoordinate(closestPoint.Y, closestPoint.X);
        //}
        public MapCoordinate ClosestPointTo(MapCoordinate point)
        {
            MapCoordinate ClosestPoint = new MapCoordinate(0,0);
            double ClosestDistance = 999999999;
            for (int i = 0; i < Points.Count; i++)
            {
                MapCoordinate P1 = _points[i];
                MapCoordinate P2;
                MapCoordinate CurrentPoint;
                double CurrentDistance;

                if (i == Points.Count - 1)
                    P2 = Points[0];
                else
                    P2 = Points[i + 1];
                CurrentDistance = CoordinateHelpers.FindDistanceToSegment(point, P1, P2, out CurrentPoint);//                { 59.3250680383031, -91.5341802003566}
                if (CurrentDistance < ClosestDistance)
                {
                    ClosestPoint = CurrentPoint;
                    ClosestDistance = CurrentDistance;
                }
            }
            return ClosestPoint;
        }

        /// <summary>
        /// Gets a ranges of bearings that the location is visible from a point (cannot exceed 180 degrees)
        /// Uses a formula that may not cover the entire location at close distances, override to change this to Polygon.BearingRangeFrom or MapBoundingBox.BearingRangeFrom if required
        /// </summary>
        /// <param name="point">The point to calculate the bearings from</param>
        /// <returns>A bearing range containg the minimum and maximum bearings</returns>
        public virtual BearingRange BearingRangeFrom(MapCoordinate point)
        {
            double centreBearing = point.BearingTo(Centre);
            double guideDistance = BoundingBox.Width;
            if (BoundingBox.Height > guideDistance) guideDistance = BoundingBox.Height;
            if (guideDistance > 6000) guideDistance = 6000;
            double targetBearing1 = CoordinateHelpers.NormaliseBearing(centreBearing + 90);
            double targetBearing2 = CoordinateHelpers.NormaliseBearing(centreBearing - 90);
            MapCoordinate Point1 = ClosestPointTo(CoordinateHelpers.DestinationPointFromBearingAndDistance(Centre,guideDistance, targetBearing1));//{54.9964314712174, -7.32574279029166}
            MapCoordinate Point2 = ClosestPointTo(CoordinateHelpers.DestinationPointFromBearingAndDistance(Centre, guideDistance, targetBearing2));//{54.9953895258887, -7.32721765393637}
            double start = point.BearingTo(Point2);
            double end = point.BearingTo(Point1);
            //BearingRange BoundingBoxRange = BoundingBox.BearingRangeFrom(point);
            BearingRange BR = new BearingRange(start, end);
            double differenceFromCentre = CoordinateHelpers.AngleDifference(BR.Centre, centreBearing);
            if (differenceFromCentre > 60|| differenceFromCentre<-60)
                BR = new BearingRange(end, start);
            //if(BR.Range> BoundingBoxRange.Range) BR = new BearingRange(end, start);
            return BR;
        }
        public virtual double BearingFrom(MapCoordinate point)
        {
            if (_bearingCalculatedFrom==null||!point.Equals(_bearingCalculatedFrom))
            {
                _bearingCalculatedFrom = point;
                LastBearing = point.BearingTo(Centre);
            }

            return LastBearing;
        }
        /// <summary>
        /// Calculate the centre point, and bounding box when the points list is updated
        /// </summary>
        private void CalculateProperties()
        {
            if (_points.Count < 3) return; //At least 3 points are required to calculate the properties
            _centre = CentralPoint();
            _clockwise = AngleSum(_centre) < 0;
            _crossesNorthPole = ContainsPoint(new MapCoordinate(90, 0));
            _crossesSouthPole = ContainsPoint(new MapCoordinate(-90, 0));
            double MinLon = double.MaxValue, MaxLon = -double.MaxValue, MinLat = double.MaxValue, MaxLat = -double.MaxValue;
            if (_crossesNorthPole)
            {
                MinLat = double.MaxValue;
                MaxLat = double.MaxValue;
                MinLon = -180;
                MaxLon = 0;
                foreach (MapCoordinate P in _points)
                {
                    if(P.Longitude<0)
                    {
                        if (P.Latitude < MaxLat) MaxLat = P.Latitude;
                    }
                    else
                    {
                        if (P.Latitude < MinLat) MinLat = P.Latitude;
                    }
                }
            }
            else if (_crossesSouthPole)
            {
                MinLat = -double.MaxValue; 
                MaxLat = -double.MaxValue;
                MinLon = 0;
                MaxLon = 180;
                foreach (MapCoordinate P in _points)
                {
                    if (P.Longitude < 0)
                    {
                        if (P.Latitude > MinLat) MinLat = P.Latitude;
                    }
                    else
                    {
                        if (P.Latitude > MaxLat) MaxLat = P.Latitude;
                    }
                }
            }
            else
            {
                foreach (MapCoordinate P in _points)
                {

                    //Iterate through the points and obtain the extremes of the polygon
                    double DistanceFromCentreLon = CoordinateHelpers.AngleSubtract(P.Longitude, _centre.Longitude);
                    double DistanceFromCentreLat = CoordinateHelpers.AngleSubtract(P.Latitude, _centre.Latitude);
                    if (DistanceFromCentreLon < MinLon)
                    {
                        MinLon = DistanceFromCentreLon;
                    }
                    if (DistanceFromCentreLon > MaxLon)
                    {
                        MaxLon = DistanceFromCentreLon;
                    }

                    if (DistanceFromCentreLat < MinLat)
                    {
                        MinLat = DistanceFromCentreLat;
                    }
                    if (DistanceFromCentreLat > MaxLat)
                    {
                        MaxLat = DistanceFromCentreLat;
                    }
                }
                if (_centre.Longitude + MaxLon>180)
                {
                    _crossesDateLine = true;
                }
                if (_centre.Longitude + MinLon < -180)
                {
                    _crossesDateLine = true;
                }
                MinLon = CoordinateHelpers.AngleAddition(_centre.Longitude, MinLon);
                MaxLon = CoordinateHelpers.AngleAddition(_centre.Longitude, MaxLon);
                MinLat = CoordinateHelpers.AngleAddition(_centre.Latitude, MinLat);
                MaxLat = CoordinateHelpers.AngleAddition(_centre.Latitude, MaxLat);
            }
            _boundingBox = new MapBoundingBox(new MapCoordinate(MaxLat, MinLon), new MapCoordinate(MinLat, MaxLon), _crossesDateLine,_crossesSouthPole,_crossesNorthPole);//create a bounding box from the northeast point and the southwest point
            if (!_crossesNorthPole&&!_crossesSouthPole&&ContainsPoint(_boundingBox.Centre))
                _centre = BoundingBox.Centre;
            if (!ContainsPoint(_centre)) _centre = ClosestPointTo(_centre);
            OnPropertyChanged("Centre");
        }
        private MapCoordinate CentralPoint()
        {
            if (Points.Count == 1)
            {
                return Points[0];
            }

            double x = 0;
            double y = 0;
            double z = 0;
            foreach (var point in Points)
            {
                var latitude = point.Latitude * Math.PI / 180;
                var longitude = point.Longitude * Math.PI / 180;
                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }


            var total = Points.Count;
            x = x / total;
            y = y / total;
            z = z / total;

            if (Math.Abs(x) < 10e-9) x = 0;
            if (Math.Abs(y) < 10e-9) y = 0;
            if (Math.Abs(z) < 10e-9) z = 0;
            var centralLongitude = Math.Atan2(y,x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            

            return new MapCoordinate( centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
