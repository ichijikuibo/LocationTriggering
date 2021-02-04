using LocationTriggering;
using SampleAndTesting.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LocationTriggering.Extentions;

namespace SampleAndTesting.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Sample : ContentPage
    {
        LocationListener<SampleLocation> Listener;
        public Sample()
        {
            InitializeComponent();
            Listener = new LocationListener<SampleLocation>();//Declare an instance of the LocationListener class with your class base on LocationTrigger
            LoadXML();//load data into the Listener
            this.BindingContext = Listener;//Bind the listener to the page to allow data binding
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Listener.StartListening(new TimeSpan(0,0,1),2);//Start listening for gps changes  checking every 1 secondd for changes greater then 2 meters
            Listener.PositionUpdated += Listener_PositionUpdated; // declare and event for when the position is updated to update direction for the closest locations
            Listener.LocationsChanged += Listener_LocationsChanged; //declare an event for when the location is changed to update the ui for when the user is with a location
            Listener.ClosestLocationChanged += Listener_ClosestLocationChanged;// declare and event forupdating the direction whenever a new location is added (PositionUpdated is called first)
        }


        private void Listener_LocationsChanged(object sender, LocationTriggering.Utilities.LocationTriggeredEventArgs<SampleLocation> e)
        {
            if(e.CurrentLocations.Count==0)
            {
                CurrentLocationRow.Height = new GridLength(1, GridUnitType.Star);
                CurrentLocationList.IsVisible = false;
                NeabyLocationsList.HeightRequest = Height;
            }
            else
            {
                CurrentLocationRow.Height = new GridLength(1, GridUnitType.Star);
                CurrentLocationList.IsVisible = true;
                NeabyLocationsList.HeightRequest = Height/2;
            }
        }

        private void Listener_PositionUpdated(object sender, LocationTriggering.Utilities.PositionUpdatedEventArgs e)
        {
            foreach (SampleLocation SL in Listener.ClosestLocations)
            {
                SL.UpdateDirection(e.GPSPosition);
            }
        }

        private void Listener_ClosestLocationChanged(object sender, LocationTriggering.Utilities.LocationTriggeredEventArgs<SampleLocation> e)
        {
            foreach (SampleLocation SL in e.CurrentLocations)
            {
                SL.UpdateDirection(e.GPSPosition);
            }
        }

        private void LoadXML()
        {
            string resourcePrefix = "SampleAndTesting.";
#if __IOS__
            resourcePrefix = "SampleAndTesting.IOS.";
#endif
#if __ANDROID__
            resourcePrefix = "SampleAndTesting.Droid.";
#endif
            XmlDocument docXML = new XmlDocument();
            Stream stream = typeof(Sample).GetTypeInfo().Assembly.GetManifestResourceStream(resourcePrefix + "Data.SampleLocations.xml");
            docXML.Load(stream);
            XmlNodeList locationList = docXML.GetElementsByTagName("Location");
            foreach (XmlNode node in locationList)
            {
                string title = node.Attributes[0].Value.ToString();
                string picture = "https://derrysmarttour.com/Data/Images/" + node.Attributes[1].Value.ToString();
                string thumbnail = "https://derrysmarttour.com/Data/Images/" + node.Attributes[2].Value.ToString();
                SampleLocation newLoc = new SampleLocation(title, picture, thumbnail);
                foreach (XmlNode childnode in node.ChildNodes)
                {
                    if (childnode.Name.ToLower() == "summary")
                    {
                        newLoc.Summary = childnode.InnerText;
                        continue;
                    }
                    if (childnode.Name.ToLower() == "details")
                    {
                        newLoc.Details = childnode.InnerText;
                        continue;
                    }
                    if (childnode.Name.ToLower() == "polygon")
                    {
                        newLoc.LocationType = LocationTriggering.Utilities.TriggerType.Polygon;
                        newLoc.AddRange(GetPoints(childnode));
                        continue;
                    }
                    if (childnode.Name.ToLower() == "circle")
                    {
                        newLoc.Radius = double.Parse(childnode.Attributes[0].Value.ToString());
                        newLoc.LocationType = LocationTriggering.Utilities.TriggerType.Radial;
                        newLoc.AddRange(GetPoints(childnode));
                        continue;
                    }
                    if (childnode.Name.ToLower() == "polyline")
                    {
                        newLoc.Radius = double.Parse(childnode.Attributes[0].Value.ToString());
                        newLoc.LocationType = LocationTriggering.Utilities.TriggerType.Polyline;
                        newLoc.AddRange(GetPoints(childnode));
                        continue;
                    }                    
                }
                Listener.LocationTriggers.Add(newLoc);
            }
        }
        private List<MapCoordinate> GetPoints(XmlNode node)
        {
            List<MapCoordinate> points = new List<MapCoordinate>();
            foreach (XmlNode childnode in node.ChildNodes)
            {
                double lat = double.Parse(childnode.Attributes[1].Value.ToString());
                double lng = double.Parse(childnode.Attributes[0].Value.ToString());
                points.Add(new MapCoordinate(lat, lng));
            }
            return points;
        }
    }
}