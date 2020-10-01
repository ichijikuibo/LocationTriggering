using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using LocationTriggering;
using SampleAndTesting.Data;
using Xamarin.Forms.Maps;
using LocationTriggering.Utilities;
using Polygon = Xamarin.Forms.Maps.Polygon;
using Xamarin.Essentials;

namespace SampleAndTesting.Tests
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationListenerTest : ContentPage
    {
        LocationListener<BasicLocationTrigger> testLocationListener;
        IReadOnlyList<BasicLocationTrigger> locationsAtPoint;
        TestLocationTriggerData _testData;
        List<Polygon> locationPolygons;
        List<Circle> locationCentres;
        Pin currentMapCoordinate;
        public LocationListenerTest()
        {
            InitializeComponent();
            testLocationListener = new LocationListener<BasicLocationTrigger>();
            testLocationListener.PositionUpdated += TestLocationListener_PositionUpdated;
            testLocationListener.LocationsChanged += TestLocationListener_LocationsChanged;
            
            locationPolygons = new List<Polygon>();
            locationCentres = new List<Circle>();
            _testData = new TestLocationTriggerData();

            foreach (BasicLocationTrigger LT in _testData.TestData)
            {
                testLocationListener.LocationTriggers.Add(LT);
            }
            currentPositions.ItemsSource = testLocationListener.CurrentLocationTriggers;
            closestPositions.ItemsSource = testLocationListener.ClosestLocations;
            UpdateMap();
            MapTest.MapClicked += MapTest_MapClicked;
            
        }

        private void TestLocationListener_LocationsChanged(object sender, LocationTriggeredEventArgs<BasicLocationTrigger> e)
        {
            locationsAtPoint = e.CurrentLocations;
        }

        private void TestLocationListener_PositionUpdated(object sender, PositionUpdatedEventArgs e)
        {
            CurentPositionLabel.Text = e.GPSPosition.Latitude + ", " + e.GPSPosition.Longitude;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MapTest.MoveToRegion(new MapSpan(new Position(53.576741, -7.753773), 3, 3));
        }
        private void MapTest_MapClicked(object sender, MapClickedEventArgs e)
        {
            MapLatitudeEntry.Text = e.Position.Latitude.ToString();
            MapLogitudeEntry.Text = e.Position.Longitude.ToString();
        }
        private void SetAsCurrentButton_Clicked(object sender, EventArgs e)
        {
            LongitudeEntry.Text = MapLogitudeEntry.Text;
            LatitudeEntry.Text = MapLatitudeEntry.Text;
        }
        private async void CopyButton_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(MapLatitudeEntry.Text + "," + MapLogitudeEntry.Text);
        }
        private MapCoordinate GetTestMapCoordinate()
        {
            double longitude = double.Parse(LongitudeEntry.Text);
            double latitude = double.Parse(LatitudeEntry.Text);
            if (MapTest.Pins.Contains(currentMapCoordinate)) MapTest.Pins.Remove(currentMapCoordinate);
            currentMapCoordinate = new Pin();
            currentMapCoordinate.Label = "GetTestMapCoordinate()";
            currentMapCoordinate.Type = PinType.SavedPin;
            currentMapCoordinate.Position = new Position(latitude, longitude);
            MapTest.Pins.Add(currentMapCoordinate);
            return new MapCoordinate(latitude, longitude);
        }
        private async void CurrentLocationButton_Clicked(object sender, EventArgs e)
        {
            Location current = await Geolocation.GetLocationAsync();
            LongitudeEntry.Text = current.Longitude.ToString();
            LatitudeEntry.Text = current.Latitude.ToString();
        }

        private void UpdateMap()
        {
            foreach (Polygon polygon in locationPolygons)
            {
                if (MapTest.MapElements.Contains(polygon)) MapTest.MapElements.Remove(polygon);
            }
            foreach (Circle centre in locationCentres)
            {
                if (MapTest.MapElements.Contains(centre)) MapTest.MapElements.Remove(centre);
            }

            locationPolygons.Clear();

            locationCentres.Clear();
            foreach (BasicLocationTrigger LT in testLocationListener.LocationTriggers)
            {
                Polygon polygon = new Polygon();
                Circle CentreCentoid = new Circle();
                CentreCentoid.FillColor = Color.Blue;
                CentreCentoid.StrokeColor = Color.Blue;
                CentreCentoid.Center = new Position(LT.Polygon.Centre.Y, LT.Polygon.Centre.X);
                double radius = LT.BoundingBox.Width * 0.02;
                CentreCentoid.Radius = Distance.FromKilometers(radius);
                if (locationsAtPoint!=null&&locationsAtPoint.Contains(LT)) polygon.StrokeColor = Color.Green;
                foreach (MapCoordinate MC in LT.Points)
                {
                    polygon.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
                }
                MapTest.MapElements.Add(polygon);
                locationPolygons.Add(polygon);
                MapTest.MapElements.Add(CentreCentoid);
                locationCentres.Add(CentreCentoid);
            }         

            
            


        }
        private List<MapCoordinate> TextToPoint(string text)
        {
            List<MapCoordinate> points = new List<MapCoordinate>();
            text = text.TrimEnd('\n').TrimEnd('\r').TrimEnd('\n');
            string[] splitCoordinates = text.Replace(" ", "").Replace("\r", "\n").Replace("\n\n", "\n").Split('\n');
            foreach (string s in splitCoordinates)
            {
                string[] splitCoordinate = s.Split(',');
                points.Add(new MapCoordinate(double.Parse(splitCoordinate[0]), double.Parse(splitCoordinate[1])));
            }
            return points;
        }
        private void ContentPage_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width < 500) TestColumn.Width = this.Width - 15;
            else TestColumn.Width = 500;
            if (this.Width > 800 || this.Width > this.Height)
            {
                MapStack.SetValue(Grid.RowProperty, 0);
                MapStack.SetValue(Grid.ColumnProperty, 1);
                MapStack.SetValue(Grid.ColumnSpanProperty, 1);
                MapStack.SetValue(Grid.RowSpanProperty, 2);
                TestScroll.SetValue(Grid.RowProperty, 0);
                TestScroll.SetValue(Grid.ColumnProperty, 0);
                TestScroll.SetValue(Grid.ColumnSpanProperty, 1);
                TestScroll.SetValue(Grid.RowSpanProperty, 2);
            }
            else
            {
                MapStack.SetValue(Grid.RowProperty, 1);
                MapStack.SetValue(Grid.ColumnProperty, 0);
                MapStack.SetValue(Grid.ColumnSpanProperty, 2);
                MapStack.SetValue(Grid.RowSpanProperty, 1);
                TestScroll.SetValue(Grid.RowProperty, 0);
                TestScroll.SetValue(Grid.ColumnProperty, 0);
                TestScroll.SetValue(Grid.ColumnSpanProperty, 2);
                TestScroll.SetValue(Grid.RowSpanProperty, 1);

            }
        }

        private void StartListeningButton_Clicked(object sender, EventArgs e)
        {
            if (!testLocationListener.IsListening)
            {
                try
                {
                    testLocationListener.StartListening(new TimeSpan(0, 0, int.Parse(IntervalEntry.Text)), double.Parse(DistanceEntry.Text));
                }
                catch (Exception exception)
                {
                    CurentPositionLabel.Text = exception.Message;
                }
            }
        }

        private void StopListeningButton_Clicked(object sender, EventArgs e)
        {
            if (testLocationListener.IsListening)
            {
                try
                {
                    testLocationListener.StopListening();
                }
                catch (Exception exception)
                {
                    CurentPositionLabel.Text = exception.Message;
                }
            }
        }

        private void ManualButton_Clicked(object sender, EventArgs e)
        {
            MapCoordinate MC = GetTestMapCoordinate();
            testLocationListener.ProcessLocation(new Plugin.Geolocator.Abstractions.Position(MC.Latitude, MC.Longitude));
        }
    }
}