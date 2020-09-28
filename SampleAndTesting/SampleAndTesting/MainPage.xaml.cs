using SampleAndTesting.Tests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using SampleAndTesting.Samples;

namespace SampleAndTesting
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e) //Open the Testing menu
        {
            Navigation.PushAsync(new TestsMenu());
        }
        private void Samples_Tapped(object sender, EventArgs e) //Open the Testing menu
        {
            Navigation.PushAsync(new SamplesMenu());
        }
    }
}
