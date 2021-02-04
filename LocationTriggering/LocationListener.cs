using LocationTriggering.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocationTriggering
{
    /// <summary>
    /// A Class for listening to gps triggers and sending events whenever a new loccation is reached or exited
    /// </summary>
    /// <typeparam name="T">A type derived from a LocationTrigger</typeparam>
    public class LocationListener<T> where T : LocationTrigger
    {
        /// <summary>
        /// Event sent when ever there is a change to the CurrentLocationTriggers
        /// </summary>
        public event LocationTriggerEventHandler<T> LocationsChanged;
        public event LocationTriggerEventHandler<T> LocationsInDirectionChanged;
        public event LocationTriggerEventHandler<T> ClosestLocationChanged;
        public event LocationEnteredEventHandler<T> LocationEntered;
        public event LocationExitedEventHandler<T> LocationExited;
        public event PositionUpdatedEventHandler PositionUpdated;
        //public event ExceptionEventHandler ExceptionThrown;
        private LocationTriggerCollection<T> _LocationTriggers;
        private LocationTriggerCollection<T> _locationTriggersInDirection;
        private LocationTriggerCollection<T> _currentLocationTriggers;
        private LocationTriggerCollection<T> _closestLocations;
        private MapCoordinate _lastPosition;
        private IReadOnlyList<T> _lastlocationTriggers;
        //private TimeSpan _gpsInterval;
        private double _gpsDistance;
        private double _locationsInDirectionDistance=0;
        private int _numberOfClosestLocations = 10;
        private bool processingLocationUpdate = false;
        private bool processingClosestUpdate = false;
        private bool processingBearingUpdate = false;
        private bool firstUpdate = true;

        /// <summary>
        /// List of the location triggers to be checked against the gps location
        /// </summary>
        public LocationTriggerCollection<T> LocationTriggers { get => _LocationTriggers; }
        /// <summary>
        /// The last GPS position received from the GPS 
        /// </summary>
        public MapCoordinate LastPosition { get => _lastPosition; }
        /// <summary>
        /// The triggers that were returned in the last gps update
        /// </summary>
        public LocationTriggerCollection<T> CurrentLocationTriggers { get => _currentLocationTriggers; }
        /// <summary>
        /// The triggers that were returned in the last bearing update
        /// </summary>
        public LocationTriggerCollection<T> LocationTriggersInDirection { get => _locationTriggersInDirection; }
        /// <summary>
        /// The number of location to store in ClosestLocations at once. 0 to disable -1 for all locations default:10
        /// </summary>
        public int NumberOfClosestLocations { get => _numberOfClosestLocations; set {
                _numberOfClosestLocations = value;
                     _lastPosition = null;
            } }
        /// <summary>
        /// The maximum ddistance for checking fo locations in a direction default unlimited
        /// </summary>
        public double LocationsInDirectionDistance
        {
            get => _locationsInDirectionDistance; set
            {
                _locationsInDirectionDistance = value;
            }
        }
        /// <summary>
        /// A list of the closest locations to the last checked GPS point. Default 10, number change changed with NumberOfClosestLocations
        /// </summary>
        public LocationTriggerCollection<T> ClosestLocations { get => _closestLocations; }
        public double GpsDistance { get => _gpsDistance; }

        /// <summary>
        ///  Default constructor
        /// </summary>       
        public LocationListener()
        {
            _LocationTriggers = new LocationTriggerCollection<T>();
            _closestLocations = new LocationTriggerCollection<T>();
            _currentLocationTriggers = new LocationTriggerCollection<T>();
            _locationTriggersInDirection = new LocationTriggerCollection<T>();

        }
        /// <summary>
        /// Changes the distance change required to process changes
        /// </summary>
        /// <param name="distanceMetres">The minimum distance between that needs to be moved for the location to be updated</param>
        public virtual void ChangeGpsDistance(double distanceMetres)
        {
            _gpsDistance = distanceMetres;
            _lastPosition = null;
        }

        /// <summary>
        /// Checks the for triggers at the specifed Location and invokes events 
        /// </summary>
        /// <param name="gpsLocation">The gps location to be checked</param>
        public virtual async void ProcessLocation(MapCoordinate gpsLocation)
        {
            if (processingLocationUpdate) return;
            processingLocationUpdate = true;

            //Lists to store the changes
            List<T> locationsEntered = new List<T>();
            List<T> locationsExited = new List<T>();
            IReadOnlyList<T> locationsAtPoint = new List<T>();
            //If there are alot of locations added this code could be slow so run on a background thread
            await Task.Run(() =>
            {

                locationsAtPoint = _LocationTriggers.LocationsAtPoint(gpsLocation);
                //check for locations that were in the previous updated but not in the current update and add to a list
                foreach (T LT in CurrentLocationTriggers)
                {
                    if (!locationsAtPoint.Contains(LT))
                        locationsExited.Add(LT);
                }
                //check for locations at the current point that weren't in the previous update and add to a List
                if (locationsAtPoint.Count > 0)
                {
                    foreach (T LT in locationsAtPoint)
                    {
                        if (!CurrentLocationTriggers.Contains(LT))
                            locationsEntered.Add(LT);
                    }
                }

            });

            //The CurrentLocationTriggers List has been changed send a LocationsChanged change event

            if (locationsEntered.Count > 0 || locationsExited.Count > 0 || firstUpdate)
            {


                //disable sorting till items are updated
                Comparison<T> sortComparison = null;
                if (_currentLocationTriggers.SortOnChangeComparison != null)
                {
                    sortComparison = _currentLocationTriggers.SortOnChangeComparison;
                    _currentLocationTriggers.StopSortingOnChange();
                }
                //Send a LocationExited entered event for each of the newly exited locations
                foreach (T exitedLocation in locationsExited)
                {
                    if (_currentLocationTriggers.Contains(exitedLocation)) _currentLocationTriggers.Remove(exitedLocation);
                    LocationExited?.Invoke(this, new LocationUpdatedEventArgs<T>() { TimeTriggered = System.DateTime.Now, GPSPosition = gpsLocation, Location = exitedLocation });
                }

                //Send a LocationEntered entered event for each of the newly entered locations
                foreach (T enteredLocation in locationsEntered)
                {
                    if (!_currentLocationTriggers.Contains(enteredLocation))
                    {
                        _currentLocationTriggers.Add(enteredLocation);
                    }
                    LocationEntered?.Invoke(this, new LocationUpdatedEventArgs<T>() { TimeTriggered = System.DateTime.Now, GPSPosition = gpsLocation, Location = enteredLocation });
                }

                if (sortComparison != null)
                {
                    _currentLocationTriggers.SortOnChange(sortComparison);
                }
                LocationTriggeredEventArgs<T> e = new LocationTriggeredEventArgs<T>();
                e.CurrentLocations = new List<T>(_currentLocationTriggers);
                e.LocationsAdded = locationsEntered.AsReadOnly();
                e.LocationsRemoved = locationsExited.AsReadOnly();
                e.TimeTriggered = System.DateTime.Now;
                e.GPSPosition = gpsLocation;
                _lastlocationTriggers = locationsAtPoint;
                LocationsChanged?.Invoke(this, e);
            }
            processingLocationUpdate = false;
        }
        /// <summary>
        /// Checks the for triggers in the specified direction from the specifed Location and invokes events 
        /// </summary>
        /// <param name="bearing">The direction 0-360 to check for locations</param>
        /// <param name="gpsLocation">The gps location to be checked. null= LastPosition</param>
        public virtual async void UpdateBearing(double bearing, MapCoordinate gpsLocation)
        {
            if (processingBearingUpdate) return;
            processingBearingUpdate = true;
            if (gpsLocation == null)
                gpsLocation = _lastPosition;

            //Lists to store the changes
            List<T> locationsAdded = new List<T>();
            List<T> locationsRemoved = new List<T>();
            IReadOnlyList<T> locationsAtDirection = new List<T>();
            //If there are alot of locations added this code could be slow so run on a background thread
            await Task.Run(() =>
            {

                locationsAtDirection = _LocationTriggers.LocationsInDirection(gpsLocation, bearing, LocationsInDirectionDistance);
                //check for locations that were in the previous updated but not in the current update and add to a list
                foreach (T LT in _locationTriggersInDirection)
                {
                    if (!locationsAtDirection.Contains(LT))
                        locationsRemoved.Add(LT);
                }
                //check for locations at the current point that weren't in the previous update and add to a List
                if (locationsAtDirection.Count > 0)
                {
                    foreach (T LT in locationsAtDirection)
                    {
                        if (!_locationTriggersInDirection.Contains(LT))
                            locationsAdded.Add(LT);
                    }
                }

            });

            //The CurrentLocationTriggers List has been changed send a LocationsChanged change event
            if (locationsAdded.Count > 0 || locationsRemoved.Count > 0|| firstUpdate)
            {
                foreach (T exitedLocation in locationsRemoved)
                {
                    if (_locationTriggersInDirection.Contains(exitedLocation)) _locationTriggersInDirection.Remove(exitedLocation);
                }

                foreach (T enteredLocation in locationsAdded)
                {
                    if (!_locationTriggersInDirection.Contains(enteredLocation))
                    {
                        _locationTriggersInDirection.Add(enteredLocation);


                    }
                }

                LocationTriggeredEventArgs<T> e = new LocationTriggeredEventArgs<T>();
                e.CurrentLocations = locationsAtDirection;
                e.LocationsAdded = locationsAdded.AsReadOnly();
                e.LocationsRemoved = locationsRemoved.AsReadOnly();
                e.TimeTriggered = System.DateTime.Now;
                e.GPSPosition = gpsLocation;
                _lastlocationTriggers = locationsAtDirection;
                LocationsInDirectionChanged?.Invoke(this, e);
            }

            processingBearingUpdate = false;
        }

        /// <summary>
        /// Update the list of closest locations
        /// </summary>
        /// <param name="gpsLocation">The position to calculate distances from</param>
        public virtual async void ProcessClosestLocations(MapCoordinate gpsLocation)
        {
            if (processingClosestUpdate) return;
            processingClosestUpdate = true;
            if (_numberOfClosestLocations == 0) return;
            List<T> locationsAdded = new List<T>();
            List<T> locationsRemoved = new List<T>();
            //  IReadOnlyList<T> ClosestLocations = LocationTriggers.ClosestLocations(gpsLocation, _numberOfClosestLocations);


            //If there are alot of locations added this code could be slow so run on a background thread
            await Task.Run(() =>
            {

                IReadOnlyList<T> ClosestLocations = LocationTriggers.ClosestLocations(gpsLocation, _numberOfClosestLocations);
                LocationTriggers.UpdateBearings(gpsLocation, false);
                //check for locations that were in the previous updated but not in the current update and add to a list
                foreach (T LT in _closestLocations)
                {
                    if (!ClosestLocations.Contains(LT))
                        locationsRemoved.Add(LT);
                }
                //check for locations at the current point that weren't in the previous update and add to a List
                if (ClosestLocations.Count > 0)
                {
                    foreach (T LT in ClosestLocations)
                    {
                        if (!_closestLocations.Contains(LT))
                            locationsAdded.Add(LT);
                    }
                }


            });

            if (locationsAdded.Count > 0 || locationsRemoved.Count > 0|| firstUpdate)
            {
                foreach (T exitedLocation in locationsRemoved)
                {
                    if (_closestLocations.Contains(exitedLocation)) _closestLocations.Remove(exitedLocation);
                }
                foreach (T enteredLocation in locationsAdded)
                {
                    if (!_closestLocations.Contains(enteredLocation))
                    {
                        _closestLocations.Add(enteredLocation);
                    }
                }

                LocationTriggeredEventArgs<T> e = new LocationTriggeredEventArgs<T>();
                e.CurrentLocations = ClosestLocations;
                e.LocationsAdded = locationsAdded.AsReadOnly();
                e.LocationsRemoved = locationsRemoved.AsReadOnly();
                e.TimeTriggered = System.DateTime.Now;
                e.GPSPosition = gpsLocation;
                ClosestLocationChanged?.Invoke(this, e);
            }
            _closestLocations.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); });
            processingClosestUpdate = false;

        }
        bool updating = false;
        public virtual async void Update(MapCoordinate gpsLocation)
        {
            if (updating)
                return;
            updating = true;
            if (_lastPosition != null)
            {
                if (gpsLocation.Equals(_lastPosition))
                {
                    updating = false;
                    return;
                }
                double distance = gpsLocation.DistanceTo(_lastPosition) * 1000;
                //If distance is too close to previous end the method
                if (distance <= _gpsDistance)
                {
                    updating = false;
                    return;
                }
            }
            //Process the new position
            ProcessLocation(gpsLocation);

            ProcessClosestLocations(gpsLocation);
            await Task.Run(() =>
            {
                while (processingBearingUpdate || processingClosestUpdate || processingLocationUpdate)
                    Thread.Sleep(10);
            }
            );
            //send a PositionUpdated event
            PositionUpdated?.Invoke(this, new PositionUpdatedEventArgs() { GPSPosition = gpsLocation, TimeTriggered = System.DateTime.Now });

            _lastPosition = gpsLocation;//Store the current position
            firstUpdate = false;
            updating = false;
        }
    }

}
