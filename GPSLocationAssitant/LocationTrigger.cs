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
        
        private Polygon _polygon;
        private MapBoundingBox _boundingBox;
        private List<MapCoordinate> _points = new List<MapCoordinate>();
        private double _width; //Calculated width property in decimal degrees that is assigned a value when _points is updated
        private double _height; //Calculated heigth property in decimal degrees that is assigned a value when _points is updated
        private double _widthMeters; //Calculated width property in meters that is assigned a value when _points is updated
        private double _heightMeters; //Calculated height property in meters that is assigned a value when _points is updated
        private MapCoordinate _centre;  //Calculated centre property that is assigned a value when _points is updated
        private double _bearing; //Calculated 360 degree bearing staring from north that is assigned a value when _points is updated
        private MapCoordinate _distanceCalcualtedFrom;
        private double _distance;
        private double _bearingFrom;
        private MapCoordinate _bearingCalculatedFrom;
        

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
        /// A polygon used to calculate if a point is contianed within the list of points
        /// </summary>
        public Polygon Polygon { get => _polygon; }
        /// <summary>
        /// The estimated height of the location in decimal degrees not the same as BoundingBox.Height 
        /// </summary>
        public double Height { get => _height; }
        /// <summary>
        /// The estimated Width of the location in decimal degrees not the same as BoundingBox.Height 
        /// </summary>
        public double Width { get => _width; }
        /// <summary>
        /// Returns the current number of points in the location
        /// </summary>
        /// <returns></returns>
        public int NumberOfPoints { get=> _points.Count;  }
        /// <summary>
        /// The estimated bearing of the polygon
        /// </summary>
        public double PolygonBearing { get => _bearing;  }
        /// <summary>
        /// The estimated height of the location in metres not the same as BoundingBox.Height 
        /// </summary>
        public double HeightMeters { get => _heightMeters; }
        /// <summary>
        /// The estimated Width of the location in metres not the same as BoundingBox.Height 
        /// </summary>
        public double WidthMeters { get => _widthMeters; }


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
                    PointD[] Points = new PointD[XN.ChildNodes.Count];
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
        protected LocationTrigger(string id,string coordinates,char latLngSplit=',', char pointSplit=' ')
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
                MapCoordinate newCoordinate = new MapCoordinate(double.Parse(splitCoordinate[1]), double.Parse(splitCoordinate[0]));
                if(!Contains(newCoordinate))_points.Add(newCoordinate);
            }
            CalculateProperties();
        }
        private bool Contains(MapCoordinate point)
        {
            foreach(MapCoordinate MC in _points)
            {
                if (MC.Equals(point)) return true;
            }
            return false;
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
                if (boundingBox.containsPoint(MC)) result.Add(MC);
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
        public MapCoordinate[] GetPointsInPolygon(Polygon polygon)
        {
            List<MapCoordinate> result = new List<MapCoordinate>();
            foreach (MapCoordinate MC in _points)
            {
                if (polygon.PointInPolygon(MC.Longitude, MC.Latitude)) result.Add(MC);
            }
            if (result.Count > 0)
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
            return OverlapsWith(location.Polygon);
        }
        /// <summary>
        /// Returns true if any of the poitns of the 2 locations are within the polygon for the other
        /// </summary>
        /// <param name="polygon">Polygon to check for overlaps with</param>
        /// <returns></returns>
        public bool OverlapsWith(Polygon polygon)
        {
            if (polygon.HasAPointIn(_polygon)) return true;
            if (_polygon.HasAPointIn(polygon)) return true;
            MapCoordinate MC = this.ClosestPointTo(new MapCoordinate(polygon.Centre.Y, polygon.Centre.X));
            if (polygon.PointInPolygon(MC.Longitude, MC.Latitude))return true;
            return false;
        }
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
            _polygon = null;
            _width = 0;
            _height = 0;
            _boundingBox = null;
            _points.Clear();
        }

        /// <summary>
        /// First checks if the point is within the bounding box of the location if true it then tests if it is within the polygon
        /// </summary>
        /// <param name="point">The point being checked</param>
        /// <returns></returns>
        public bool ContainsPoint(MapCoordinate point)
        {
            if(_boundingBox.containsPoint(point))
            {
                if (_polygon.PointInPolygon(point.Longitude, point.Latitude)) return true;
            }
            return false;
        }
        /// <summary>
        /// Returns the distance from the centre of this location to the specified point
        /// </summary>
        /// <param name="point">Point to measure distance from</param>
        /// <returns>Distance in meters</returns>
        public double DistanceTo(MapCoordinate point)
        {
            if (_distanceCalcualtedFrom==null||!point.Equals(_distanceCalcualtedFrom))
            {
                LastDistance = _centre.DistanceTo(point);
                _distanceCalcualtedFrom = point;
            }
            return LastDistance;
        }
        /// <summary>
        /// Returns the distance from the centre of this location to the centre of the specified location
        /// </summary>
        /// <param name="point">Location to measure distance from</param>
        /// <returns>Distance in meters</returns>
        public double DistanceTo(LocationTrigger point)
        {
            return DistanceTo(point.Centre);
        }
        /// <summary>
        /// Returns the closest distance to the boundary of this location to the specified point
        /// </summary>
        /// <param name="point">Location to measure distance from</param>
        /// <returns>Distance in meters</returns>
        public double ClosestDistanceTo(MapCoordinate point)
        {
            return (point.DistanceTo(ClosestPointTo(point)));
        }
        /// <summary>
        /// Returns the closest distance to the boundary of this location to the boundary of the specified location
        /// </summary>
        /// <param name="point">Location to measure distance from</param>
        /// <returns>Distance in meters</returns>
        public double ClosestDistanceTo(LocationTrigger location)
        {
            MapCoordinate ClosestPoint1 = ClosestPointTo(location.Centre);
            MapCoordinate ClosestPoint2 = location.ClosestPointTo(ClosestPoint1);
            return ClosestPoint1.DistanceTo(ClosestPoint2);
        }
        /// <summary>
        /// Returns the point closest tot the specified point on the location's boundary
        /// </summary>
        /// <param name="point">Point to measure distance from</param>
        /// <returns>Position of the closest point in decimal degrees</returns>
        public MapCoordinate ClosestPointTo(MapCoordinate point)
        {
            PointD closestPoint = Polygon.ClosestPointTo(new PointD(point.Longitude, point.Latitude));
            return new MapCoordinate(closestPoint.Y, closestPoint.X);
        }
        /// <summary>
        /// Gets a ranges of bearings that the location is visible from a point (cannot exceed 180 degrees)
        /// </summary>
        /// <param name="point">The point to calculate the bearings from</param>
        /// <returns>A bearing range containg the minimum and maximum bearings</returns>
        public BearingRange BearingRangeFrom(MapCoordinate point)
        {
            double centreBearing = point.BearingTo(Centre);
            double guideDistance = BoundingBox.width;
            if (BoundingBox.height > guideDistance) guideDistance = BoundingBox.height;
            double targetBearing1 = CoordinateHelpers.NormaliseBearing(centreBearing + 90);
            double targetBearing2 = CoordinateHelpers.NormaliseBearing(centreBearing - 90);
            MapCoordinate Point1 = ClosestPointTo(Centre.PointAtDistanceAndBearing(guideDistance, targetBearing1));
            MapCoordinate Point2 = ClosestPointTo(Centre.PointAtDistanceAndBearing(guideDistance, targetBearing2));
            double start = point.BearingTo(Point1);
            double end = point.BearingTo(Point2);

            //PointD Rotated = GetPointOnPermiterFrom(point).ToPointD();
            //PointD CentrePointD = _centre.ToPointD();
            //PointD Point1 = new PointD(CentrePointD.X - Rotated.X / Math.Cos(CentrePointD.Y / 180 * Math.PI), CentrePointD.Y - Rotated.Y);
            //PointD Point2 = new PointD(CentrePointD.X + Rotated.X / Math.Cos(CentrePointD.Y / 180 * Math.PI), CentrePointD.Y + Rotated.Y);
            //double start = CoordinateHelpers.GetAngle(point.Latitude,point.Longitude, Point1.Y,Point1.X);
            //double end = CoordinateHelpers.GetAngle(point.Latitude, point.Longitude, Point2.Y,Point2.X);
            return new BearingRange(start, end);
        }
        public double BearingFrom(MapCoordinate point)
        {
            if (_bearingCalculatedFrom==null||!point.Equals(_bearingCalculatedFrom))
            {
                _bearingCalculatedFrom = point;
                LastBearing = point.BearingTo(Centre);
            }

            return LastBearing;
        }
        public MapCoordinate GetPointOnPermiterFrom(MapCoordinate point)
        {
            double TargetAngle = CoordinateHelpers.GetAngle(_centre.Latitude, _centre.Longitude, point.Latitude, point.Longitude);
            TargetAngle -= _bearing + 90;
            if (TargetAngle < 0) TargetAngle = 360 + TargetAngle;
            if (TargetAngle > 360)
                TargetAngle %= 360;
            PointD PointOnPerimeter = CoordinateHelpers.getPointOnRect(TargetAngle, _width, _height);
            PointD Rotated = CoordinateHelpers.RotateVector2d(PointOnPerimeter, -(_bearing));
            return new MapCoordinate(Rotated.Y, Rotated.X);
        }
        /// <summary>
        /// Creates the polygon when the points are changed
        /// </summary>
        private void RecreatePolygon()
        {
            PointD[] pointsArray = new PointD[_points.Count];
            for (int i = 0; i < _points.Count; i++)
            {
                pointsArray[i] = new PointD(_points[i].Longitude, _points[i].Latitude);
            }
            _polygon = new Polygon(pointsArray);
        }
        /// <summary>
        /// Calculate the centre point, width,height,bearing and bounding box when the points list is updated
        /// </summary>
        private void CalculateProperties()
        {
            //recreate the polygon class for determining if a point if within the location
            RecreatePolygon();
            if (_points.Count < 3) return; //At least 3 points are required to calcualte the properties
            //Varibles to help calcualte the properties 
            double MinLon = double.MaxValue, MaxLon = -double.MaxValue, MinLat = double.MaxValue, MaxLat = -double.MaxValue;
            MapCoordinate Zero = new MapCoordinate(0, 0); //Provides an initial value for the points
            MapCoordinate TopPoint = Zero, RightPoint = Zero, LeftPoint = Zero, BottomPoint = Zero; //used to determine the width, height and bearing of the polygon
            List<MapCoordinate> TopPoints = new List<MapCoordinate>();
            List<MapCoordinate> BottomPoints = new List<MapCoordinate>();
            List<MapCoordinate> LeftPoints = new List<MapCoordinate>();
            List<MapCoordinate> RightPoints = new List<MapCoordinate>();
            foreach (MapCoordinate P in _points)
            {

                //Iterate through the points and obtain the extremes of the polygon
                if (Polygon.CrossesInternationalDateLine)
                {

                    if (P.Longitude > 0)
                    {
                        if (P.Longitude < MinLon)
                        {
                            MinLon = P.Longitude;
                            LeftPoint = P;
                        }

                    }
                    else
                    {
                        if (P.Longitude > MaxLon)
                        {
                            MaxLon = P.Longitude;
                            RightPoint = P;
                        }
                    }

                }
                else
                {
                    if (P.Longitude < MinLon)
                    {
                        MinLon = P.Longitude;
                        LeftPoints.Clear();
                        LeftPoints.Add(P);
                        //LeftPoint = P;
                    }
                    else if(P.Longitude == MinLon) LeftPoints.Add(P);
                    if (P.Longitude > MaxLon)
                    {
                        MaxLon = P.Longitude;
                        RightPoints.Clear();
                        RightPoints.Add(P);
                        //RightPoint = P;
                    }
                    else if (P.Longitude == MaxLon) RightPoints.Add(P);
                }
                if (P.Latitude < MinLat)
                {
                    MinLat = P.Latitude;
                    BottomPoints.Clear();
                    BottomPoints.Add(P);
                    //BottomPoint = P;
                }
                else if (P.Latitude == MinLat) BottomPoints.Add(P);
                if (P.Latitude > MaxLat)
                {
                    MaxLat = P.Latitude;
                    TopPoints.Clear();
                    TopPoints.Add(P);
                    //TopPoint = P;
                }
                else if (P.Latitude == MaxLat) TopPoints.Add(P);
            }
            if (BottomPoints.Count == 1) BottomPoint = BottomPoints[0];
            if (TopPoints.Count == 1) TopPoint = TopPoints[0];
            if (LeftPoints.Count == 1) LeftPoint = LeftPoints[0];
            if (RightPoints.Count == 1) RightPoint = RightPoints[0];
            if (BottomPoint == Zero)
            {
                foreach (MapCoordinate p in BottomPoints)
                {
                    if (p == LeftPoint || p == RightPoint) BottomPoints.Remove(p);
                    if (BottomPoints.Count == 1)
                    {
                        BottomPoint = BottomPoints[0];
                        break;
                    }
                }
                if (BottomPoint == Zero)
                {
                    double minLng=180;
                    foreach (MapCoordinate p in BottomPoints)
                    {
                        if(p.Longitude<minLng)
                        {
                            minLng = p.Longitude;
                            BottomPoint = p;
                        }
                    }
                }
            }
            if (TopPoint == Zero)
            {

                foreach (MapCoordinate p in TopPoints)
                {
                    if (p == LeftPoint || p == RightPoint) TopPoints.Remove(p);

                    if (TopPoints.Count == 1)
                    {
                        TopPoint = TopPoints[0];
                        break;
                    }
                }
                if (TopPoint == Zero)
                {
                    double maxLng = -180;
                    foreach (MapCoordinate p in TopPoints)
                    {
                        if (p.Longitude > maxLng)
                        {
                            maxLng = p.Longitude;
                            TopPoint = p;
                        }
                    }
                }
            }
            if (LeftPoint == Zero)
            {
                foreach (MapCoordinate p in LeftPoints)
                {
                    if (p == TopPoint || p == BottomPoint) LeftPoints.Remove(p);
                    if (LeftPoints.Count == 1)
                    {
                        LeftPoint = LeftPoints[0];
                        break;
                    }
                }
                if (LeftPoint == Zero) LeftPoint = LeftPoints[0];
                double totallat = 0;
                double lng = 0;
                foreach (MapCoordinate p in LeftPoints)
                {
                    lng = p.Longitude;
                    totallat += p.Latitude;
                }
                LeftPoint = new MapCoordinate( totallat / LeftPoints.Count, lng);
            }
            if (RightPoint == Zero)
            {
                foreach (MapCoordinate p in RightPoints)
                {
                    if (p == TopPoint || p == BottomPoint) RightPoints.Remove(p);
                    if (RightPoints.Count == 1)
                    {
                        RightPoint = RightPoints[0];
                        break;
                    }
                }
                if (RightPoint == Zero) RightPoint = RightPoints[0];
            }



            _centre = new MapCoordinate(_polygon.Centre.Y,_polygon.Centre.X);//find the average of the latitudes and longitudes to determine the centre
            if (Polygon.CrossesInternationalDateLine)
            {
                _boundingBox = new MapBoundingBox(new MapCoordinate(MaxLat, MaxLon), new MapCoordinate(MinLat, MinLon));//create a bounding box from the northeast point and the southwest point
            }
            else
            {
                _boundingBox = new MapBoundingBox(new MapCoordinate(MaxLat, MinLon), new MapCoordinate(MinLat, MaxLon));//create a bounding box from the northeast point and the southwest point
            }
            

            //calculate the distance between the corner points to determine the with and height of the polygon
            double TopToRight = Math.Sqrt(
               Math.Pow((TopPoint.Longitude - RightPoint.Longitude), 2) +
               Math.Pow((TopPoint.Latitude - RightPoint.Latitude), 2));
            if (TopToRight < 0) TopToRight *= -1;

            double TopToLeft = Math.Sqrt(
                Math.Pow((TopPoint.Longitude - LeftPoint.Longitude), 2) +
                Math.Pow(TopPoint.Latitude - LeftPoint.Latitude, 2));
            if (TopToLeft < 0) TopToLeft *= -1;

            double BottomToRight = Math.Sqrt(
                 Math.Pow((BottomPoint.Longitude - RightPoint.Longitude), 2) +
                 Math.Pow((BottomPoint.Latitude - RightPoint.Latitude), 2));
            if (BottomToRight < 0) BottomToRight *= -1;

            double BottomToLeft = Math.Sqrt(
               Math.Pow((BottomPoint.Longitude - LeftPoint.Longitude), 2) +
               Math.Pow(BottomPoint.Latitude - LeftPoint.Latitude, 2));
            if (BottomToLeft < 0) BottomToLeft *= -1;


            //use the largest sides of the polygon to determine the width and height
            if (TopToRight > BottomToLeft)
            {
                _width = TopToRight;
                _widthMeters = TopPoint.DistanceTo(RightPoint);
            }
            else
            {
                _width = BottomToLeft;
                _widthMeters = BottomPoint.DistanceTo(LeftPoint);
            }
            if (TopToLeft > BottomToRight)
            {
                _height = TopToLeft;
                _heightMeters = TopPoint.DistanceTo(RightPoint);
            }
            else
            {
                _height = BottomToRight;
                _heightMeters = BottomPoint.DistanceTo(RightPoint);
            }
            if (Polygon.CrossesInternationalDateLine && _width > 180) _width = 360 - _width;
             //Use a line down the middle of the polygon to calculate the bearing
             _bearing = CoordinateHelpers.GetAngle((BottomPoint.Latitude + LeftPoint.Latitude) / 2, (BottomPoint.Longitude + LeftPoint.Longitude) / 2, (TopPoint.Latitude + RightPoint.Latitude) / 2, (TopPoint.Longitude + RightPoint.Longitude) / 2);


            OnPropertyChanged("centre");
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
