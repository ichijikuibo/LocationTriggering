using LocationTriggering;
using LocationTriggering.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedTests
{
    [TestClass]
    public class Requirement4Tests
    {
        Data.TestLocationTriggerData testData;
        [TestMethod]
        public void BearingRangeConstructor()
        {
            
            string[] tests = Helpers.OpenFile("TestData/BearingRangeConstructorTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                BearingRange br = new BearingRange(double.Parse(input[0]), double.Parse(input[1]));
                Assert.AreEqual(Math.Round(br.Start,6), Math.Round(double.Parse(result[0]),6), String.Format("Expected : {0}; Actual: {1}", result[0], br.Start));
                Assert.AreEqual(Math.Round(br.End,6), double.Parse(result[1]), String.Format("Expected : {0}; Actual: {1}", result[1], br.End));
                Assert.AreEqual(Math.Round(br.Range,6), double.Parse(result[2]), String.Format("Expected : {0}; Actual: {1}", result[2], br.Range));
                Assert.AreEqual(Math.Round(br.Centre,6), double.Parse(result[3]), String.Format("Expected : {0}; Actual: {1}", result[3], br.Centre));
            }
        }

        [TestMethod]
        public void ContainBearing()
        {

            string[] tests = Helpers.OpenFile("TestData/ContainsBearingTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                BearingRange br = new BearingRange(double.Parse(input[0]), double.Parse(input[1]));
                if(result[0].ToLower()=="true")
                {
                    Assert.IsTrue(br.ContainsBearing(double.Parse(input[2])), String.Format("Range : {0}; Input: {1}", br.ToString(), input[2]));
                }
                else if (result[0].ToLower() == "false")
                {
                    Assert.IsFalse(br.ContainsBearing(double.Parse(input[2])), String.Format("Range : {0}; Input: {1}", br.ToString(), input[2]));
                }
                else
                {
                    Assert.ThrowsException<Exception>(()=>br.ContainsBearing(double.Parse(input[2])), String.Format("Range : {0}; Input: {1}", br.ToString(), input[2]));
                }

            }
        }

        [TestMethod]
        public void BearingRangeOverlaps()
        {

            string[] tests = Helpers.OpenFile("TestData/BearingRangeOverlapsTestData.txt"); 
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                BearingRange br = new BearingRange(double.Parse(input[0]), double.Parse(input[1]));
                BearingRange br2 = new BearingRange(double.Parse(input[2]), double.Parse(input[3]));
                if (result[0].ToLower() == "true")
                {
                    Assert.IsTrue(br.OverlapsWith(br2), String.Format("Range : {0}; Input: {1}", br.ToString(), br2.ToString()));
                }
                else if (result[0].ToLower() == "false")
                {
                    Assert.IsFalse(br.OverlapsWith(br2), String.Format("Range : {0}; Input: {1}", br.ToString(), br2.ToString()));
                }


            }
        }
        [TestMethod]
        public void bearingTo()
        {

            string[] tests = Helpers.OpenFile("TestData/2PointTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[0]), double.Parse(input[1]));
                MapCoordinate MC2 = new MapCoordinate(double.Parse(input[2]), double.Parse(input[3]));
                double bearing = Math.Round(MC1.BearingTo(MC2));
                Assert.AreEqual(bearing, double.Parse(result[1]), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result[1], bearing));
            }
        }
        [TestMethod]
        public void bearingFrom()
        {

            string[] tests = Helpers.OpenFile("TestData/2PointTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[0]), double.Parse(input[1]));
                MapCoordinate MC2 = new MapCoordinate(double.Parse(input[2]), double.Parse(input[3]));
                double bearing = Math.Round(MC1.BearingFrom(MC2));
                Assert.AreEqual(bearing, double.Parse(result[2]), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result[2], bearing));
            }
        }
        [TestMethod]
        public void PointAtDistanceAndBearing()
        {

            string[] tests = Helpers.OpenFile("TestData/PointAtDistanceAndBearing.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[0]), double.Parse(input[1]));
                MapCoordinate resultMC = MC1.PointAtDistanceAndBearing(double.Parse(input[2]), double.Parse(input[3]));
                Assert.AreEqual(Math.Round(double.Parse(result[0]), 7), Math.Round(resultMC.Latitude,7), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), MC1.ToString(), resultMC.ToString()));
                Assert.AreEqual(Math.Round(double.Parse(result[1]), 7), Math.Round(resultMC.Longitude,7), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), MC1.ToString(), resultMC.ToString()));
            }
        }
        [TestMethod]
        public void BearingRange()
        {

            string[] tests = Helpers.OpenFile("TestData/BearingRangeTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[1]), double.Parse(input[2]));
                BearingRange resultRange = TestTrigger.BearingRangeFrom(MC1);
                Assert.AreEqual(Math.Round(double.Parse(result[0])), Math.Round(resultRange.Start), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), MC1.ToString(), resultRange.ToString()));
                Assert.AreEqual(Math.Round(double.Parse(result[1])), Math.Round(resultRange.End), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), MC1.ToString(), resultRange.ToString()));
            }
        }

        [TestMethod]
        public void LocationsInDirection()
        {
            LocationTriggerCollection<BasicLocationTrigger> testCollection = new LocationTriggerCollection<BasicLocationTrigger>();
            testCollection.UseClosestDistance = false;
            testCollection.AddRange(testData.TestData);
            string[] tests = Helpers.OpenFile("TestData/LocationInDirectionTestData.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                var results = testCollection.LocationsInDirection(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])), double.Parse(input[2]), double.Parse(input[3]));
                if (result[0].ToUpper() == "NOWHERE")
                {
                    
                    Assert.AreEqual(results.Count, 0, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else
                {
                    Assert.AreEqual(results.Count, result.Length, String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result.Length, results.Count));
                    foreach (var r in results)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID), String.Format("Expected for '{0}': {1}; Actual: Not Found", Helpers.StringFromArray(input), r.LocationID));
                    }
                }
            }
        }

        [TestMethod]
        public void LocationsInBearingRange()
        {
            LocationTriggerCollection<BasicLocationTrigger> testCollection = new LocationTriggerCollection<BasicLocationTrigger>();
            testCollection.UseClosestDistance = false;
            testCollection.AddRange(testData.TestData);
            string[] tests = Helpers.OpenFile("TestData/LocationsInBearingRange.txt");
            foreach (string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                var results = testCollection.LocationsInBearingRange(new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])), new BearingRange(double.Parse(input[2]), double.Parse(input[3])), double.Parse(input[4]));
                if (result[0].ToUpper() == "NOWHERE")
                {

                    Assert.AreEqual(results.Count, 0, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else if (result[0].ToUpper() == "EVERYWHERE")
                {

                    Assert.AreEqual(results.Count, testCollection.Count, String.Format("Expected for '{0}': 0; Actual: {1}", Helpers.StringFromArray(input), results.Count));
                }
                else
                {
                    Assert.AreEqual(results.Count, result.Length, String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result.Length, results.Count));
                    foreach (var r in results)
                    {
                        Assert.IsTrue(result.Contains(r.LocationID), String.Format("Expected for '{0}': {1}; Actual: Not Found", Helpers.StringFromArray(input), r.LocationID));
                    }
                }
            }
        }

        [TestInitialize]
        public void IntializeTests()
        {
            testData = new Data.TestLocationTriggerData();
        }
    }
}

