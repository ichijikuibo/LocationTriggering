
using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    /// <summary>
    /// arguments for the event that is triggered whenever there is a change to the current locations
    /// </summary>
    public class LocationTriggeredEventArgs<T> : EventArgs where T:LocationTrigger 
    {
        public DateTime TimeTriggered { get; set; }
        public MapCoordinate GPSPosition { get; set; }
        public IReadOnlyList<T> LocationsAdded { get; set; }
        public IReadOnlyList<T> LocationsRemoved { get; set; }
        public IReadOnlyList<T> CurrentLocations { get; set; }
    }
    /// <summary>
    /// The event that is triggered whenever there is a change to the current triggers
    /// </summary>
    public delegate void LocationTriggerEventHandler<T>(object sender, LocationTriggeredEventArgs<T> e) where T:LocationTrigger;

    /// <summary>
    /// arguments for the events that is triggered whenever a location is exited or entered
    /// </summary>
    public class LocationUpdatedEventArgs<T> : EventArgs where T : LocationTrigger
    {
        public DateTime TimeTriggered { get; set; }
        public MapCoordinate GPSPosition { get; set; }
        public T Location { get; set; }
    }
    /// <summary>
    /// Event that is triggered whenever a new location is entered
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public delegate void LocationEnteredEventHandler<T>(object sender, LocationUpdatedEventArgs<T> e) where T : LocationTrigger;
    /// <summary>
    /// Event that is triggered whenever a location is exited
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public delegate void LocationExitedEventHandler<T>(object sender, LocationUpdatedEventArgs<T> e) where T : LocationTrigger;
    /// <summary>
    /// Arguments for the event that is triggered whenever there is a gps update
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public class  PositionUpdatedEventArgs: EventArgs
    {
        public DateTime TimeTriggered { get; set; }
        public MapCoordinate GPSPosition { get; set; }
    }
    /// <summary>
    /// The event that is triggered whenever there is a gps update
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public delegate void PositionUpdatedEventHandler(object sender, PositionUpdatedEventArgs e);
    public delegate void ExceptionEventHandler(object sender, Exception e);



}
