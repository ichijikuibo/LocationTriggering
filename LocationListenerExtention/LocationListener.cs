using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            if (_lastPosition!=null && e.Position.Latitude == _lastPosition.Latitude && e.Position.Longitude == _lastPosition.Longitude) return;
            _lastPosition = e.Position;
            //Get the current GPS Location           
            

            //because the list may be bound to a ui component they need to be updated on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                //Process the new position
                baseListener.Update(e.Position);

            });
        }

    }
    public static class LocationListenerExtention
    {
        private static TimeSpan pollInterval;
       // private static double gpsDistance;
        private static Position _lastGPSPosition;
        private static double _accuracyRequired = 100;
        private static double _desiredAccuracy = 0;
        private static int inaccuratetLocations = 0;
        private static Position MostAccurate;


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
                await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(interval, 0);
                baselistener.ChangeGpsDistance(distanceMetres);
                //gpsDistance = distanceMetres;
                _lastGPSPosition = null;
                pollInterval = interval;
                Update(baselistener, await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync());
            }
        }
        /// <summary>
        /// Get the last GPS position recieved
        /// </summary>
        /// <returns></returns>
        public static Position getLastPosition<T>(this LocationListener<T> baselistener) where T : LocationTrigger
        {
            return _lastGPSPosition;
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
        public static void Update<T>(this LocationListener<T> baselistener, Position position) where T : LocationTrigger
        {

            bool UpdateLocation = false;
            if (_lastGPSPosition!=null && _lastGPSPosition.Accuracy<position.Accuracy&&position.Accuracy > _accuracyRequired) return;
            if (_lastGPSPosition == null || position.Accuracy <= _desiredAccuracy|| _lastGPSPosition.Accuracy < position.Accuracy)
            {
                UpdateLocation = true;
                _lastGPSPosition = new Position(position);
            }
            else
            {
                inaccuratetLocations++;
                if (MostAccurate == null || position.Accuracy <= MostAccurate.Accuracy) MostAccurate = new Position(position);
                if (inaccuratetLocations >= 5)
                {
                    _lastGPSPosition = MostAccurate;
                    UpdateLocation = true;
                }
            }


            if (UpdateLocation)
            {

                MapCoordinate newPosition = _lastGPSPosition.ToMapCoordinate(); 
                MainThread.BeginInvokeOnMainThread(() =>
                {
                        //Process the new position
                        baselistener.Update(newPosition);


                });
                MostAccurate = null;
                inaccuratetLocations = 0;
            }
        }
        public static async void Update<T>(this LocationListener<T> baselistener) where T : LocationTrigger
        {
            Update(baselistener, await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync());
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
            //gpsDistance = distanceMetres;
        }

        /// <summary>
        /// Change the desired accuracy of the gps listener(If it get nothing but inacurate results it will use the most accurate for every 10 polls)
        /// </summary>
        /// <param name="accuracy">The accuracy level in metres</param>
        public static void changeRequiredAccuracy<T>(this LocationListener<T> baselistener, double accuracy) where T : LocationTrigger
        {
            _accuracyRequired = accuracy;
        }
        public static void changeDesiredAccuracy<T>(this LocationListener<T> baselistener, double accuracy) where T : LocationTrigger
        {
            _desiredAccuracy = accuracy;
        }

    }
}
