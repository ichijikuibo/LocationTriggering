using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace LocationTriggering.Utilities
{
    public class LocationTriggeredEventArgs :EventArgs
    {
        public DateTime TimeTriggered { get; set; }
        public Location Position { get; set; }
        public IReadOnlyList<LocationTrigger> LocationsEntered { get; set; }
        public IReadOnlyList<LocationTrigger> LocationsExited { get; set; }
        public IReadOnlyList<LocationTrigger> CurrentLocations { get; set; }
    }
    public delegate void LocationTriggerEventHandler(object sender, LocationTriggeredEventArgs e);


    public class LocationUpdatedEventArgs : EventArgs
    {
        public DateTime TimeTriggered { get; set; }
        public Location Position { get; set; }
        public LocationTrigger Location { get; set; }
    }
    public delegate void LocationEnteredEventHandler(object sender, LocationUpdatedEventArgs e);
    public delegate void LocationExitedEventHandler(object sender, LocationUpdatedEventArgs e);

    public class  PositionUpdatedEventArgs : EventArgs
    {
        public DateTime TimeTriggered { get; set; }
        public Location Position { get; set; }
    }
    public delegate void PositionUpdatedEventHandler(object sender, PositionUpdatedEventArgs e);
}
