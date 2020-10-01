using LocationTriggering.Utilities;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocationTriggering
{
    /// <summary>
    /// A class for storing a list of location triggers
    /// </summary>
    /// <typeparam name="T">A type derived from LocationTrigger</typeparam>
    public class LocationTriggerCollection<T> : ObservableCollection<T>  where T : LocationTrigger 
    {
        private bool _sortOnChange = false;
        private Comparison<T> _sortOnChangeComparison;
        public LocationTriggerCollection()
        {
            //CollectionChanged += LocationTriggerCollection_CollectionChanged;
        }
        /// <summary>
        /// Find a location that matches the specified ID returns null if non found
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Location trigger that matches ID or null</returns>
        public T FindByID(string id)
        {
            foreach (T LT in Items)
            {
                if (LT.LocationID == id) return LT;
            }
            return null;

        }
        //Sorts the collection uses the Comparison specified in _sortOnChangeComparison when the list is changed
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Move && e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                if (_sortOnChange) Sort(_sortOnChangeComparison);

        }
        //private void LocationTriggerCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    base.OnCollectionChanged(e);


        //}

        /// <summary>
        /// Gets a list of the LocationTriggers that contain the specified point
        /// </summary>
        /// <param name="point">The point to check for locations</param>
        /// <returns>List of locations at the point(can be empty) </returns>
        public IReadOnlyList<T> LocationsAtPoint(MapCoordinate point)
        {
            List<T> locations = new List<T>();
            foreach (T LT in Items)
            {
                if (LT.ContainsPoint(point)) locations.Add(LT);
            }
            return locations.AsReadOnly();
        }
        /// <summary>
        /// Gets a list of the LocationTriggers that contain the specified point
        /// </summary>
        /// <param name="point">The point to check for locations</param>
        /// <returns>List of locations at the point(can be empty) </returns>
        public IReadOnlyList<T> LocationsAtPoint(Position point)
        {
            return LocationsAtPoint(new MapCoordinate(point.Latitude, point.Longitude));
        }
        public IReadOnlyList<T> LocationsInDirection(MapCoordinate position,double bearing,double maxDistance=0)
        {
            List<T> locations = new List<T>();
            foreach (T LT in Items)
            {
                if (maxDistance==0||LT.DistanceTo(position) <= maxDistance)
                {
                    if (LT.BearingRangeFrom(position).ContainsBearing(bearing)) locations.Add(LT);
                }
            }
            locations.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt1.LastDistance); });
            return locations.AsReadOnly();
        }
        public IReadOnlyList<T> LocationsInBearingRange(MapCoordinate position, BearingRange bearing, double maxDistance = 0)
        {
            List<T> locations = new List<T>();
            foreach (T LT in Items)
            {
                if (maxDistance==0||LT.DistanceTo(position) <= maxDistance)
                {
                    if (bearing.ContainsBearing(LT.BearingFrom(position))) locations.Add(LT);
                }
            }
            locations.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt1.LastDistance); });
            return locations.AsReadOnly();
        }
        /// <summary>
        /// Gets a list of locations near the specified position and with the specifed distance
        /// </summary>
        /// <param name="position">The position to measure distances from</param>
        /// <param name="distance">The highest distance to check</param>
        /// <returns>List of LocationTriggers</returns>
        public IReadOnlyList<T> LocationsNear(Position position, double distance)
        {
            return LocationsNear(new MapCoordinate(position), distance);
        }
        /// <summary>
        /// Gets a list of locations near the specified position and with the specifed distance
        /// </summary>
        /// <param name="position">The position to measure distances from</param>
        /// <param name="distance">The highest distance to check</param>
        /// <returns>List of LocationTriggers</returns>
        public IReadOnlyList<T> LocationsNear(MapCoordinate position, double distance)
        {
            List<T> locationsNear = new List<T>();
            foreach (T LT in Items)
            {
                if (LT.DistanceTo(position) <= distance)
                    locationsNear.Add(LT);
            }
            locationsNear.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt1.LastDistance); });
            return locationsNear.AsReadOnly();
        }
        /// <summary>
        /// Gets the specified number of locations nearest the specifed position. 0 for all locations sorted by distance
        /// </summary>
        /// <param name="position">The position to measure distances from</param>
        /// <param name="distance">The number of locations to return</param>
        /// <returns>List of LocationTriggers</returns>
        public IReadOnlyList<T> ClosestLocations(Position position, int number)
        {
            return ClosestLocations(new MapCoordinate(position), number);
        }
        /// <summary>
        /// Gets the specified number of locations nearest the specifed position. 0 for all locations sorted by distance
        /// </summary>
        /// <param name="position">The position to measure distances from</param>
        /// <param name="distance">The number of locations to return</param>
        /// <returns>List of LocationTriggers</returns>
        public IReadOnlyList<T> ClosestLocations(MapCoordinate position, int number)
        {
            UpdateDistances(position);
            List<T> locations = new List<T>();
            var SortedLocations = new List<T>(Items.OrderBy(LT => LT.LastDistance));
            if (number > SortedLocations.Count) number = SortedLocations.Count;
            if (number > 0)
                return SortedLocations.GetRange(0, number).AsReadOnly();
            else return SortedLocations.AsReadOnly();
        }
        /// <summary>
        /// Update the distances from the specified location for all the Locations in the collection
        /// </summary>
        /// <param name="position">The position to measure from</param>
        public void UpdateDistances(MapCoordinate position)
        {
            foreach (T LT in Items)
            {
                LT.DistanceTo(position);
            }
        }
        /// <summary>
        /// Update the distancesfrom the specified location for all the Locations in the collection
        /// </summary>
        /// <param name="position">The position to measure from</param>
        public void UpdateDistances(Position position)
        {
            UpdateDistances(new MapCoordinate(position));
        }
        /// <summary>
        /// Update the bearings from the specified location for all the Locations in the collection
        /// </summary>
        /// <param name="position">The position to measure from</param>
        public void UpdateBearings(MapCoordinate position)
        {
            foreach (LocationTrigger LT in Items)
            {
                LT.BearingFrom(position);
            }
        }
        /// <summary>
        /// Update the bearings from the specified location for all the Locations in the collection
        /// </summary>
        /// <param name="position">The position to measure from</param>
        public void UpdateBearings(Position position)
        {
            UpdateBearings(new MapCoordinate(position));
        }
        /// <summary>
        /// Set the collection to auto sort the location using the specifed comparison.  <code>delegate (LocationTrigger lt1, LocationTrigger lt2) { return lt1.LastDistance.CompareTo(lt1.LastDistance); }</code>
        /// </summary>
        /// <param name="comparison">The Comparison to use to sort the collection</param>
        public void SortOnChange(Comparison<T> comparison)
        {
            _sortOnChange = true;
            _sortOnChangeComparison = comparison;
            Sort(comparison);
        }
        /// <summary>
        /// Stop autosorting the collection after starting with SortOnChange()
        /// </summary>
        public void StopSortingOnChange()
        {
            _sortOnChange = false;

        }
        /// <summary>
        /// Set the collection to auto sort the location using the specifed comparison.  <code>delegate (LocationTrigger lt1, LocationTrigger lt2) { return lt1.LastDistance.CompareTo(lt1.LastDistance); }</code>
        /// </summary>
        /// <param name="comparison">The Comparison to use to sort the collection</param>
        public void Sort(Comparison<T> comparison)
        {
            var sortableList = new List<T>(Items);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
            {
                Move(Items.IndexOf(sortableList[i]), i);
            }
        }
    }
}
