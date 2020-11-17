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
    public partial class LocationTriggerTest : ContentPage
    {
        BasicLocationTrigger testLocationTrigger;
        TestLocationTriggerData _testData;
        Polygon polygon;
        List<Circle> radialLocation;
        Polyline polylineLocation;
        Polygon boundingBox;
        Polygon pointsInBoundingBox;
        Polygon overlapsWithPolygon;
        Polyline bearingRangeLine;
        Polyline bearingRangeLine2;
        Pin currentMapCoordinate;
        Pin locationCentre;
        Pin closestPin;
        public LocationTriggerTest()
        {
            _testData = new TestLocationTriggerData();
            InitializeComponent();
            radialLocation = new List<Circle>();
            CoordinateEntry.Text = "54.99730916770572,-7.317796210086995\n" +
                                   "54.99792881899353,-7.312922201157899\n" +
                                   "54.999177765053254,-7.313714660964874\n" +
                                   "54.99847891799941,-7.318895451500169";
            OverlapsWithEntry.Text = CoordinateEntry.Text;
            TypePicker.SelectedIndex = 0;
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
                OverlapsWithPicker.Items.Add(BLT.LocationID);
            }
            LocationPicker.Items.Add("Custom");
            LocationPicker.SelectedIndex = LocationPicker.Items.Count - 1;
            OverlapsWithPicker.Items.Add("Custom");
            OverlapsWithPicker.SelectedIndex = LocationPicker.Items.Count - 1;
        }
        private void UpdateProperties()
        {
            ConstructorResults.Text = "Centre: " + testLocationTrigger.Centre.ToString() +
                "\nBoundingBox: " + testLocationTrigger.BoundingBox;
            CoordinateEntry.Text = "";
            if (testLocationTrigger.NumberOfPoints < 100)
            {
                for (int i = 0; i < testLocationTrigger.NumberOfPoints; i++)
                {
                    CoordinateEntry.Text += testLocationTrigger.GetPoint(i) + "\n";
                }
            }
            CoordinateEntry.Text.TrimEnd('\n').TrimEnd('\r');
            RemovePointPicker.Items.Clear();
            foreach (MapCoordinate MC in testLocationTrigger.Points)
            {
                RemovePointPicker.Items.Add(MC.ToString());
            }
            RemovePointPicker.SelectedIndex = 0;
            UpdateMap();



        }
        private void UpdateMap()
        {
            if (MapTest.MapElements.Contains(polygon)) MapTest.MapElements.Remove(polygon);
            foreach (Circle c in radialLocation)
            {
                if (MapTest.MapElements.Contains(c)) MapTest.MapElements.Remove(c);
            }
            if (MapTest.MapElements.Contains(polylineLocation)) MapTest.MapElements.Remove(polylineLocation);
            if (testLocationTrigger.LocationType == TriggerType.Polygon)
            {
                polygon = new Polygon();
                foreach (MapCoordinate MC in testLocationTrigger.Points)
                {
                    polygon.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
                }
                MapTest.MapElements.Add(polygon);
            }
            else if (testLocationTrigger.LocationType == TriggerType.Radial)
            {
                radialLocation.Clear();
                foreach (MapCoordinate MC in testLocationTrigger.Points)
                {
                    Circle circle = new Circle();

                    circle.Center = new Position(MC.Latitude, MC.Longitude);
                    circle.Radius = new Distance(testLocationTrigger.Radius * 1000);
                    radialLocation.Add(circle);
                    MapTest.MapElements.Add(circle);
                }
            }
            else
            {
                polylineLocation = new Polyline();
                foreach (MapCoordinate MC in testLocationTrigger.Points)
                {
                    polylineLocation.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
                }
                MapTest.MapElements.Add(polylineLocation);
            }

            if (MapTest.Pins.Contains(locationCentre)) MapTest.Pins.Remove(locationCentre);
            locationCentre = new Pin();
            locationCentre.Type = PinType.Place;
            locationCentre.Label = "LocationTrigger.Centre";
            locationCentre.Position = new Position(testLocationTrigger.Centre.Latitude, testLocationTrigger.Centre.Longitude);
            MapTest.Pins.Add(locationCentre);
            if (MapTest.MapElements.Contains(boundingBox)) MapTest.MapElements.Remove(boundingBox);
            boundingBox = new Polygon();
            boundingBox.StrokeColor = Color.Blue;
            boundingBox.Geopath.Add(new Position(testLocationTrigger.BoundingBox.Northwest.Latitude, testLocationTrigger.BoundingBox.Northwest.Longitude));
            boundingBox.Geopath.Add(new Position(testLocationTrigger.BoundingBox.Northwest.Latitude, testLocationTrigger.BoundingBox.Southeast.Longitude));
            boundingBox.Geopath.Add(new Position(testLocationTrigger.BoundingBox.Southeast.Latitude, testLocationTrigger.BoundingBox.Southeast.Longitude));
            boundingBox.Geopath.Add(new Position(testLocationTrigger.BoundingBox.Southeast.Latitude, testLocationTrigger.BoundingBox.Northwest.Longitude));
            MapTest.MapElements.Add(boundingBox);
            double height = testLocationTrigger.BoundingBox.HeightDegrees;
            double width = testLocationTrigger.BoundingBox.WidthDegrees;
            double centreLat = testLocationTrigger.Centre.Latitude;
            MapSpan span;
            if (height >= 180)
            {
                 span = new MapSpan(new Position(0, testLocationTrigger.Centre.Longitude), 180, 180);
            }
            else
            {
                if (testLocationTrigger.Centre.Latitude + height > 90)
                {
                    centreLat = 90 - height - 0.5;
                    //width = height;
                }
                if (testLocationTrigger.Centre.Latitude - height < -90)
                {
                    centreLat = -90 + height + 0.5;
                    //width = height;
                }
                span = new MapSpan(new Position(centreLat, testLocationTrigger.Centre.Longitude), height, width);
            }

            //MapSpan span = MapSpan.FromCenterAndRadius(new Position(testLocationTrigger.Centre.Latitude, testLocationTrigger.Centre.Longitude), new Distance(testLocationTrigger.BoundingBox.Height * 1000));
            MapTest.MoveToRegion(span);


        }
        private void ConstructButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                string id = IDEntry.Text;
                List<MapCoordinate> points = new List<MapCoordinate>();
                CoordinateEntry.Text = CoordinateEntry.Text.TrimEnd('\n').TrimEnd('\r').TrimEnd('\n');
                string[] splitCoordinates = CoordinateEntry.Text.Replace(" ", "").Replace("\r", "\n").Replace("\n\n", "\n").Split('\n');
                foreach (string s in splitCoordinates)
                {
                    string[] splitCoordinate = s.Split(',');
                    points.Add(new MapCoordinate(double.Parse(splitCoordinate[0]), double.Parse(splitCoordinate[1])));
                }
                testLocationTrigger = new BasicLocationTrigger(id, points);
                if (TypePicker.SelectedItem.ToString() == "Radial")
                {
                    testLocationTrigger.LocationType = TriggerType.Radial;
                    testLocationTrigger.Radius = double.Parse(SizeEntryM.Text);
                }
                if (TypePicker.SelectedItem.ToString() == "Polyline")
                {
                    testLocationTrigger.LocationType = TriggerType.Polyline;
                    testLocationTrigger.Radius = double.Parse(SizeEntryM.Text);
                }
                UpdateProperties();
            }
            catch(Exception exception)
            {
                ConstructorResults.Text = exception.Message;
            }
        }
        private void DistanceToButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                double distance = testLocationTrigger.DistanceTo(GetTestMapCoordinate());
                DistanceToResult.Text = distance + "km";
            }
            catch(Exception exception)
            {
                DistanceToResult.Text = exception.Message;
            }
        }
        private void ContainsPointButton_Clicked(object sender, EventArgs e)
        {
            try
            {

                ContainsPointResult.Text = testLocationTrigger.ContainsPoint(GetTestMapCoordinate()).ToString();
            }
            catch (Exception exception)
            {
                ContainsPointResult.Text = exception.Message;
            }
        }
        private void ClosestDistanceToButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = testLocationTrigger.ClosestPointTo(GetTestMapCoordinate());
                SetClosestPin(new Position(MC.Latitude, MC.Longitude));
                ClosestDistanceToResult.Text = testLocationTrigger.ClosestDistanceTo(GetTestMapCoordinate()).ToString()+"m";
            }
            catch (Exception exception)
            {
                ClosestDistanceToResult.Text = exception.Message;
            }
        }
        private void ClosestPointToButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = testLocationTrigger.ClosestPointTo(GetTestMapCoordinate());
                SetClosestPin(new Position(MC.Latitude, MC.Longitude));
                ClosestPointToResult.Text = MC.ToString();
            }
            catch (Exception exception)
            {
                ClosestPointToResult.Text = exception.Message;
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
                var result = testLocationTrigger.BearingRangeFrom(testCoordinate);
                BearingRangeFromResult.Text = result.ToString() + "\nRange: " + result.Range + ", Centre: " + testCoordinate.BearingTo(testLocationTrigger.Centre);

                MapCoordinate[] points = testLocationTrigger.BearingRangePoints(testCoordinate);

                //PointD Rotated = testLocationTrigger.GetPointOnPermiterFrom(testCoordinate).ToPointD();
                //PointD CentrePointD = testLocationTrigger.Centre.ToPointD();
                //PointD Point1 = new PointD(CentrePointD.X - Rotated.X / Math.Cos(CentrePointD.Y / 180 * Math.PI), CentrePointD.Y - Rotated.Y);
                //PointD Point2 = new PointD(CentrePointD.X + Rotated.X / Math.Cos(CentrePointD.Y / 180 * Math.PI), CentrePointD.Y + Rotated.Y);
                if (MapTest.MapElements.Contains(bearingRangeLine)) MapTest.MapElements.Remove(bearingRangeLine);
                bearingRangeLine = new Polyline();
                bearingRangeLine.StrokeColor = Color.Red;
                bearingRangeLine.Geopath.Add(new Position(testCoordinate.Latitude, testCoordinate.Longitude));
                bearingRangeLine.Geopath.Add(new Position(points[1].Latitude, points[1].Longitude));
                MapTest.MapElements.Add(bearingRangeLine);
                if (MapTest.MapElements.Contains(bearingRangeLine2)) MapTest.MapElements.Remove(bearingRangeLine2);
                bearingRangeLine2 = new Polyline();
                bearingRangeLine2.StrokeColor = Color.Red;
                bearingRangeLine2.Geopath.Add(new Position(testCoordinate.Latitude, testCoordinate.Longitude));
                bearingRangeLine2.Geopath.Add(new Position(points[0].Latitude, points[0].Longitude));
                MapTest.MapElements.Add(bearingRangeLine2);

            }
            catch (Exception exception)
            {
                BearingRangeFromResult.Text = exception.Message;
            }
        }

        private void AddPointButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                testLocationTrigger.AddPoint(double.Parse(AddPointLatitudeEntry.Text), double.Parse(AddPointLongitudeEntry.Text));
                UpdateProperties();
                AddPointResult.Text = "Point added see map for result";

            }
            catch (Exception exception)
            {
                AddPointResult.Text = exception.Message;
            }
        }
        private void RemovePointButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = testLocationTrigger.Points[RemovePointPicker.SelectedIndex];
                testLocationTrigger.RemovePoint(MC);
                UpdateProperties();
                RemovePointResult.Text = "Point removed see map for result";

            }
            catch (Exception exception)
            {
                RemovePointResult.Text = exception.Message;
            }
        }
        private void RemovePointIndexButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                testLocationTrigger.RemovePoint(RemovePointPicker.SelectedIndex);
                UpdateProperties();
                RemovePointResult.Text = "Point removed see map for result";

            }
            catch (Exception exception)
            {
                RemovePointResult.Text = exception.Message;
            }
        }
        private void OverlapsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                List<MapCoordinate> points = new List<MapCoordinate>();
                OverlapsWithEntry.Text = OverlapsWithEntry.Text.TrimEnd('\n').TrimEnd('\r').TrimEnd('\n');
                string[] splitCoordinates = OverlapsWithEntry.Text.Replace(" ", "").Replace("\r","\n").Replace("\n\n","\n").Split('\n');
                foreach (string s in splitCoordinates)
                {
                    string[] splitCoordinate = s.Split(',');
                    points.Add(new MapCoordinate(double.Parse(splitCoordinate[0]), double.Parse(splitCoordinate[1])));
                }

                BasicLocationTrigger OverlapsWithTrigger = new BasicLocationTrigger("Test", points);
                OverlapsWithResults.Text = testLocationTrigger.OverlapsWith(OverlapsWithTrigger).ToString();
                OverlapsWithResults.TextColor = Color.Green;
                UpdateOverlapsPolygon(points);

            }
            catch (Exception exception)
            {
                OverlapsWithResults.Text = exception.Message;
            }
        }

        private void PointsInBoundingBoxButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapBoundingBox MBB = new MapBoundingBox(double.Parse(NWLatitudeEntry.Text), double.Parse(NWLongitudeEntry.Text), double.Parse(SELatitudeEntry.Text), double.Parse(SELongitudeEntry.Text));
                var points = testLocationTrigger.GetPointsInBoundingBox(MBB);
                if (points == null)
                {
                    PointsInBoundingBoxResults.Text = "No Results";
                }
                else
                {
                    PointsInBoundingBoxResults.Text = "";
                    foreach (var p in points)
                    {
                        PointsInBoundingBoxResults.Text += p.ToString() + ", ";
                    }
                    PointsInBoundingBoxResults.Text.TrimEnd(' ').TrimEnd(',');
                    if (MapTest.MapElements.Contains(pointsInBoundingBox)) MapTest.MapElements.Remove(pointsInBoundingBox);
                    pointsInBoundingBox = new Polygon();
                    pointsInBoundingBox.StrokeColor = Color.DarkGreen;
                    pointsInBoundingBox.Geopath.Add(new Position(MBB.Northwest.Latitude, MBB.Northwest.Longitude));
                    pointsInBoundingBox.Geopath.Add(new Position(MBB.Northwest.Latitude, MBB.Southeast.Longitude));
                    pointsInBoundingBox.Geopath.Add(new Position(MBB.Southeast.Latitude, MBB.Southeast.Longitude));
                    pointsInBoundingBox.Geopath.Add(new Position(MBB.Southeast.Latitude, MBB.Northwest.Longitude));
                    MapTest.MapElements.Add(pointsInBoundingBox);
                }
            }
            catch (Exception exception)
            {
                PointsInBoundingBoxResults.Text = exception.Message;
            }
        }

        private void UpdateOverlapsPolygon(IReadOnlyList<MapCoordinate> points)
        {
            if (MapTest.MapElements.Contains(overlapsWithPolygon)) MapTest.MapElements.Remove(overlapsWithPolygon);
            overlapsWithPolygon = new Polygon();
            overlapsWithPolygon.StrokeColor = Color.Green;
            foreach (MapCoordinate MC in points)
            {
                overlapsWithPolygon.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
            }
            MapTest.MapElements.Add(overlapsWithPolygon);
        }
        private void LocationPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedItem = LocationPicker.SelectedItem.ToString();
                if (selectedItem == "Custom")
                {
                    IDRow.Height = 35;
                    CoordinateTitleRow.Height = 20;
                    CoordinateRow.Height = 100;
                    ConstructButtonRow.Height = 30;
                    IDLabel.IsVisible = true;
                    IDEntry.IsVisible = true;
                    CoordinatesLabel.IsVisible = true;
                    CoordinateEntry.IsVisible = true;
                    ConstructButton.IsVisible = true;
                    TypeRow.Height = 35;

                    TypeLabel.IsVisible = true;
                    TypePicker.IsVisible = true;
                    if (TypePicker.SelectedItem.ToString() != "Polygon")
                    {
                        SizeRow.Height = 35;
                        SizeLabel.IsVisible = true;
                        SizeEntryM.IsVisible = true;
                    }
                    else
                    {
                        SizeRow.Height = 0;
                        SizeLabel.IsVisible = false;
                        SizeEntryM.IsVisible = false;
                    }
                }
                else
                {
                    IDRow.Height = 0;
                    CoordinateTitleRow.Height = 0;
                    CoordinateRow.Height = 0;
                    ConstructButtonRow.Height = 0;
                    LocationTrigger LT = _testData.GetLocation(selectedItem);
                    if (LT != null)
                    {
                        testLocationTrigger = (BasicLocationTrigger)LT;
                    }
                    UpdateProperties();
                    IDLabel.IsVisible = false;
                    IDEntry.IsVisible = false;
                    CoordinatesLabel.IsVisible = false;
                    CoordinateEntry.IsVisible = false;
                    ConstructButton.IsVisible = false;
                    TypeRow.Height = 0;

                    TypeLabel.IsVisible = false;
                    TypePicker.IsVisible = false;

                    SizeRow.Height = 0;
                    SizeLabel.IsVisible = false;
                    SizeEntryM.IsVisible = false;

                }
            }
            catch (Exception exception)
            {
                OverlapsWithResults.Text = exception.Message;
            }
        }
        private void OverlapsWith_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = OverlapsWithPicker.SelectedItem.ToString();
            if (selectedItem == "Custom")
            {
                OverlapsWithTitleRow.Height = 20;
                OverlapsWithCoordinateRow.Height = 100;
                OverlapsWithButtonRow.Height = 30;

                OverlapsWithCoordinatesLabel.IsVisible = true;
                OverlapsWithEntry.IsVisible = true;
                OverlapsWithButton.IsVisible = true;
            }
            else
            {
                OverlapsWithTitleRow.Height = 0;
                OverlapsWithCoordinateRow.Height = 0;
                OverlapsWithButtonRow.Height = 0;

                LocationTrigger LT = _testData.GetLocation(selectedItem);
                UpdateOverlapsPolygon(LT.Points);
                OverlapsWithResults.Text = testLocationTrigger.OverlapsWith(LT).ToString();
                OverlapsWithCoordinatesLabel.IsVisible = false;
                OverlapsWithEntry.IsVisible = false;
                OverlapsWithButton.IsVisible = false;


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

        private void TypePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TypePicker.SelectedItem.ToString() != "Polygon")
            {
                SizeRow.Height = 35;
                SizeLabel.IsVisible = true;
                SizeEntryM.IsVisible = true;
            }
            else
            {
                SizeRow.Height = 0;
                SizeLabel.IsVisible = false;
                SizeEntryM.IsVisible = false;
            }
        }
    }
}