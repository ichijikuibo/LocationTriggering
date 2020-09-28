# LocationTriggering
LocationTriggering is a class library for Xamarin.Forms that is designed to make it easy to create a location based application that displays data to the user dependant on where they are.


## Basic Usage
```
private void BasicLocationTriggerSample()
{
    //Create an instance of the location listener class
    LocationListener listener = new LocationListener();
    listener.LocationsChanged += Listener_LocationsChanged;
    //Create some map coordinates to form a polygon that makes up the location
    MapCoordinate[] londonPoints = new MapCoordinate[] {new MapCoordinate(51.71182591666487, -0.1264198857978871),
    new MapCoordinate(51.63598601587619, -0.4862170391781862),new MapCoordinate(51.46048894879159, -0.5666421769559327),
    new MapCoordinate(51.34014069255859, -0.4178664157713519), new MapCoordinate(51.28553146167313, -0.1436100153632802),
    new MapCoordinate(51.27327722676611, 0.1129864005541115), new MapCoordinate(51.37300465668022, 0.263469481534373),
    new MapCoordinate(51.56248825999992, 0.3789070374479286), new MapCoordinate(51.69593440691544, 0.1076557715779813)};

    //Create an instance of a class inherited from LocationTrigger with the points and the information
    BasicLocationTrigger london = new BasicLocationTrigger("London", londonPoints, "London", "Capital city of England");
    //Add the location's object to the LocationaTriggerCollection in LocationListener
    listener.LocationTriggers.Add(london);

    //Add more locations
    MapCoordinate[] parisPoints = new MapCoordinate[] {new MapCoordinate(48.77722216746136,2.161709809090213), new MapCoordinate(48.73084876120987,2.235814063969268),
    new MapCoordinate(48.6975904208622,2.410007684154414),new MapCoordinate(48.6912584687781,2.620424692765133),
    new MapCoordinate(48.79625850828037,2.642334415022416),new MapCoordinate(48.88409247254472,2.641387951030956),
    new MapCoordinate(48.99664375620552,2.565568311307791),new MapCoordinate(49.01179869658374,2.347396843066536),
    anew MapCoordinate(48.94537589150431,2.177910117161548),new MapCoordinate(48.84534555641764,2.101415147749033)};
    BasicLocationTrigger paris = new BasicLocationTrigger("Paris", parisPoints, "Paris", "Capital city of France");
    listener.LocationTriggers.Add(paris);
    
    //Start the listener and have it update every 1 second and 5 meters moved
    listener.StartListening(new TimeSpan(0,0,1),5);
}

private void Listener_LocationsChanged(object sender, LocationTriggering.Utilities.LocationTriggeredEventArgs e)
{
    if (e.CurrentLocations.Count > 0)//Check if there is at least 1 location at the current position 
    {
        //Convert the LocationTrigger to a BasicLocationTrigger or any class inherited from LocationTrigger
        BasicLocationTrigger BLT = e.CurrentLocations[0] as BasicLocationTrigger;
        locationLabel.Text = "You are in " + BLT.Title;
        detailsLabel.Text = BLT.Description;
    }
    else //There is no locations at the current position
    {
        locationLabel.Text = "The centre of the universe";
        detailsLabel.Text = "because everywhere is the centre of the universe";
    }
}
```
## Limitations
- Triggers can not cross the north or south pole
- Triggers can not have a width greater than 180 degrees

## Plans
1. Add radial and polyline based locations
2. Add support for locations greater than 180 degrees logitude
3. Add latitude normalisation and support locations containing poles
4. Implement Vincenty solutions of geodesics on the ellipsoid for more accurate distance calculations https://www.movable-type.co.uk/scripts/latlong-vincenty.html
5. Add monitoring of orientation sensor and send events when phone is facing a LocationTrigger
6. Add navigation feature for directions to a location