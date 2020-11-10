using LocationTriggering;
using LocationTriggering.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedTests
{
    [TestClass]
    public class Requirement2Tests
    {
        Data.TestLocationTriggerData testData;
        [TestMethod]
        public void PointInPolygonTest()
        {
            string[] tests = Helpers.OpenFile("TestData/ContainsPointTestData.txt");
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
        public void PointInBoundingBoxTest()
        {
            string[] tests = Helpers.OpenFile("TestData/BoundingBoxContainsPointTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                if (result[0].ToUpper() == "NT")
                {
                    Assert.IsFalse(TestTrigger.BoundingBox.ContainsPoint(new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]))), String.Format("Expected for '{0}': false; Actual: true", Helpers.StringFromArray(input)));
                }
                else
                {
                    Assert.IsTrue(TestTrigger.BoundingBox.ContainsPoint(new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]))), String.Format("Expected for '{0}': true; Actual: false", Helpers.StringFromArray(input)));
                }
            }
        }
        [TestMethod]
        public void LocationsAtPoint()
        {
            LocationTriggerCollection<BasicLocationTrigger> testCollection = new LocationTriggerCollection<BasicLocationTrigger>();
            testCollection.AddRange(testData.TestData);
            string[] tests = Helpers.OpenFile("TestData/LocationsAtPointTestData.txt");
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
                    Assert.AreEqual(results.Count, result.Length, String.Format("Expected for '{0}': {1}; Actual: {2}",Helpers.StringFromArray(input), result.Length,results.Count));
                    foreach(var r in results)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID), String.Format("Expected for '{0}': {1}; Actual: Not Found", Helpers.StringFromArray(input), r.LocationID));
                    }
                }
            }
        }
        [TestMethod]
        public void BoundingBoxConstructorTest()
        {
            
            string[] tests = Helpers.OpenFile("TestData/BoundingBoxConstructorTestData.txt");
            foreach (string test in tests)
            {
                string[] split = test.Split('\t');
                string[] input = split[0].Split(',');
                string[] result = split[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                Assert.AreEqual(Math.Round(double.Parse(result[0]), 6), Math.Round(TestTrigger.BoundingBox.WidthDegrees, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[1]), 6), Math.Round(TestTrigger.BoundingBox.HeightDegrees, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[2]), 2), Math.Round(TestTrigger.BoundingBox.Width, 2), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[3]), 2), Math.Round(TestTrigger.BoundingBox.Height, 2), TestTrigger.LocationID);
            }
        }
        [TestInitialize]
        public void IntializeTests()
        {
            testData = new Data.TestLocationTriggerData();
        }
    }
}

