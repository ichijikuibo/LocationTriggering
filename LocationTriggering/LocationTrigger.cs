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
        private MapCoordinate _centre;  //Calculated centre property that is assigned a value when _points is updated
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
            if(_boundingBox.containsPoint(point))
            {
                if (_polygon.PointInPolygon(point.Longitude, point.Latitude)) return true;
            }
            return false;
        }
        /// <summary>
        /// Returns the distance from the centre of this location to the specified point
        /// override if you want to chang this to ClosestDistanceTo
        /// </summary>
        /// <param name="point">Point to measure distance from</param>
        /// <returns>Distance in meters</returns>
        public virtual double DistanceTo(MapCoordinate point)
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
        public virtual double DistanceTo(LocationTrigger point)
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
        /// Uses a formula that may not cover the entire location at close distances, override to change this to Polygon.BearingRangeFrom or MapBoundingBox.BearingRangeFrom if required
        /// </summary>
        /// <param name="point">The point to calculate the bearings from</param>
        /// <returns>A bearing range containg the minimum and maximum bearings</returns>
        public virtual BearingRange BearingRangeFrom(MapCoordinate point)
        {
            double centreBearing = point.BearingTo(Centre);
            double guideDistance = BoundingBox.width;
            if (BoundingBox.height > guideDistance) guideDistance = BoundingBox.height;
            double targetBearing1 = CoordinateHelpers.NormaliseBearing(centreBearing + 90);
            double targetBearing2 = CoordinateHelpers.NormaliseBearing(centreBearing - 90);
            MapCoordinate Point1 = ClosestPointTo(new MapCoordinate(CoordinateHelpers.DestinationPointFromBearingAndDistance(Centre.ToPointD(),guideDistance, targetBearing1)));//{54.9964314712174, -7.32574279029166}
            MapCoordinate Point2 = ClosestPointTo(new MapCoordinate(CoordinateHelpers.DestinationPointFromBearingAndDistance(Centre.ToPointD(), guideDistance, targetBearing2)));//{54.9953895258887, -7.32721765393637}
            double start = point.BearingTo(Point2);
            double end = point.BearingTo(Point1);
            BearingRange BoundingBoxRange = BoundingBox.BearingRangeFrom(point);
            BearingRange BR = new BearingRange(start, end);
            if(BR.Range> BoundingBoxRange.Range) BR = new BearingRange(end, start);
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
        /// Calculate the centre point, and bounding box when the points list is updated
        /// </summary>
        private void CalculateProperties()
        {
            //recreate the polygon class for determining if a point if within the location
            RecreatePolygon();
            if (_points.Count < 3) return; //At least 3 points are required to calcualte the properties
            //Varibles to help calcualte the properties 
            double MinLon = double.MaxValue, MaxLon = -double.MaxValue, MinLat = double.MaxValue, MaxLat = -double.MaxValue;
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
                        }

                    }
                    else
                    {
                        if (P.Longitude > MaxLon)
                        {
                            MaxLon = P.Longitude;
                        }
                    }

                }
                else
                {
                    if (P.Longitude < MinLon)
                    {
                        MinLon = P.Longitude;
                    }
                    if (P.Longitude > MaxLon)
                    {
                        MaxLon = P.Longitude;
                    }
                }
                if (P.Latitude < MinLat)
                {
                    MinLat = P.Latitude;
                }
                if (P.Latitude > MaxLat)
                {
                    MaxLat = P.Latitude;
                }
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


            OnPropertyChanged("centre");
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
