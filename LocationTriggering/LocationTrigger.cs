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
        private TriggerType _locationType = TriggerType.Polygon;
        private double _radius;


        protected string _locationID;

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// The Id of the location
        /// </summary>
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

        /// <summary>
        /// The points that make up the trigger
        /// </summary>
        public IReadOnlyList<MapCoordinate> Points { get => _points.AsReadOnly(); }
        /// <summary>
        /// The Last distance calcualted for the trigger
        /// </summary>
        public double LastDistance { get=>_distance; private set { _distance = value; OnPropertyChanged(); } }
        /// <summary>
        /// The last bearing calcualted for the trigger
        /// </summary>
        public double LastBearing { get => _bearingFrom; private set { _bearingFrom = value; OnPropertyChanged(); } }
        /// <summary>
        /// the type of the location
        /// </summary>
        public TriggerType LocationType { get => _locationType;
            set {
                _locationType = value;
                CalculateProperties();
            } }
        /// <summary>
        /// The radius of the circle or thickness of the polyline
        /// </summary>
        public double Radius
        {
            get => _radius; 
            set
            {
                _radius = value;
                CalculateProperties();
            }
        }

        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="id">a unique identifier for the location</param>
        protected LocationTrigger(string id)
        {
            _locationID = id;
        }
        /// <summary>
        /// Construct a new trigger from a string containing the coordinates
        /// </summary>
        /// <param name="id">ID of the trigger</param>
        /// <param name="coordinates">String of Coordinates</param>
        /// <param name="latLngSplit">The character that splits the latitude and longitude</param>
        /// <param name="pointSplit">The character that splits the coordinates</param>
        /// <param name="longitudeFirst">True if coordinate is Longitude,Latitude false if it is Latitude,Longitude</param>
        /// <param name="newType">The type of the trigger</param>
        /// <param name="newRadius">In a radial coordinate this is the radius of the circle, for a polyline its the thickness of the line, its not used for a polygon</param>
        protected LocationTrigger(string id,string coordinates,char latLngSplit=',', char pointSplit=' ',bool longitudeFirst = false, TriggerType newType=TriggerType.Polygon,double newRadius=0)
        {
            _locationType = newType;
            _radius = newRadius;
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
        protected LocationTrigger(string id, IEnumerable<MapCoordinate> points, TriggerType newType = TriggerType.Polygon, double newRadius = 0)
        {
            _locationType = newType;
            _radius = newRadius;
            _locationID = id;
            _points.AddRange(points);
            CalculateProperties();
        }
        /// <summary>
        /// Creates a new location with an id and a list of MapCoordinates to create a Polyline type location
        /// </summary>
        /// <param name="id">The id of the new location</param>
        /// <param name="points">A list of MapCoordinates to add to the new location</param>
        /// <param name="radius">The radius in km of the circle</param>
        protected LocationTrigger(string id, MapCoordinate point, double radius)
        {
            _radius = radius;
            _locationType = TriggerType.Radial;
            _locationID = id;
            _points.Add(point);
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
        /// <summary>
        /// Returns true if one of the points of the trigger matches the specified point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
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
        /// Returns true if any of the poitns of the 2 locations are within the polygon for the other
        /// </summary>
        /// <param name="location">Location to check for overlaps with</param>
        /// <returns></returns>
        public bool OverlapsWith(LocationTrigger location)
        {
            if (HasAPointIn(location.Points)) return true;
            if (location.HasAPointIn(Points)) return true;
            MapCoordinate MC = this.ClosestPointTo(location.Centre);
            if (ContainsPoint(MC)) return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
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
        public virtual bool ContainsPoint(MapCoordinate point,bool absolute = false)
        {
            if (_locationType == TriggerType.Polygon)
            {
                if (_boundingBox == null || _boundingBox.ContainsPoint(point))
                {
                    if (absolute)
                        return (Math.Abs(AngleSum(point)) > 0.000001);
                    if (_clockwise)
                        return (AngleSum(point) < -0.000001);
                    else
                        return (AngleSum(point) > 0.000001);
                }
            }
            if(_locationType==TriggerType.Radial)
            {
                foreach (MapCoordinate mc in _points)
                {
                    double distance = point.DistanceTo(mc);
                    if (distance<=_radius) return true;
                }
            }
            if (_locationType == TriggerType.Polyline)
            {
                if (ClosestDistanceTo(point) <= _radius) return true;
            }
            return false;
        }
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
            if (_distanceCalcualtedFrom == null || !point.Equals(_distanceCalcualtedFrom))
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
            if (_locationType == TriggerType.Radial)
            {
                foreach(MapCoordinate mc in _points)
                {
                    double distanceTo = mc.DistanceTo(point);
                    if (distanceTo < ClosestDistance)
                    {
                        ClosestDistance = distanceTo;
                        ClosestPoint = mc;
                    }
                }
                ClosestPoint = CoordinateHelpers.DestinationPointFromBearingAndDistance(ClosestPoint, _radius, point.BearingFrom(ClosestPoint));
            }
            else
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    if (i == Points.Count - 1 && _locationType == TriggerType.Polyline) continue;
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
            }
            if (_locationType == TriggerType.Polyline)
            {
                ClosestPoint = CoordinateHelpers.DestinationPointFromBearingAndDistance(ClosestPoint, _radius, point.BearingFrom(ClosestPoint));
            }
            return ClosestPoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public virtual BearingRange BearingRangeFrom(MapCoordinate point,BearingRangeType mode = BearingRangeType.Default)
        {
            if(mode==BearingRangeType.BoundingBox)
            {
                return _boundingBox.BearingRangeFrom(point);
            }
            if (_locationType == TriggerType.Radial)
            {
                return BearingRangeRadial(point);
            }
            if(mode==BearingRangeType.Points||_locationType==TriggerType.Polyline)
            {
                return BearingRangeFromPoints(point);
            }
            return BearingRangeFromDefault(point);
        }

        /// <summary>
        /// Gets a ranges of bearings that the location is visible from a point (cannot exceed 180 degrees)
        /// Uses a formula that may not cover the entire location at close distances, override to change this to Polygon.BearingRangeFrom or MapBoundingBox.BearingRangeFrom if required
        /// </summary>
        /// <param name="point">The point to calculate the bearings from</param>
        /// <returns>A bearing range containg the minimum and maximum bearings</returns>
        protected virtual BearingRange BearingRangeFromDefault(MapCoordinate point)
        {
            double centreBearing = point.BearingTo(Centre);
            double centreBearingFrom = point.BearingFrom(Centre);
            double guideDistance = BoundingBox.Width;
            if (BoundingBox.Height > guideDistance) guideDistance = BoundingBox.Height;
            if (guideDistance > 10000) guideDistance = 10000;
            double targetBearing1 = CoordinateHelpers.NormaliseBearing(centreBearingFrom + 90);
            double targetBearing2 = CoordinateHelpers.NormaliseBearing(centreBearingFrom - 90);
            MapCoordinate Point1 = ClosestPointTo(CoordinateHelpers.DestinationPointFromBearingAndDistance(Centre,guideDistance, targetBearing1));//{50.285669812737773, -103.05873332721963}
            MapCoordinate Point2 = ClosestPointTo(CoordinateHelpers.DestinationPointFromBearingAndDistance(Centre, guideDistance, targetBearing2));//{54.9188979176421, -264.68226572650019}
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected virtual BearingRange BearingRangeFromPoints(MapCoordinate point)
        {
            double centreBearing = point.BearingTo(_centre);
            double start = 360;
            double end = -360;
            foreach (MapCoordinate p in Points)
            {
                double angle = point.BearingTo(p);
                double difference = CoordinateHelpers.AngleDifference(centreBearing, angle);
                if (difference < start) start = difference;
                if (difference > end) end = difference;

            }
            start += centreBearing;
            end += centreBearing;
            return new BearingRange(start, end);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected virtual BearingRange BearingRangeRadial(MapCoordinate point)
        {
            double centreBearing = point.BearingTo(_centre);
            double start = 360, end = -360;
            MapCoordinate startMC = Points[0];
            MapCoordinate endMC = Points[0];
            foreach (MapCoordinate p in Points)
            {
                double angle = point.BearingTo(p);
                double difference = CoordinateHelpers.AngleDifference(centreBearing, angle);
                if (difference < start)
                {
                    start = difference;
                    startMC = p;
                }
                if (difference > end)
                {
                    end = difference;
                    endMC = p;
                }

            }
            startMC = startMC.PointAtDistanceAndBearing(_radius, startMC.BearingTo(point) + 90);
            endMC = endMC.PointAtDistanceAndBearing(_radius, endMC.BearingTo(point) - 90);
            start = point.BearingTo(startMC);
            end += point.BearingTo(endMC); 
            return new BearingRange(start, end);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        private void CalculateProperties()
        {
            if(_locationType == TriggerType.Polygon)
            {
                CalculatePropertiesPolygon();
            }
            if (_locationType == TriggerType.Radial)
            {
                CalculatePropertiesRadial();
            }
            if (_locationType == TriggerType.Polyline)
            {
                CalculatePropertiesPolyline();
            }
        }

        /// <summary>
        /// Calculate the centre point, and bounding box when the points list is updated
        /// </summary>
        private void CalculatePropertiesRadial()
        {
            if (_points.Count == 0) return; //At least 3 points are required to calculate the properties
            _centre = _points[0];
            if (_points.Count > 1)
            {
                _centre = CentralPoint();
            }
            double MinLon = double.MaxValue, MaxLon = -double.MaxValue, MinLat = double.MaxValue, MaxLat = -double.MaxValue;
            MapCoordinate left = _points[0], top = _points[0], right = _points[0], bottom = _points[0];
            foreach (MapCoordinate P in _points)
            {
                //Iterate through the points and obtain the extremes of the polygon
                double DistanceFromCentreLon = CoordinateHelpers.AngleSubtract(P.Longitude, _centre.Longitude);
                double DistanceFromCentreLat = CoordinateHelpers.AngleSubtract(P.Latitude, _centre.Latitude);
                if (DistanceFromCentreLon < MinLon)
                {
                    left = P;
                    MinLon = DistanceFromCentreLon;
                }
                if (DistanceFromCentreLon > MaxLon)
                {
                    right = P;
                    MaxLon = DistanceFromCentreLon;
                }

                if (DistanceFromCentreLat < MinLat)
                {
                    bottom = P;
                    MinLat = DistanceFromCentreLat;
                }
                if (DistanceFromCentreLat > MaxLat)
                {
                    top = P;
                    MaxLat = DistanceFromCentreLat;
                }
            }

            MinLon = left.PointAtDistanceAndBearing(_radius, 270).Longitude;
            MaxLon = right.PointAtDistanceAndBearing(_radius, 90).Longitude;
            MinLat = bottom.PointAtDistanceAndBearing(_radius, 180).Latitude;
            MaxLat = top.PointAtDistanceAndBearing(_radius, 0).Latitude;
            if (MinLat-MaxLat < -180|| MinLat - MaxLat > 180)
            {
                _crossesDateLine = true;
            }
            _crossesNorthPole = ContainsPoint(new MapCoordinate(90, 0));
            _crossesSouthPole = ContainsPoint(new MapCoordinate(-90, 0));
            _boundingBox = new MapBoundingBox(new MapCoordinate(MaxLat, MinLon), new MapCoordinate(MinLat, MaxLon), _crossesDateLine, _crossesNorthPole, _crossesSouthPole);//create a bounding box from the northeast point and the southwest point
            OnPropertyChanged("Centre");
        }
        /// <summary>
        /// Calculate the centre point, and bounding box when the points list is updated
        /// </summary>
        private void CalculatePropertiesPolyline()
        {
            if (_points.Count < 2) return; //At least 3 points are required to calculate the properties
            if (_points[0].Equals(_points[_points.Count - 1])) _points.RemoveAt(_points.Count - 1);
            _centre = CentralPoint();
            _centre = ClosestPointTo(_centre);
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
                    if (P.Longitude < 0)
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
                if (_centre.Longitude + MaxLon > 180)
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
            _boundingBox = new MapBoundingBox(new MapCoordinate(MaxLat, MinLon), new MapCoordinate(MinLat, MaxLon), _crossesDateLine, _crossesSouthPole, _crossesNorthPole);//create a bounding box from the northeast point and the southwest point
            if (!_crossesNorthPole && !_crossesSouthPole && ContainsPoint(_boundingBox.Centre))
                _centre = ClosestPointTo(_centre);
            OnPropertyChanged("Centre");
        }

        /// <summary>
        /// Calculate the centre point, and bounding box when the points list is updated
        /// </summary>
        private void CalculatePropertiesPolygon()
        {
            if (_points.Count < 3) return; //At least 3 points are required to calculate the properties
            if (_points[0].Equals(_points[_points.Count - 1])) _points.RemoveAt(_points.Count - 1);
            _centre = CentralPoint();
            if (!ContainsPoint(_centre, true))
            {
                MapCoordinate newCentre = ClosestPointTo(_centre);
                double newLat = newCentre.Latitude;
                double newLng = newCentre.Longitude;
                if (newCentre.Longitude - _centre.Longitude > 0) 
                    newLng += 0.00001;
                else 
                    newLng -= 0.00001;
                if (newCentre.Latitude - _centre.Latitude > 0) 
                    newLat += 0.00001;
                else 
                    newLat -= 0.00001;

                _centre = new MapCoordinate(newLat,newLng);
            }
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
            if (!ContainsPoint(_centre, true))
            {
                MapCoordinate newCentre = ClosestPointTo(_centre);
                double newLat = newCentre.Latitude;
                double newLng = newCentre.Longitude;
                if (newCentre.Longitude - _centre.Longitude > 0)
                    newLng += 0.00001;
                else
                    newLng -= 0.00001;
                if (newCentre.Latitude - _centre.Latitude > 0)
                    newLat += 0.00001;
                else
                    newLat -= 0.00001;

                _centre = new MapCoordinate(newLat, newLng);
            }
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

//-38.22088578757306,137.0966687221523	
//-41.14699039869232,140.7612796790796	
//-45.04385654271243,148.3872680855425	
//-42.24179445780681,150.6105643998689	
//-38.32190852403915,152.1014846698897	
//-33.89799364036446,155.3469909177914	
//-28.2874936691238,155.0570339726316	
//-23.7704830553687,153.7832215087749	
//-21.77847625328914,151.5481740679563	
//-19.55314919190631,150.0743420356932	
//-16.4719197121896,148.0976078680809	
//-12.84354524549978,145.9945297340139	
//-10.33326425528148,143.8111869597975	
//-10.72205525360104,141.3334382076673	
//-13.30767885192975,140.4202503581765	
//-15.56152160293079,138.8003168012125	
//-14.73240204142622,137.2715710235034	
//-10.8708950199417,137.8909261323328	
//-9.829804478915685,135.5704295562607	
//-10.38699118944958,129.9702616119519	
//-12.52722083690712,125.3678818352159	
//-14.37105493488749,121.9743265059042	
//-16.87571478485158,117.0330684275972	
//-19.2497279871944,113.0512470589866	
//-24.0717763197421,111.0930608341264	
//-27.67669163023321,111.4384453281121	
//-31.79289631001624,112.2517232308155	
//-35.78486437095834,113.2697362694079	
//-35.28754504637568,121.005790388567	
//-33.52778111634026,124.687129311849	
//-34.00024705162931,134.4421596618281	
