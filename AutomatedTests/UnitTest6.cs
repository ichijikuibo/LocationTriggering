using LocationTriggering;
using LocationTriggering.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedTests
{
    [TestClass]
    public class PolylineRadialTests
    {
        Data.TestLocationTriggerData testData;
        [TestMethod]
        public void PointInPolygonTest()
        {
            string[] tests = Helpers.OpenFile("TestData/RadialPolylineContainsPointTestData.txt");
            foreach(string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                if (result[0].ToUpper() == "NT")
                {
                    Assert.IsFalse(TestTrigger.ContainsPoint(new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]))), String.Format("Expected for '{0}': false; Actual: true", Helpers.StringFromArray(input)));
                }
                else
                {
                    Assert.IsTrue(TestTrigger.ContainsPoint(new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]))), String.Format("Expected for '{0}': true; Actual: false", Helpers.StringFromArray(input)));
                }
            }
        }
        [TestMethod]
        public void LocationsAtPoint()
        {
            LocationTriggerCollection<BasicLocationTrigger> testCollection = new LocationTriggerCollection<BasicLocationTrigger>();
            testCollection.AddRange(testData.TestData);
            string[] tests = Helpers.OpenFile("TestData/RadialPolylineLocationsAtPointTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                if (result[0].ToUpper() == "NT")
                {
                    var results = testCollection.LocationsAtPoint(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])));
                    Assert.AreEqual(results.Count,0, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else
                {
                    var results = testCollection.LocationsAtPoint(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])));
                    Assert.AreEqual(result.Length, results.Count, String.Format("Expected for '{0}': {1}; Actual: {2}",Helpers.StringFromArray(input), result.Length,results.Count));
                    foreach(var r in results)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID), String.Format("Expected for '{0}': {1}; Actual: Not Found", Helpers.StringFromArray(input), r.LocationID));
                    }
                }
            }
        }
        [TestMethod]
        public void ClosestPointTo()
        {
            string[] tests = Helpers.OpenFile("TestData/RadialPolylineClosestPoint.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]));
                MapCoordinate MCResult = new MapCoordinate(double.Parse(result[0]), double.Parse(result[1]));
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                MapCoordinate ActualResult = TestTrigger.ClosestPointTo(MC1);
                Assert.AreEqual(Math.Round(MCResult.Longitude, 5), Math.Round(ActualResult.Longitude, 5));
                Assert.AreEqual(Math.Round(MCResult.Latitude, 5), Math.Round(ActualResult.Latitude, 5));
            }
        }
        [TestMethod]
        public void BearingRange()
        {

            string[] tests = Helpers.OpenFile("TestData/RadialPolylineBearingRangeTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]));
                BearingRange resultRange = TestTrigger.BearingRangeFrom(MC1);
                BearingRange expectedRange = new BearingRange(double.Parse(result[0]), double.Parse(result[1]));
                Assert.AreEqual(Math.Round(expectedRange.Start), Math.Round(resultRange.Start), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), expectedRange.ToString(), resultRange.ToString()));
                Assert.AreEqual(Math.Round(expectedRange.End), Math.Round(resultRange.End), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), expectedRange.ToString(), resultRange.ToString()));                
            }
        }
        [TestInitialize]
        public void IntializeTests()
        {
            testData = new Data.TestLocationTriggerData();
            testData.AdditionalTestData();
        }
    }
}

