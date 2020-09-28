﻿using LocationTriggering.Utilities;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LocationTriggering
{
    public class LocationListener
    {
        /// <summary>
        /// Event sent when ever there is a change to the CurrentLocationTriggers
        /// </summary>
        public event LocationTriggerEventHandler LocationsChanged;
        public event LocationEnteredEventHandler LocationEntered;
        public event LocationExitedEventHandler LocationExited;
        public event PositionUpdatedEventHandler PositionUpdated;

        private LocationTriggerCollection _LocationTriggers;
        private LocationTriggerCollection _currentLocationTriggers;
        private LocationTriggerCollection _closestLocations;
        private bool _listening;
        private Position _lastPosition;
        private IReadOnlyList<LocationTrigger> _lastlocationTriggers;
        private TimeSpan _gpsInterval;
        private double _gpsDistance;
        private int _numberOfClosestLocations = 10;

        /// <summary>
        /// List of the location triggers to be checked against the gps location
        /// </summary>
        public LocationTriggerCollection LocationTriggers { get => _LocationTriggers; }
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
        public LocationTriggerCollection CurrentLocationTriggers { get => _currentLocationTriggers; }
        /// <summary>
        /// The number of location to store in ClosestLocations at once. 0 to disable -1 for all locations default:10
        /// </summary>
        public int NumberOfClosestLocations { get => _numberOfClosestLocations; set => _numberOfClosestLocations = value; }
        /// <summary>
        /// A list of the closest locations to the last checked GPS point. Default 10, number change changed with NumberOfClosestLocations
        /// </summary>
        public LocationTriggerCollection ClosestLocations { get => _closestLocations; }

        /// <summary>
        ///  Default constructor
        /// </summary>       
        public LocationListener()
        {
            _LocationTriggers = new LocationTriggerCollection();
            _closestLocations = new LocationTriggerCollection();
            _currentLocationTriggers = new LocationTriggerCollection();
            _listening = false;
        }
        /// <summary>
        /// Starts the listener and sets it to check for GPS the specified interval and fire events if it detects a change greater then the specified distance
        /// </summary>
        /// <param name="interval">A System.TimeSpan that determines how ofter to check for changes</param>
        /// <param name="distanceMetres">The minimum distance between that needs to be moved for the location to be updated</param>
        public async void StartListening(TimeSpan interval,double distanceMetres)
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
            //Process the new position
            ProcessLocation(newPosition);
            //send a PositionUpdated event
            PositionUpdated?.Invoke(this, new PositionUpdatedEventArgs() { GPSPosition = newPosition, TimeTriggered = System.DateTime.Now });

            _lastPosition = newPosition;//Store the current position
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
        public async void  ChangeGpsPollInterval(TimeSpan interval)
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
        //private async void _GPSTimer_Elapsed(Object stateInfo)
        ////private async void _GPSTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{

        //    double distance = 0;
        //    //Get the current GPS Location            
        //    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(async () =>
        //    {
        //        Location newPosition = await Geolocation.GetLocationAsync();

        //        //if position is the same as last position end the method
        //        if (_lastPosition != null)
        //        {
        //            if (newPosition.Latitude == _lastPosition.Latitude || newPosition.Longitude == _lastPosition.Longitude)
        //            {
        //                return;
        //            }
        //            //Check distance between current location and previous location
        //            distance = newPosition.CalculateDistance(_lastPosition, DistanceUnits.Kilometers) * 1000;
        //            //If distance is too close to previous end the method
        //            if (distance < _gpsDistance)
        //            {
        //                return;
        //            }
        //        }
        //        //Process the new position
        //        ProcessLocation(newPosition);
        //        //send a PositionUpdated event
        //        PositionUpdated?.Invoke(this, new PositionUpdatedEventArgs() { Position = newPosition, TimeTriggered = System.DateTime.Now });

        //        _lastPosition = newPosition;//Store the current position
        //    });
        //}
        /// <summary>
        /// Checks the for triggers at the specifed Location and invokes events 
        /// </summary>
        /// <param name="gpsLocation">The gps location to be checked</param>
        public virtual void ProcessLocation(Position gpsLocation)
        { 
            //Get the locations at the current point
            IReadOnlyList<LocationTrigger> locationsAtPoint = _LocationTriggers.LocationsAtPoint(gpsLocation);
            //Lists to store the changes
            List<LocationTrigger> locationsEntered = new List<LocationTrigger>();
            List<LocationTrigger> locationsExited = new List<LocationTrigger>();
            //check for locations at the current point that weren't in the previous update and add to a List
            if (locationsAtPoint.Count > 0)
            {
                foreach (LocationTrigger LT in locationsAtPoint)
                {
                    if (!CurrentLocationTriggers.Contains(LT))
                        locationsEntered.Add(LT);
                }
            }
            //check for locations that were in the previous updated but not in the current update and add to a list
            foreach (LocationTrigger LT in CurrentLocationTriggers)
            {
                if (!locationsAtPoint.Contains(LT)) 
                    locationsExited.Add(LT);
            }
            //The CurrentLocationTriggers List has been changed send a LocationsChanged change event
            if (locationsEntered.Count > 0 || locationsExited.Count > 0)
            {
                LocationTriggeredEventArgs e = new LocationTriggeredEventArgs();
                e.CurrentLocations = locationsAtPoint;
                e.LocationsEntered = locationsEntered.AsReadOnly();
                e.LocationsExited = locationsExited.AsReadOnly();
                e.TimeTriggered = System.DateTime.Now;
                e.GPSPosition = gpsLocation;
                _lastlocationTriggers = locationsAtPoint;
                LocationsChanged?.Invoke(this, e);

                //Send a LocationEntered entered event for each of the newly entered locations
                foreach (LocationTrigger enteredLocation in locationsEntered)
                {
                    if(!_currentLocationTriggers.Contains(enteredLocation))_currentLocationTriggers.Add(enteredLocation);
                    LocationEntered?.Invoke(this, new LocationUpdatedEventArgs() { TimeTriggered = System.DateTime.Now, GPSPosition = gpsLocation, Location = enteredLocation });
                }
                //Send a LocationExited entered event for each of the newly exited locations
                foreach (LocationTrigger exitedLocation in locationsExited)
                {
                    if (_currentLocationTriggers.Contains(exitedLocation)) _currentLocationTriggers.Add(exitedLocation);
                    LocationExited?.Invoke(this, new LocationUpdatedEventArgs() { TimeTriggered = System.DateTime.Now, GPSPosition = gpsLocation, Location = exitedLocation });
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
            foreach (LocationTrigger LT in _closestLocations)
            {
                if (!ClosestLocations.Contains(LT)) _closestLocations.Remove(LT);
            }
            foreach(LocationTrigger LT in ClosestLocations)
            {
                if (!_closestLocations.Contains(LT)) _closestLocations.Add(LT);
            }
            _closestLocations.Sort(delegate (LocationTrigger lt1, LocationTrigger lt2) { return lt1.LastDistance.CompareTo(lt1.LastDistance); });
        }
    }
}