using LocationTriggering;
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
    public partial class BearingRangeTests : ContentPage
    {
        private BearingRange bearingRange;
        public BearingRangeTests()
        {
            InitializeComponent();
            ConstructButton_Clicked(this, null);
        }
        private void ConstructButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                bearingRange = new BearingRange(double.Parse(StartBearingEntry.Text), double.Parse(EndBearingEntry.Text));
                ConstructResult.Text = bearingRange.ToString() + "\nRange: " + bearingRange.Range + "\nCentre: " + bearingRange.Centre;
            }
            catch (Exception exception)
            {
                ConstructResult.Text = exception.Message;
            }
        }
        private void ToStringButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                ToStringResult.Text = bearingRange.ToString();
            }
            catch (Exception exception)
            {
                ToStringResult.Text = exception.Message;
            }
        }

        private void ContainBearingButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                double bearing = double.Parse(ContainBearingEntry.Text);
                ContainBearingResult.Text = bearingRange.ContainsBearing(bearing).ToString();
            }
            catch (Exception exception)
            {
                ContainBearingResult.Text = exception.Message;
            }
        }
        
        private void EqualsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                BearingRange BR = new BearingRange(double.Parse(EqualsStartBearingEntry.Text), double.Parse(EqualsEndBearingEntry.Text));
                EqualsResult.Text = BR.Equals(bearingRange).ToString();
            }
            catch (Exception exception)
            {
                EqualsResult.Text = exception.Message;
            }

        }
        private void OverlapsWithButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                BearingRange BR = new BearingRange(double.Parse(OverlapsWithStartBearingEntry.Text), double.Parse(OverlapsWithEndBearingEntry.Text));
                OverlapsWithResult.Text = BR.OverlapsWith(bearingRange).ToString();
            }
            catch (Exception exception)
            {
                OverlapsWithResult.Text = exception.Message;
            }

        }
    }
}