using LocationTriggering.Utilities;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

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
        public event LocationEnteredEventHandler<T> LocationEntered;
        public event LocationExitedEventHandler<T> LocationExited;
        public event PositionUpdatedEventHandler PositionUpdated;
        private LocationTriggerCollection<T> _LocationTriggers;
        private LocationTriggerCollection<T> _currentLocationTriggers;
        private LocationTriggerCollection<T> _closestLocations;
        private bool _listening;
        private Position _lastPosition;
        private IReadOnlyList<T> _lastlocationTriggers;
        private TimeSpan _gpsInterval;
        private double _gpsDistance;
        private int _numberOfClosestLocations = 10;

        /// <summary>
        /// List of the location triggers to be checked against the gps location
        /// </summary>
        public LocationTriggerCollection<T> LocationTriggers { get => _LocationTriggers; }
        /// <summary>
        /// True if the LocationListener is active
        /// </summary>
        public bool IsListening { get => _listening; }
        /// <summary>
        /// The last GPS position received from the GPS 
        /// </summary>
        public Position LastPosition { get => _lastPosition; }
        /// <summary>
        /// The triggers that were returned in the last gps update
        /// </summary>
        public LocationTriggerCollection<T> CurrentLocationTriggers { get => _currentLocationTriggers; }
        /// <summary>
        /// The number of location to store in ClosestLocations at once. 0 to disable -1 for all locations default:10
        /// </summary>
        public int NumberOfClosestLocations { get => _numberOfClosestLocations; set => _numberOfClosestLocations = value; }
        /// <summary>
        /// A list of the closest locations to the last checked GPS point. Default 10, number change changed with NumberOfClosestLocations
        /// </summary>
        public LocationTriggerCollection<T> ClosestLocations { get => _closestLocations; }

        /// <summary>
        ///  Default constructor
        /// </summary>       
        public LocationListener()
        {
            _LocationTriggers = new LocationTriggerCollection<T>();
            _closestLocations = new LocationTriggerCollection<T>();
            _currentLocationTriggers = new LocationTriggerCollection<T>();
            _listening = false;
        }
        /// <summary>
        /// Starts the listener and sets it to check for GPS the specified interval and fire events if it detects a change greater then the specified distance
        /// Stop llistening needs to be called when its no longer required or leaving the page
        /// </summary>
        /// <param name="interval">A System.TimeSpan that determines how ofter to check for changes</param>
        /// <param name="distanceMetres">The minimum distance between that needs to be moved for the location to be updated</param>
        public async void StartListening(TimeSpan interval, double distanceMetres)
        {
            //check if GPS Location permission has been granted
            //if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted) throw new PermissionException("Location permission not granted, Location permission is required for this feature");
            if (Plugin.Geolocator.CrossGeolocator.IsSupported && Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationAvailable)
            {
                Plugin.Geolocator.CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
                await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(interval, distanceMetres);
                _listening = true;
                _gpsDistance = distanceMetres;
                _gpsInterval = interval;
                //Create a timer at the specified interval to check for updates
                //_GPSTimer = new Timer(_GPSTimer_Elapsed, null, new TimeSpan(0, 0, 1), interval);
            }
        }

        private void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            double distance = 0;
            //Get the current GPS Location            

            Position newPosition = e.Position;

            //if position is the same as last position end the method
            if (_lastPosition != null)
            {
                if (newPosition.Latitude == _lastPosition.Latitude || newPosition.Longitude == _lastPosition.Longitude)
                {
                    return;
                }
                //Check distance between current location and previous location
                distance = newPosition.CalculateDistance(_lastPosition, GeolocatorUtils.DistanceUnits.Kilometers) * 1000;
                //If distance is too close to previous end the method

            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                //Process the new position
                ProcessLocation(newPosition);

                ProcessClosestLocations(newPosition);
                //send a PositionUpdated event
                PositionUpdated?.Invoke(this, new PositionUpdatedEventArgs() { GPSPosition = newPosition, TimeTriggered = System.DateTime.Now });

                _lastPosition = newPosition;//Store the current position
            });
        }
        /// <summary>
        /// Stops the GPS Listener
        /// </summary>
        public async void StopListening()
        {
            if (_listening)
            {
                await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
                Plugin.Geolocator.CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;
                _listening = false;
            }
        }
        /// <summary>
        /// Changes the interval between GPS updates
        /// </summary>
        /// <param name="interval">A System.TimeSpan that determines how ofter to check for changes</param>
        public async void ChangeGpsPollInterval(TimeSpan interval)
        {
            _gpsInterval = interval;
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(interval, _gpsDistance);
        }
        /// <summary>
        /// Changes the distance change required to process changes
        /// </summary>
        /// <param name="distanceMetres">The minimum distance between that needs to be moved for the location to be updated</param>
        public async void ChangeGpsDistance(double distanceMetres)
        {
            _gpsDistance = distanceMetres;
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(_gpsInterval, _gpsDistance);
        }
        /// <summary>
        /// Checks the for triggers at the specifed Location and invokes events 
        /// </summary>
        /// <param name="gpsLocation">The gps location to be checked</param>
        public virtual void ProcessLocation(Position gpsLocation)
        {
            //Get the locations at the current point
            IReadOnlyList<T> locationsAtPoint = _LocationTriggers.LocationsAtPoint(gpsLocation);
            //Lists to store the changes
            List<T> locationsEntered = new List<T>();
            List<T> locationsExited = new List<T>();
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

            //The CurrentLocationTriggers List has been changed send a LocationsChanged change event
            if (locationsEntered.Count > 0 || locationsExited.Count > 0)
            {
                LocationTriggeredEventArgs<T> e = new LocationTriggeredEventArgs<T>();
                e.CurrentLocations = locationsAtPoint;
                e.LocationsEntered = locationsEntered.AsReadOnly();
                e.LocationsExited = locationsExited.AsReadOnly();
                e.TimeTriggered = System.DateTime.Now;
                e.GPSPosition = gpsLocation;
                _lastlocationTriggers = locationsAtPoint;
                LocationsChanged?.Invoke(this, e);

                //Send a LocationEntered entered event for each of the newly entered locations
                foreach (T enteredLocation in locationsEntered)
                {
                    if (!_currentLocationTriggers.Contains(enteredLocation))
                    {
                        _currentLocationTriggers.Add(enteredLocation);


                    }
                    LocationEntered?.Invoke(this, new LocationUpdatedEventArgs<T>() { TimeTriggered = System.DateTime.Now, GPSPosition = gpsLocation, Location = enteredLocation });
                }
                //Send a LocationExited entered event for each of the newly exited locations
                foreach (T exitedLocation in locationsExited)
                {
                    if (_currentLocationTriggers.Contains(exitedLocation)) _currentLocationTriggers.Remove(exitedLocation);
                    LocationExited?.Invoke(this, new LocationUpdatedEventArgs<T>() { TimeTriggered = System.DateTime.Now, GPSPosition = gpsLocation, Location = exitedLocation });
                }

            }

        }
        /// <summary>
        /// Update the list of closest locations
        /// </summary>
        /// <param name="gpsLocation">The position to calculate distances from</param>
        public virtual void ProcessClosestLocations(Position gpsLocation)
        {
            if (_numberOfClosestLocations == 0) return;
            var ClosestLocations = LocationTriggers.ClosestLocations(gpsLocation, _numberOfClosestLocations);

            List<T> locationsAdded = new List<T>();
            List<T> locationsRemoved = new List<T>();
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


            if (locationsAdded.Count > 0 || locationsRemoved.Count > 0)
            {

                //Send a LocationEntered entered event for each of the newly entered locations
                foreach (T enteredLocation in locationsAdded)
                {
                    if (!_closestLocations.Contains(enteredLocation))
                    {
                        _closestLocations.Add(enteredLocation);
                    }
                }
                //Send a LocationExited entered event for each of the newly exited locations
                foreach (T exitedLocation in locationsRemoved)
                {
                    if (_closestLocations.Contains(exitedLocation)) _closestLocations.Remove(exitedLocation);
                }

            }
            _closestLocations.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); });
        }
        public virtual void ManualUpdate(Position gpsLocation)
        {
            ProcessLocation(gpsLocation);
            ProcessClosestLocations(gpsLocation);
        }
    }

}
