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
using LocationTriggering.Extentions;
using Location = Xamarin.Essentials.Location;

namespace SampleAndTesting.Tests
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationTriggerCollectionTest : ContentPage
    {
        LocationTriggerCollection<BasicLocationTrigger> locationTriggerCollectionTest;
        IReadOnlyList<LocationTrigger> locationsAtPoint;
        IReadOnlyList<LocationTrigger> locationsInDirection;
        IReadOnlyList<LocationTrigger> locationsNear;
        TestLocationTriggerData _testData;
        List<Polygon> locationPolygons;
        List<Circle> locationCentres;
        List<Circle> radialLocations;
        List<Polyline> polylineLocations;
        Pin currentMapCoordinate;
        public LocationTriggerCollectionTest()
        {
            locationPolygons = new List<Polygon>();
            locationCentres = new List<Circle>();
            polylineLocations = new List<Polyline>();
            radialLocations = new List<Circle>();
            _testData = new TestLocationTriggerData();
            InitializeComponent();
            locationTriggerCollectionTest = new LocationTriggerCollection<BasicLocationTrigger>();
            foreach (BasicLocationTrigger LT in _testData.TestData)
            {
                locationTriggerCollectionTest.Add(LT);
            }
            CoordinateEntry.Text = "54.99730916770572,-7.317796210086995\n" +
                                   "54.99792881899353,-7.312922201157899\n" +
                                   "54.999177765053254,-7.313714660964874\n" +
                                   "54.99847891799941,-7.318895451500169";
            UpdateMap();
            FillLocationPicker();
           
            
            locationTriggerCollectionTest.UpdateDistances(GetTestMapCoordinate());
            SortOnChangePicker.SelectedIndex = 0;

            //SortOnChangePicker_SelectedIndexChanged(this, null);
            MapTest.MapClicked += MapTest_MapClicked;

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            MapTest.MoveToRegion(new MapSpan(new Position(53.576741, -7.753773), 3, 3));
            SortOnChangeList.ItemsSource = locationTriggerCollectionTest;
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
            foreach (Circle circle in radialLocations)
            {
                if (MapTest.MapElements.Contains(circle)) MapTest.MapElements.Remove(circle);
            }
            foreach (Polyline polyline in polylineLocations)
            {
                if (MapTest.MapElements.Contains(polyline)) MapTest.MapElements.Remove(polyline);
            }
            foreach (Circle centre in locationCentres)
            {
                if (MapTest.MapElements.Contains(centre)) MapTest.MapElements.Remove(centre);
            }
            polylineLocations.Clear();
            locationPolygons.Clear();
            radialLocations.Clear();
            locationCentres.Clear();
            foreach (LocationTrigger LT in locationTriggerCollectionTest)
            {
                if (LT.LocationType == TriggerType.Polygon)
                {
                    Polygon polygon = new Polygon();
                    if (locationsAtPoint != null && locationsAtPoint.Contains(LT)) polygon.StrokeColor = Color.Green;
                    if (locationsInDirection != null && locationsInDirection.Contains(LT)) polygon.StrokeColor = Color.Blue;
                    if (locationsNear != null && locationsNear.Contains(LT)) polygon.StrokeColor = Color.Red;
                    foreach (MapCoordinate MC in LT.Points)
                    {
                        polygon.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
                    }
                    MapTest.MapElements.Add(polygon);
                    locationPolygons.Add(polygon);
                }
                if (LT.LocationType == TriggerType.Polyline)
                {
                    Polyline polyline = new Polyline();
                    if (locationsAtPoint != null && locationsAtPoint.Contains(LT)) polyline.StrokeColor = Color.Green;
                    if (locationsInDirection != null && locationsInDirection.Contains(LT)) polyline.StrokeColor = Color.Blue;
                    if (locationsNear != null && locationsNear.Contains(LT)) polyline.StrokeColor = Color.Red;
                    foreach (MapCoordinate MC in LT.Points)
                    {
                        polyline.Geopath.Add(new Position(MC.Latitude, MC.Longitude));
                    }
                    MapTest.MapElements.Add(polyline);
                    polylineLocations.Add(polyline);
                }
                if (LT.LocationType == TriggerType.Radial)
                {
                    foreach (MapCoordinate MC in LT.Points)
                    {
                        Circle circle = new Circle();
                    if (locationsAtPoint != null && locationsAtPoint.Contains(LT)) circle.StrokeColor = Color.Green;
                    if (locationsInDirection != null && locationsInDirection.Contains(LT)) circle.StrokeColor = Color.Blue;
                    if (locationsNear != null && locationsNear.Contains(LT)) circle.StrokeColor = Color.Red;

                        circle.Center = new Position(MC.Latitude,MC.Longitude);
                        circle.Radius = new Distance(LT.Radius*1000);
                        MapTest.MapElements.Add(circle);
                        radialLocations.Add(circle);
                    }

                }
                Circle CentreCentoid = new Circle();
                CentreCentoid.FillColor = Color.Blue;
                CentreCentoid.StrokeColor = Color.Blue;
                CentreCentoid.Center = new Position(LT.Centre.Latitude, LT.Centre.Longitude);
                double radius = LT.BoundingBox.Width * 0.02;
                CentreCentoid.Radius = Distance.FromKilometers(radius);

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

        private void AddLocationButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                BasicLocationTrigger newLT = new BasicLocationTrigger(IDEntry.Text, TextToPoint(CoordinateEntry.Text), IDEntry.Text, "Test Data");
                locationTriggerCollectionTest.Add(newLT);
                UpdateMap();
                FillLocationPicker();
                AddLocationResult.Text = "Location added see map for result";

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
                BasicLocationTrigger LT = locationTriggerCollectionTest[RemoveLocationPicker.SelectedIndex];
                locationTriggerCollectionTest.Remove(LT);
                UpdateMap();
                FillLocationPicker();
                RemoveLocationResult.Text = "Location removed see map for result";

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

        private void LocationsInDirectionButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = GetTestMapCoordinate();
                locationsInDirection = locationTriggerCollectionTest.LocationsInDirection(MC,double.Parse(LocationsInDirectionBearingEntry.Text),double.Parse(LocationsInDirectionDistanceEntry.Text));
                LocationsInDirectionResult.Text = "";
                if (locationsInDirection.Count > 0)
                {
                    foreach (LocationTrigger LT in locationsInDirection)
                    {
                        LocationsInDirectionResult.Text += LT.LocationID + ", ";
                    }
                    LocationsInDirectionResult.Text.TrimEnd(' ').TrimEnd(',');
                }
                else
                {
                    LocationsInDirectionResult.Text = "No Results";
                }
                UpdateMap();
            }
            catch (Exception exception)
            {
                LocationsAtPointResult.Text = exception.Message;
            }
        }
        private void LocationsInBearingRangeButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = GetTestMapCoordinate();
                BearingRange BR = new BearingRange(double.Parse(LocationsInBearingRangeStartEntry.Text), double.Parse(LocationsInBearingRangeEndEntry.Text));
                locationsInDirection = locationTriggerCollectionTest.LocationsInBearingRange(MC, BR, double.Parse(LocationsInBearingRangeDistanceEntry.Text));
                LocationsInBearingRangeResult.Text = "";
                if (locationsInDirection.Count > 0)
                {
                    foreach (LocationTrigger LT in locationsInDirection)
                    {
                        LocationsInBearingRangeResult.Text += LT.LocationID + ", ";
                    }
                    LocationsInBearingRangeResult.Text.TrimEnd(' ').TrimEnd(',');
                }
                else
                {
                    LocationsInBearingRangeResult.Text = "No Results";
                }
                UpdateMap();
            }
            catch (Exception exception)
            {
                LocationsInBearingRangeResult.Text = exception.Message;
            }
        }
        private void LocationsNearButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = GetTestMapCoordinate();
                locationsNear = locationTriggerCollectionTest.LocationsNear(MC, double.Parse(LocationsNearEntry.Text));
                LocationsNearResult.Text = "";
                if (locationsNear.Count > 0)
                {
                    foreach (LocationTrigger LT in locationsNear)
                    {
                        LocationsNearResult.Text += LT.LocationID + ", ";
                    }
                    LocationsNearResult.Text.TrimEnd(' ').TrimEnd(',');
                }
                else
                {
                    LocationsNearResult.Text = "No Results";
                }
                UpdateMap();
            }
            catch (Exception exception)
            {
                LocationsNearResult.Text = exception.Message;
            }
        }
        private void ClosestLocationsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                MapCoordinate MC = GetTestMapCoordinate();
                var closestLocations = locationTriggerCollectionTest.ClosestLocations(MC, int.Parse(ClosestLocationsEntry.Text));
                LocationsNearResult.Text = "";
                ClosestLocationsList.ItemsSource = closestLocations;
                ClosestLocationsResult.Text = "Success see list";
            }
            catch (Exception exception)
            {
                ClosestLocationsResult.Text = exception.Message;
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

        private void SortOnChangePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(SortOnChangePicker.SelectedItem.ToString())
            {
                case "Distance Ascending": 
                    locationTriggerCollectionTest.SortOnChange(delegate (BasicLocationTrigger lt1, BasicLocationTrigger lt2) { return lt1.LastDistance.CompareTo(lt2.LastDistance); });
                    break;
                case "Distance Descending":
                    locationTriggerCollectionTest.SortOnChange(delegate (BasicLocationTrigger lt1, BasicLocationTrigger lt2) { return lt2.LastDistance.CompareTo(lt1.LastDistance); });
                    break;
                case "ID Ascending":
                    locationTriggerCollectionTest.SortOnChange(delegate (BasicLocationTrigger lt1, BasicLocationTrigger lt2) { return lt1.LocationID.CompareTo(lt2.LocationID); });
                    break;
                case "ID Descending":
                    locationTriggerCollectionTest.SortOnChange(delegate (BasicLocationTrigger lt1, BasicLocationTrigger lt2) { return lt2.LocationID.CompareTo(lt1.LocationID); });
                    break;
                case "Title Ascending":
                    locationTriggerCollectionTest.SortOnChange(delegate (BasicLocationTrigger lt1, BasicLocationTrigger lt2) { return (lt1.Title.CompareTo(lt2.Title)); });
                    break;
                case "Title Descending":
                    locationTriggerCollectionTest.SortOnChange(delegate (BasicLocationTrigger lt1, BasicLocationTrigger lt2) { return (lt2.Title.CompareTo(lt1.Title)); });
                    break;
            }

        }
    }
}