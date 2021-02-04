using LocationTriggering.Utilities;
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
        //private bool _filterOnChange;
        private Comparison<T> _sortOnChangeComparison;
        private Func<T,bool> _filterCondition;
        private bool _useClosestDistance = true;
        private DistanceUnit _units = DistanceUnit.Kilometres;
        private LocationTriggerCollection<T> _filteredCollection;
        public bool UseClosestDistance { get => _useClosestDistance; set => _useClosestDistance = value; }
        public DistanceUnit Units { get => _units; set => _units = value; }
        public LocationTriggerCollection<T> FilteredCollection { get => _filteredCollection; }
        public Comparison<T> SortOnChangeComparison { get => _sortOnChangeComparison; }

        public LocationTriggerCollection()
        {
            _filteredCollection = this;
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
        public void AddRange(IEnumerable<T> newItems)
        {
            foreach (T LT in newItems)
            {
                Add(LT);
            }
        }
        //Sorts the collection uses the Comparison specified in _sortOnChangeComparison when the list is changed
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (e.Action != NotifyCollectionChangedAction.Move && e.Action != NotifyCollectionChangedAction.Reset)
            {
                if (_sortOnChange) Sort(_sortOnChangeComparison);
            }

        }
        public LocationTriggerCollection<T> filterCollection(Func<T,bool> condition)
        {
            if (FilteredCollection == this)
            {
                _filteredCollection = new LocationTriggerCollection<T>();
                if(_sortOnChangeComparison!=null)_filteredCollection.SortOnChange(_sortOnChangeComparison);
            }
            _filterCondition = condition;
            var filtered = this.Where(condition);
           // _filterOnChange = true;
            List<T> removedLocations = new List<T>();
            foreach(T loc in FilteredCollection)
            {
                if (!filtered.Contains(loc)) removedLocations.Add(loc);
            }
            foreach(T loc in removedLocations)
            {
                FilteredCollection.Remove(loc);
            }
            foreach(T loc in filtered)
            {
                if (!_filteredCollection.Contains(loc)) FilteredCollection.Add(loc);
            }
            return FilteredCollection;
        }
        public void StopFiltering()
        {
           // _filterOnChange = false;
            _filteredCollection = this;
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
        public IReadOnlyList<T> LocationsInDirection(MapCoordinate position,double bearing,double maxDistance=0)
        {
            List<T> locations = new List<T>();
            foreach (T LT in Items)
            {
                if (maxDistance==0||LT.DistanceTo(position,Units,UseClosestDistance) <= maxDistance)
                {
                    if (LT.BearingRangeFrom(position).ContainsBearing(bearing)) locations.Add(LT);
                }
            }
            locations.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); });
            return locations.AsReadOnly();
        }
        public IReadOnlyList<T> LocationsInBearingRange(MapCoordinate position, BearingRange bearing, double maxDistance = 0)
        {
            List<T> locations = new List<T>();
            foreach (T LT in Items)
            {
                if (maxDistance==0||LT.DistanceTo(position, Units, UseClosestDistance) <= maxDistance)
                {
                    if (bearing.OverlapsWith(LT.BearingRangeFrom(position))) locations.Add(LT);
                }
            }
            locations.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); });
            return locations.AsReadOnly();
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
                if (LT.DistanceTo(position, Units, UseClosestDistance) <= distance)
                    locationsNear.Add(LT);
            }
            locationsNear.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); });
            return locationsNear.AsReadOnly();
        }
        /// <summary>
        /// Gets the specified number of locations nearest the specifed position. 0 for all locations sorted by distance
        /// </summary>
        /// <param name="position">The position to measure distances from</param>
        /// <param name="distance">The number of locations to return</param>
        /// <returns>List of LocationTriggers</returns>
        public IReadOnlyList<T> ClosestLocations(MapCoordinate position, int number,IEnumerable<T> exceptions = null)
        {
            UpdateDistances(position,false);
            List<T> locations = new List<T>();
            var SortedLocations = new List<T>(Items);
            SortedLocations.Sort(delegate (T lt1, T lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); });
            if(exceptions!= null)
            {
                foreach(T LT in exceptions)
                {
                    if (SortedLocations.Contains(LT)) SortedLocations.Remove(LT);
                }
            }
            if (number > SortedLocations.Count) number = SortedLocations.Count;
            if (number > 0)
                return SortedLocations.GetRange(0, number).AsReadOnly();
            else return SortedLocations.AsReadOnly();
        }
        /// <summary>
        /// Update the distances from the specified location for all the Locations in the collection
        /// </summary>
        /// <param name="position">The position to measure from</param>
        public void UpdateDistances(MapCoordinate position, bool sort = true)
        {            
            foreach (T LT in Items)
            {
                    LT.DistanceTo(position, Units, UseClosestDistance);
            }
            if (_sortOnChange&&sort&& _sortOnChange)
            {
                Sort(_sortOnChangeComparison);
            }
        }

        /// <summary>
        /// Update the bearings from the specified location for all the Locations in the collection
        /// </summary>
        /// <param name="position">The position to measure from</param>
        public void UpdateBearings(MapCoordinate position, bool sort = true)
        {
            foreach (LocationTrigger LT in Items)
            {
                LT.BearingFrom(position);
            }
            if (_sortOnChange&& sort&& _sortOnChange)
            {
                Sort(_sortOnChangeComparison);
            }
        }
        /// <summary>
        /// Update the bearings and distances from the specified location for all the Locations in the collection
        /// </summary>
        /// <param name="position">The position to measure from</param>
        public void UpdateDistanceAndBearings(MapCoordinate position, bool sort = true)
        {
            foreach (LocationTrigger LT in Items)
            {
                LT.BearingFrom(position);
                LT.DistanceTo(position, Units, UseClosestDistance);
            }
            if (_sortOnChange&&sort&& _sortOnChange)
            {
                Sort(_sortOnChangeComparison);
            }
        }

        /// <summary>
        /// Set the collection to auto sort the location using the specifed comparison.  <code>delegate (LocationTrigger lt1, LocationTrigger lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); }</code>
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
        /// Set the collection to auto sort the location using the specifed comparison.  <code>delegate (LocationTrigger lt1, LocationTrigger lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); }</code>
        /// </summary>
        /// <param name="comparison">The Comparison to use to sort the collection</param>
        public void Sort(Comparison<T> comparison)
        {
            var sortableList = new List<T>(Items);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
            {
                if(sortableList[i]!=this[i])
                    Move(Items.IndexOf(sortableList[i]), i);
            }
        }
    }
}
