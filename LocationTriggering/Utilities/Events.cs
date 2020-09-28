using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    /// <summary>
    /// arguments for the event that is triggered whenever there is a change to the current locations
    /// </summary>
    public class LocationTriggeredEventArgs :EventArgs
    {
        public DateTime TimeTriggered { get; set; }
        public Position GPSPosition { get; set; }
        public IReadOnlyList<LocationTrigger> LocationsEntered { get; set; }
        public IReadOnlyList<LocationTrigger> LocationsExited { get; set; }
        public IReadOnlyList<LocationTrigger> CurrentLocations { get; set; }
    }
    /// <summary>
    /// The event that is triggered whenever there is a change to the current triggers
    /// </summary>
    public delegate void LocationTriggerEventHandler(object sender, LocationTriggeredEventArgs e);

    /// <summary>
    /// arguments for the events that is triggered whenever a location is exited or entered
    /// </summary>
    public class LocationUpdatedEventArgs : EventArgs
    {
        public DateTime TimeTriggered { get; set; }
        public Position GPSPosition { get; set; }
        public LocationTrigger Location { get; set; }
    }
    /// <summary>
    /// Event that is triggered whenever a new location is entered
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public delegate void LocationEnteredEventHandler(object sender, LocationUpdatedEventArgs e);
    /// <summary>
    /// Event that is triggered whenever a location is exited
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public delegate void LocationExitedEventHandler(object sender, LocationUpdatedEventArgs e);
    /// <summary>
    /// Arguments for the event that is triggered whenever there is a gps update
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public class  PositionUpdatedEventArgs : EventArgs
    {
        public DateTime TimeTriggered { get; set; }
        public Position GPSPosition { get; set; }
    }
    /// <summary>
    /// The event that is triggered whenever there is a gps update
    /// </summary>
    /// <param name="sender">The class that sent the event</param>
    /// <param name="e">The arguments for the event</param>
    public delegate void PositionUpdatedEventHandler(object sender, PositionUpdatedEventArgs e);
}
