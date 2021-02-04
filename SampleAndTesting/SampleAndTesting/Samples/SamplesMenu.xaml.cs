using LocationTriggering.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleAndTesting.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplesMenu : ContentPage
    {
        public SamplesMenu()
        {
            InitializeComponent();
        }

        private void BasicUsageSample(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BasicUsage());
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Sample());
        }
    }
}