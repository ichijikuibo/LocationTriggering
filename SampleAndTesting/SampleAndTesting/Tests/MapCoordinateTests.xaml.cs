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
            double longitude = double.Parse(LongitudeEntry.Text);
            double latitude = double.Parse(LatitudeEntry.Text);
            mapCoordinate = new MapCoordinate(latitude, longitude);
        }

        private void DistanceToButton_Clicked(object sender, EventArgs e)
        {
            double distance = mapCoordinate.DistanceTo(GetTestMapCoordinate());
            DistanceToResult.Text = distance + "m";
        }

        private void DistanceToFeetButton_Clicked(object sender, EventArgs e)
        {

            double distance = mapCoordinate.DistanceToFeet(GetTestMapCoordinate());
            DistanceToFeetResult.Text = distance + "ft";
        }
        private void BearingToButton_Clicked(object sender, EventArgs e)
        {

            double bearing = mapCoordinate.BearingTo(GetTestMapCoordinate());
            BearingToResult.Text = bearing + " Degrees";
        }
        private void BearingFromButton_Clicked(object sender, EventArgs e)
        {

            double bearing = mapCoordinate.BearingFrom(GetTestMapCoordinate());
            BearingFromResult.Text = bearing + " Degrees";
        }
        private void ToStringButton_Clicked(object sender, EventArgs e)
        {

            ToStringResult.Text = mapCoordinate.ToString();
        }
        private void EqualsButton_Clicked(object sender, EventArgs e)
        {

            EqualsResult.Text = mapCoordinate.Equals(GetTestMapCoordinate()).ToString();
        }
    }
}