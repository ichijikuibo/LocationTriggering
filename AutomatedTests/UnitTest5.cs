using LocationTriggering;
using LocationTriggering.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace AutomatedTests
{
    [TestClass]
    public class Requirement5Tests
    {
        Data.TestLocationTriggerData testData;
        bool wait = false;
        LocationListener<BasicLocationTrigger> listener;
        [TestMethod]
        public void UpdatePosition()
        {
            listener.LocationsChanged += Listener_LocationsChanged;
            string[] tests = Helpers.OpenFile("TestData/LocationsAtPointTestData.txt");
            foreach (string s in tests)
            {
                wait = true;
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                listener.Update(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])));
                var results = listener.CurrentLocationTriggers;
                int waits = 0;
                while (wait)
                {
                    waits++;
                    Thread.Sleep(10);
                    if (waits == 20)
                    {
                        break;
                    }
                }
                if (result[0].ToUpper() == "NT")
                {
                    Assert.AreEqual(0, results.Count, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else
                {
                    Assert.AreEqual(result.Length, results.Count, String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result.Length, results.Count));
                    foreach (var r in results)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID), String.Format("Expected for '{0}': {1}; Actual: Not Found", Helpers.StringFromArray(input), r.LocationID));
                    }
                }
            }
        }

        private void Listener_LocationsChanged(object sender, LocationTriggeredEventArgs<BasicLocationTrigger> e)
        {
            wait = false;
        }

        [TestMethod]
        public void UpdateDistances()
        {
            listener.ClosestLocationChanged += Listener_ClosestLocationChanged;
            string[] tests = Helpers.OpenFile("TestData/ClosestLocationsTestData.txt");
            foreach (string s in tests)
            {
                wait = true;
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                listener.NumberOfClosestLocations = int.Parse(input[2]);
                listener.Update(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])));


                int waits = 0;
                while (wait)
                {
                    waits++;
                    Thread.Sleep(40);
                    if (waits == 20)
                    {
                        break;
                    }
                }
                var results = listener.ClosestLocations;
                if (result[0].ToUpper() == "NT")
                {
                    Assert.AreEqual(0, results.Count, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else if (result[0].ToUpper() == "EVERYTHING")
                {
                    Assert.AreEqual(listener.ClosestLocations.Count, results.Count, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else
                {
                    Assert.AreEqual(result.Length, results.Count, String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result.Length, results.Count));
                    foreach (var r in results)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID), String.Format("Expected for '{0}': {1}; Actual: Not Found", Helpers.StringFromArray(input), r.LocationID));
                    }
                }
            }
        }

        private void Listener_ClosestLocationChanged(object sender, LocationTriggeredEventArgs<BasicLocationTrigger> e)
        {
            wait = false;
        }
        [TestMethod]
        public void LocationsInDirection()
        {
            listener.LocationsInDirectionChanged += Listener_LocationsInDirectionChanged; 
            string[] tests = Helpers.OpenFile("TestData/LocationInDirectionTestData.txt");
            foreach (string s in tests)
            {
                wait = true;
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                listener.LocationsInDirectionDistance = int.Parse(input[3]);
                listener.UpdateBearing(double.Parse(input[2]),new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])));


                int waits = 0;
                while (wait)
                {
                    waits++;
                    Thread.Sleep(40);
                    if (waits == 20)
                    {
                        break;
                    }
                }
                var results = listener.LocationTriggersInDirection;
                if (result[0].ToUpper() == "NOWHERE")
                {
                    Assert.AreEqual(0, results.Count, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else if (result[0].ToUpper() == "EVERYTHING")
                {
                    Assert.AreEqual(listener.ClosestLocations.Count, results.Count, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else
                {
                    Assert.AreEqual(result.Length, results.Count, String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result.Length, results.Count));
                    foreach (var r in results)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID), String.Format("Expected for '{0}': {1}; Actual: Not Found", Helpers.StringFromArray(input), r.LocationID));
                    }
                }
            }
        }

        private void Listener_LocationsInDirectionChanged(object sender, LocationTriggeredEventArgs<BasicLocationTrigger> e)
        {
            wait = false;
        }

        [TestInitialize]
        public void IntializeTests()
        {
            testData = new Data.TestLocationTriggerData();
            listener = new LocationListener<BasicLocationTrigger>();
            listener.ChangeGpsDistance(0.0000001);
            listener.LocationTriggers.AddRange(testData.TestData);
        }

    }
}

