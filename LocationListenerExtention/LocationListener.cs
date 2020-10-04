using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using LocationTriggering;
using LocationTriggering.Utilities;
using Plugin.Geolocator.Abstractions;
using Xamarin.Essentials;

namespace LocationTriggering.Extentions
{
    public class ListenerClass<T> where T:LocationTrigger
    {
        LocationListener<T> baseListener;
        double _gpsDistance;
        TimeSpan _gpstime;
        Position _lastPosition;
        public ListenerClass(LocationListener<T> newbase, double distance,TimeSpan time)
        {
            _gpsDistance = distance;
            _gpstime = time;
            baseListener = newbase;
        }
        public void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            //Geolocator often calls the event 2 times with the same coordinate so check if the current is the same as the last
            if (e.Position.Latitude == _lastPosition.Latitude && e.Position.Longitude == _lastPosition.Longitude) return;
            _lastPosition = e.Position;
            //Get the current GPS Location           
            MapCoordinate newPosition = new MapCoordinate(e.Position.Latitude, e.Position.Longitude);

            //because the list may be bound to a ui component they need to be updated on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                //Process the new position
                baseListener.Update(newPosition);

            });
        }

    }
    public static class LocationListenerExtention
    {
        private static TimeSpan pollInterval;
        private static double gpsDistance;


        /// <summary>
        /// Starts the listener and sets it to check for GPS the specified interval and fire events if it detects a change greater then the specified distance
        /// Stop llistening needs to be called when its no longer required or leaving the page
        /// </summary>
        /// <param name="interval">A System.TimeSpan that determines how ofter to check for changes</param>
        /// <param name="distanceMetres">The minimum distance between that needs to be moved for the location to be updated</param>
        public static async void StartListening<T>(this LocationListener<T> baselistener, TimeSpan interval, double distanceMetres) where T:LocationTrigger
        {
            //check if GPS Location permission has been granted
            //if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted) throw new PermissionException("Location permission not granted, Location permission is required for this feature");
            if (Plugin.Geolocator.CrossGeolocator.IsSupported && Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationAvailable&& !Plugin.Geolocator.CrossGeolocator.Current.IsListening)
            {
                //it is not posible to bind an event in this static generic extention class so an object of another class is created to handle the event
                ListenerClass<T> baseListener = new ListenerClass<T>(baselistener, distanceMetres,interval);
                Plugin.Geolocator.CrossGeolocator.Current.PositionChanged += baseListener.Current_PositionChanged;
                await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(interval, distanceMetres);
                baselistener.ChangeGpsDistance(distanceMetres);
                gpsDistance = distanceMetres;
                pollInterval = interval;
            }
        }

        /// <summary>
        /// Stops the GPS Listener
        /// </summary>
        public static async void StopListening<T>(this LocationListener<T> baselistener) where T : LocationTrigger
        {
            if (Plugin.Geolocator.CrossGeolocator.Current.IsListening)
            {
                await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
                //Plugin.Geolocator.CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;
            }
        }
        /// <summary>
        /// Changes the interval between GPS updates
        /// </summary>
        /// <param name="interval">A System.TimeSpan that determines how ofter to check for changes</param>
        public static async void ChangeGpsPollInterval<T>(this LocationListener<T> baselistener,TimeSpan interval) where T : LocationTrigger
        {
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(interval, baselistener.GpsDistance);
            pollInterval = interval;
        }
        /// <summary>
        /// Changes the distance change required to process changes
        /// </summary>
        /// <param name="distanceMetres">The minimum distance between that needs to be moved for the location to be updated</param>
        public static async void ChangeGpsDistance<T>(this LocationListener<T> baselistener, double distanceMetres) where T : LocationTrigger
        {
            baselistener.ChangeGpsDistance(distanceMetres);
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(pollInterval, distanceMetres);
            gpsDistance = distanceMetres;
        }

    }
}
