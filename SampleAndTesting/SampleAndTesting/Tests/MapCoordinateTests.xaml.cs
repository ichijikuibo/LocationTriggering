using LocationTriggering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleAndTesting.Tests
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapCoordinateTests : ContentPage
    {
        private MapCoordinate mapCoordinate;
        public MapCoordinateTests()
        {
            InitializeComponent();
            mapCoordinate = new MapCoordinate(54.995547, -7.321983);
            LatitudeEntry.Text = "54.995547";
            LongitudeEntry.Text = "-7.321983";
        }
        private MapCoordinate GetTestMapCoordinate()
        {
            double longitude = double.Parse(DistanceToLongitudeEntry.Text);
            double latitude = double.Parse(DistanceToLatitudeEntry.Text);
            return new MapCoordinate(latitude, longitude);
        }
        private void ConstructButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                double longitude = 0;
                double latitude = 0;
                if(LongitudeEntry.Text!="")
                longitude = double.Parse(LongitudeEntry.Text);
                if (LatitudeEntry.Text != "")
                    latitude = double.Parse(LatitudeEntry.Text);
                if (LongitudeEntry.Text == "")
                    mapCoordinate = new MapCoordinate(latitude, double.NaN);
                else if (LatitudeEntry.Text == "")
                    mapCoordinate = new MapCoordinate(double.NaN, longitude);
                else
                    mapCoordinate = new MapCoordinate(latitude, longitude);
                ConstructResult.Text = mapCoordinate.ToString();
            }
            catch(Exception exception)
            {
                ConstructResult.Text = exception.Message;
            }
        }

        private void DistanceToButton_Clicked(object sender, EventArgs e)
        {
            try { 
            double distance = mapCoordinate.DistanceTo(GetTestMapCoordinate());
            DistanceToResult.Text = distance + "km";
            }
            catch (Exception exception)
            {
                DistanceToResult.Text = exception.Message;
            }
        }

        private void DistanceToFeetButton_Clicked(object sender, EventArgs e)
        {
            try {                
                double distance = mapCoordinate.DistanceTo(GetTestMapCoordinate(),LocationTriggering.Utilities.DistanceUnit.Feet);
                DistanceToFeetResult.Text = distance + "ft";
            }
            catch (Exception exception)
            {
                DistanceToFeetResult.Text = exception.Message;
            }
        }
        private void BearingToButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                double bearing = mapCoordinate.BearingTo(GetTestMapCoordinate());
                BearingToResult.Text = bearing + " Degrees";
            }
            catch (Exception exception)
            {
                BearingToResult.Text = exception.Message;
            }
        }
        private void BearingFromButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                double bearing = mapCoordinate.BearingFrom(GetTestMapCoordinate());
                BearingFromResult.Text = bearing + " Degrees";
            }
            catch (Exception exception)
            {
                BearingToResult.Text = exception.Message;
            }
        }
        private void PointAtDistanceAndBearingButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                double distance = double.Parse(PointAtDistanceAndBearingDistanceEntry.Text);
                double bearing = double.Parse(PointAtDistanceAndBearingBearingEntry.Text);
                MapCoordinate point = mapCoordinate.PointAtDistanceAndBearing(distance, bearing);
                PointAtDistanceAndBearingResult.Text = point.ToString();
            }
            catch (Exception exception)
            {
                PointAtDistanceAndBearingResult.Text = exception.Message;
            }
        }
        private void ToStringButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                ToStringResult.Text = mapCoordinate.ToString();
            }
            catch (Exception exception)
            {
                ToStringResult.Text = exception.Message;
            }
        }
        private void EqualsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                EqualsResult.Text = mapCoordinate.Equals(GetTestMapCoordinate()).ToString();
            }
            catch (Exception exception)
            {
                EqualsResult.Text = exception.Message;
            }
        }
    }
}