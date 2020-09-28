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
    public partial class LocationTriggerCollectionTest : ContentPage
    {
        LocationTriggerCollection locationTriggerCollectionTest;
        IReadOnlyList<LocationTrigger> locationsAtPoint;
        TestLocationTriggerData _testData;
        List<Polygon> locationPolygons;
        List<Circle> locationCentres;
        List<Circle> locationCentres2;
        Pin currentMapCoordinate;
        public LocationTriggerCollectionTest()
        {
            locationPolygons = new List<Polygon>();
            locationCentres = new List<Circle>();
            locationCentres2 = new List<Circle>();
            _testData = new TestLocationTriggerData();
            InitializeComponent();
            locationTriggerCollectionTest = new LocationTriggerCollection();
            foreach (LocationTrigger LT in _testData.TestData)
            {
                locationTriggerCollectionTest.Add(LT);
            }
            CoordinateEntry.Text = "54.99730916770572,-7.317796210086995\n" +
                                   "54.99792881899353,-7.312922201157899\n" +
                                   "54.999177765053254,-7.313714660964874\n" +
                                   "54.99847891799941,-7.318895451500169";
            UpdateMap();
            FillLocationPicker();
            MapTest.MapClicked += MapTest_MapClicked;

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
            DistanceToLongitudeEntry.Text = MapLogitudeEntry.Text;
            DistanceToLatitudeEntry.Text = MapLatitudeEntry.Text;
        }
        private async void CopyButton_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(MapLatitudeEntry.Text + "," + MapLogitudeEntry.Text);
        }
        private MapCoordinate GetTestMapCoordinate()
        {
            double longitude = double.Parse(DistanceToLongitudeEntry.Text);
            double latitude = double.Parse(DistanceToLatitudeEntry.Text);
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
            DistanceToLongitudeEntry.Text = current.Longitude.ToString();
            DistanceToLatitudeEntry.Text = current.Latitude.ToString();
        }
        private void FillLocationPicker()
        {
            RemoveLocationPicker.Items.Clear();
            foreach (LocationTrigger BLT in locationTriggerCollectionTest)
            {
                RemoveLocationPicker.Items.Add(BLT.LocationID);
            }
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
            foreach (Circle centre in locationCentres2)
            {
                if (MapTest.MapElements.Contains(centre)) MapTest.MapElements.Remove(centre);
            }
            locationPolygons.Clear();
            locationCentres2.Clear();
            locationCentres.Clear();
            foreach (LocationTrigger LT in locationTriggerCollectionTest)
            {
                Polygon polygon = new Polygon();
                Circle centreAverage = new Circle();
                Circle CentreCentoid = new Circle();
                centreAverage.FillColor = Color.Green;
                CentreCentoid.FillColor = Color.Blue;
                centreAverage.StrokeColor = Color.Green;
                CentreCentoid.StrokeColor = Color.Blue;
                centreAverage.Center = new Position(LT.Centre.Latitude, LT.Centre.Longitude);
                CentreCentoid.Center = new Position(LT.Polygon.Centre.Y, LT.Polygon.Centre.X);
                double radius = LT.BoundingBox.width * 0.02;
                centreAverage.Radius = Distance.FromKilometers(radius);
                CentreCentoid.Radius = Distance.FromKilometers(radius);
                if (locationsAtPoint!=null&&locationsAtPoint.Contains(LT)) polygon.StrokeColor = Color.Green;
                foreach (MapCoordinate MC in LT.Points)
                {
                    polygon.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
                }
                MapTest.MapElements.Add(polygon);
                locationPolygons.Add(polygon);
                MapTest.MapElements.Add(centreAverage);
                MapTest.MapElements.Add(CentreCentoid);
                locationCentres2.Add(CentreCentoid);
                locationCentres.Add(centreAverage);
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

        private void AddLocationButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                LocationTrigger newLT = new BasicLocationTrigger(IDEntry.Text, TextToPoint(CoordinateEntry.Text), IDEntry.Text, "Test Data");
                locationTriggerCollectionTest.Add(newLT);
                UpdateMap();
                FillLocationPicker();
                AddLocationResult.Text = "Point added see map for result";

            }
            catch (Exception exception)
            {
                AddLocationResult.Text = exception.Message;
            }
        }
        private void RemoveLocationButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                LocationTrigger LT = locationTriggerCollectionTest[RemoveLocationPicker.SelectedIndex];
                locationTriggerCollectionTest.Remove(LT);
                UpdateMap();
                FillLocationPicker();
                RemoveLocationResult.Text = "Point removed see map for result";

            }
            catch (Exception exception)
            {
                RemoveLocationResult.Text = exception.Message;
            }
        }
        private void LocationsAtPoint_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = GetTestMapCoordinate();
                locationsAtPoint = locationTriggerCollectionTest.LocationsAtPoint(MC);
                LocationsAtPointResult.Text = "";
                if (locationsAtPoint.Count > 0)
                {
                    foreach (LocationTrigger LT in locationsAtPoint)
                    {
                        LocationsAtPointResult.Text += LT.LocationID + ", ";
                    }
                    LocationsAtPointResult.Text.TrimEnd(' ').TrimEnd(',');
                }
                else
                {
                    LocationsAtPointResult.Text = "No Results";
                }
                UpdateMap();
            }
            catch (Exception exception)
            {
               LocationsAtPointResult.Text = exception.Message;
            }
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


    }
}