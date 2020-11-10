using LocationTriggering;
using LocationTriggering.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedTests
{
    [TestClass]
    public class Requirement3Tests
    {
        Data.TestLocationTriggerData testData;
        [TestMethod]
        public void DistanceTo()
        {
            
            string[] tests = Helpers.OpenFile("TestData/2PointTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[0]), double.Parse(input[1]));
                MapCoordinate MC2 = new MapCoordinate(double.Parse(input[2]), double.Parse(input[3]));
                double distance = Helpers.RoundToSignificantDigits(MC1.DistanceTo(MC2), 4);
                Assert.AreEqual(distance, double.Parse(result[0]), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result[0], distance));
            }
        }
        [TestMethod]
        public void ClosestPointTo()
        {
            string[] tests = Helpers.OpenFile("TestData/ClosestPointTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]));
                MapCoordinate MCResult = new MapCoordinate(double.Parse(result[0]), double.Parse(result[1]));               
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                MapCoordinate ActualResult = TestTrigger.ClosestPointTo(MC1);
                Assert.AreEqual(Math.Round(MCResult.Longitude,5), Math.Round(ActualResult.Longitude, 5));
                Assert.AreEqual( Math.Round(MCResult.Latitude,5), Math.Round(ActualResult.Latitude, 5));
            }
        }
        [TestMethod]
        public void NearbyLocations()
        {
            string[] tests = Helpers.OpenFile("TestData/NearbyLocationsTestData.txt");
            LocationTriggerCollection<BasicLocationTrigger> testCollection = new LocationTriggerCollection<BasicLocationTrigger>();
            testCollection.AddRange(testData.TestData);
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                var ActualResult = testCollection.LocationsNear(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])), double.Parse(input[2]));
                if(result[0].ToLower() == "nt")
                {
                    Assert.AreEqual(0,ActualResult.Count);
                }
                else if(result[0].ToLower()=="everything")
                {
                    Assert.AreEqual(testCollection.Count,ActualResult.Count);
                }
                else
                {
                    Assert.AreEqual(result.Length, ActualResult.Count);
                    foreach(var r in ActualResult)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID));
                    }
                }
            }
        }
        [TestMethod]
        public void ClosestLocations()
        {
            string[] tests = Helpers.OpenFile("TestData/ClosestLocationsTestData.txt");
            LocationTriggerCollection<BasicLocationTrigger> testCollection = new LocationTriggerCollection<BasicLocationTrigger>();
            testCollection.AddRange(testData.TestData);
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                var ActualResult = testCollection.ClosestLocations(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])), int.Parse(input[2]));
                if (result[0].ToLower() == "everything")
                {
                    Assert.AreEqual(testCollection.Count, ActualResult.Count);
                }
                else
                {
                    Assert.AreEqual(result.Length, ActualResult.Count);
                    foreach (var r in ActualResult)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID));
                    }
                }
            }
        }
        [TestMethod]
        public void SortedLocations()
        {
            string[] tests = Helpers.OpenFile("TestData/ClosestLocationsTestData.txt");
            LocationTriggerCollection<BasicLocationTrigger> testCollection = new LocationTriggerCollection<BasicLocationTrigger>();
            testCollection.AddRange(testData.TestData);
            int count = 0;
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] expectedResults = test[1].Split(',');
                var ActualResult = testCollection.ClosestLocations(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])), int.Parse(input[2]));
                if (expectedResults[0].ToLower() == "everything")
                {
                    Assert.AreEqual(testCollection.Count, ActualResult.Count);
                }
                else
                {
                    Assert.AreEqual(expectedResults.Length, ActualResult.Count);
                    for (int i= 0;i<expectedResults.Length;i++)
                    {
                        Assert.AreEqual(expectedResults[i], ActualResult[i].LocationID);
                    }
                }
                count++;
            }
        }
        [TestInitialize]
        public void IntializeTests()
        {
            testData = new Data.TestLocationTriggerData();
        }
    }
}

