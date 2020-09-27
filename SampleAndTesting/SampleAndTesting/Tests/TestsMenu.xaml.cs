using LocationTriggering.Utilities;
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
    public partial class TestsMenu : ContentPage
    {
        public TestsMenu()
        {
            InitializeComponent();
        }

        private void MapCoordinate_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MapCoordinateTests());
        }
        private void LocationTrigger_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LocationTriggerTest());
        }
        private void LocationTriggerCollection_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LocationTriggerCollectionTest());
        }
        private void BearingRange_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BearingRangeTests());
        }
    }
}