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
using System.Data;

namespace SampleAndTesting.Tests
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BoundingBoxTest : ContentPage
    {
        BasicLocationTrigger testLocationTrigger;
        MapBoundingBox testBoundingBox;
        TestLocationTriggerData _testData;
        Polygon polygon;
        Polygon boundingBox;
        Polyline bearingRangeLine;
        Polyline bearingRangeLine2;
        Pin currentMapCoordinate;
        Pin locationCentre;
        Pin closestPin;
        public BoundingBoxTest()
        {
            _testData = new TestLocationTriggerData();
            InitializeComponent();
            ConstructButton_Clicked(this, null);
            FillLocationPicker();
            MapTest.MapClicked += MapTest_MapClicked;
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
            foreach(BasicLocationTrigger BLT in _testData.TestData)
            {
                LocationPicker.Items.Add(BLT.LocationID);
            }
            LocationPicker.Items.Add("Custom");
            LocationPicker.SelectedIndex = LocationPicker.Items.Count - 1;
        }
        private void UpdateProperties()
        {
            ConstructorResults.Text = testBoundingBox.ToString() + " HeightM: " + testBoundingBox.Height + " WidthM: " + testBoundingBox.Width
                + " HeightD: " + testBoundingBox.HeightDegrees + " WidthD: " + testBoundingBox.Width;

            UpdateMap();



        }
        private void UpdateMap()
        {
            if (MapTest.MapElements.Contains(polygon)) MapTest.MapElements.Remove(polygon);
            if (MapTest.Pins.Contains(locationCentre)) MapTest.Pins.Remove(locationCentre);
            if (testLocationTrigger != null)
            {
                polygon = new Polygon();
                foreach (MapCoordinate MC in testLocationTrigger.Points)
                {
                    polygon.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
                }
                MapTest.MapElements.Add(polygon);                
                locationCentre = new Pin();
                locationCentre.Type = PinType.Place;
                locationCentre.Label = "LocationTrigger.Centre";
                locationCentre.Position = new Position(testLocationTrigger.Centre.Latitude, testLocationTrigger.Centre.Longitude);
                MapTest.Pins.Add(locationCentre);
            }
            if (MapTest.MapElements.Contains(boundingBox)) MapTest.MapElements.Remove(boundingBox);
            boundingBox = new Polygon();
            boundingBox.StrokeColor = Color.Blue;
            boundingBox.Geopath.Add(new Position(testBoundingBox.Northwest.Latitude, testBoundingBox.Northwest.Longitude));
            boundingBox.Geopath.Add(new Position(testBoundingBox.Northwest.Latitude, testBoundingBox.Southeast.Longitude));
            boundingBox.Geopath.Add(new Position(testBoundingBox.Southeast.Latitude, testBoundingBox.Southeast.Longitude));
            boundingBox.Geopath.Add(new Position(testBoundingBox.Southeast.Latitude, testBoundingBox.Northwest.Longitude));


            MapTest.MapElements.Add(boundingBox);
            MapTest.MoveToRegion(new MapSpan(new Position(testBoundingBox.Centre.Latitude, testBoundingBox.Centre.Longitude), testBoundingBox.HeightDegrees, testBoundingBox.WidthDegrees));


        }
        private void ConstructButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                testBoundingBox = new MapBoundingBox(new MapCoordinate(double.Parse(NWLatitudeEntry.Text), double.Parse(NWLongitudeEntry.Text)), new MapCoordinate(double.Parse(SELatitudeEntry.Text), double.Parse(SELongitudeEntry.Text)));
                testLocationTrigger = null;
                UpdateProperties();
            }
            catch(Exception exception)
            {
                ConstructorResults.Text = exception.Message;
            }
        }
        private void SetClosestPin(Position pinPosition)
        {
            if (MapTest.Pins.Contains(closestPin)) MapTest.Pins.Remove(closestPin);
            closestPin = new Pin();
            closestPin.Position = pinPosition;
            closestPin.Type = PinType.SearchResult;
            closestPin.Label = "ClosestPoint";
            MapTest.Pins.Add(closestPin);
        }
        private void BearingRangeFromButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var testCoordinate = GetTestMapCoordinate();
                var result = testBoundingBox.BearingRangeFrom(testCoordinate);
                BearingRangeFromResult.Text = result.ToString() + "\nRange: " + result.Range + ", Centre: " + testCoordinate.BearingTo(testBoundingBox.Centre);
                double size = testBoundingBox.Width;
                if(testBoundingBox.Height>size) size = testBoundingBox.Height;
                double lineLength = testCoordinate.DistanceTo(testBoundingBox.Centre) + size;

                MapCoordinate Point1 = testCoordinate.PointAtDistanceAndBearing(lineLength, result.Start);
                MapCoordinate Point2 = testCoordinate.PointAtDistanceAndBearing(lineLength, result.End);

                if (MapTest.MapElements.Contains(bearingRangeLine)) MapTest.MapElements.Remove(bearingRangeLine);
                bearingRangeLine = new Polyline();
                bearingRangeLine.StrokeColor = Color.Red;
                bearingRangeLine.Geopath.Add(new Position(testCoordinate.Latitude, testCoordinate.Longitude));
                bearingRangeLine.Geopath.Add(new Position(Point2.Latitude, Point2.Longitude));
                MapTest.MapElements.Add(bearingRangeLine);
                if (MapTest.MapElements.Contains(bearingRangeLine2)) MapTest.MapElements.Remove(bearingRangeLine2);
                bearingRangeLine2 = new Polyline();
                bearingRangeLine2.StrokeColor = Color.Red;
                bearingRangeLine2.Geopath.Add(new Position(testCoordinate.Latitude, testCoordinate.Longitude));
                bearingRangeLine2.Geopath.Add(new Position(Point1.Latitude, Point1.Longitude));
                MapTest.MapElements.Add(bearingRangeLine2);

            }
            catch (Exception exception)
            {
                BearingRangeFromResult.Text = exception.Message;
            }
        }
        private void ContainsPointButton_Clicked(object sender, EventArgs e)
        {
            try
            {

                ContainsPointResult.Text = testBoundingBox.ContainsPoint(GetTestMapCoordinate()).ToString();
            }
            catch (Exception exception)
            {
                ContainsPointResult.Text = exception.Message;
            }
        }
        private void EqualsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapBoundingBox MBB = new MapBoundingBox(new MapCoordinate(double.Parse(EqualsNWLatitudeEntry.Text), double.Parse(EqualsNWLongitudeEntry.Text)), new MapCoordinate(double.Parse(EqualsSELatitudeEntry.Text), double.Parse(EqualsSELongitudeEntry.Text)));
                EqualsResults.Text = testBoundingBox.Equals(MBB).ToString();
            }
            catch (Exception exception)
            {
                EqualsResults.Text = exception.Message;
            }
        }
        private void LocationPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedItem = LocationPicker.SelectedItem.ToString();
                if (selectedItem == "Custom")
                {
                    EntryRow1.Height = 35;
                    EntryRow2.Height = 35;
                    EntryRow3.Height = 35;
                    EntryRow4.Height = 35;
                    EntryRow5.Height = 35;
                    NWLatitudeEntry.IsVisible = true;
                    NWLongitudeEntry.IsVisible = true;
                    SELatitudeEntry.IsVisible = true;
                    SELongitudeEntry.IsVisible = true;
                    NWLatitudeLabel.IsVisible = true;
                    NWLongitudeLabel.IsVisible = true;
                    SELatitudeLabel.IsVisible = true;
                    SELongitudeLabel.IsVisible = true;
                    ConstructorButton.IsVisible = true;

                }
                else
                {
                    EntryRow1.Height = 0;
                    EntryRow2.Height = 0;
                    EntryRow3.Height = 0;
                    EntryRow4.Height = 0;
                    EntryRow5.Height = 0;
                    NWLatitudeEntry.IsVisible = false;
                    NWLongitudeEntry.IsVisible = false;
                    SELatitudeEntry.IsVisible = false;
                    SELongitudeEntry.IsVisible = false;
                    NWLatitudeLabel.IsVisible = false;
                    NWLongitudeLabel.IsVisible = false;
                    SELatitudeLabel.IsVisible = false;
                    SELongitudeLabel.IsVisible = false;
                    ConstructorButton.IsVisible = false;
                    LocationTrigger LT = _testData.GetLocation(selectedItem);
                    if (LT != null)
                    {
                        testLocationTrigger = (BasicLocationTrigger)LT;
                        testBoundingBox = LT.BoundingBox;
                    }
                    UpdateProperties();



                }
            }
            catch (Exception exception)
            {
                ConstructorResults.Text = exception.Message;
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