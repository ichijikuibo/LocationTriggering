using AutomatedTests.TestData;
using LocationTriggering;
using LocationTriggering.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AutomatedTests
{
    [TestClass]
    public class Requirement1Tests
    {
        [TestMethod]
        public void MapCoordinateConstructorTest()
        {
            string[] tests = Helpers.OpenFile("TestData\\CoordinateTestData.txt");
            foreach (string test in tests)
            {
                string[] split = test.Split('\t');
                string[] input = split[0].Split(',');
                string[] result = split[1].Split(',');
                MapCoordinate testCoordinate = new MapCoordinate(0, 0);

                MapCoordinate resultCoordinate = new MapCoordinate(0, 0);
                if (result[0].ToUpper() == "NT")
                {
                    Assert.ThrowsException<InvalidCoordinateException>(() => testCoordinate = new MapCoordinate(double.Parse(input[0]), double.Parse(input[1])), String.Format("Expected for '{0}': false; Actual: {1}", input, result));
                }
                else
                {
                    testCoordinate = new MapCoordinate(double.Parse(input[0]), double.Parse(input[1]));
                    resultCoordinate = new MapCoordinate(double.Parse(result[0]), double.Parse(result[1]));
                    Assert.AreEqual(Math.Round(testCoordinate.Latitude, 6), Math.Round(resultCoordinate.Latitude, 6), String.Format("Expected for '{0}'; Actual: {1}", resultCoordinate.Latitude.ToString(), testCoordinate.Latitude.ToString()));
                    Assert.AreEqual(Math.Round(testCoordinate.Longitude, 6), Math.Round(resultCoordinate.Longitude, 6), String.Format("Expected for '{0}'; Actual: {1}", resultCoordinate.Longitude.ToString(), testCoordinate.Longitude.ToString()));
                }
            }
        }
        [TestMethod]
        public void LocationTriggerCentre()
        {
            Data.TestLocationTriggerData testData = new Data.TestLocationTriggerData();
            string[] tests = Helpers.OpenFile("TestData\\LocationTestData.txt");
            foreach (string test in tests)
            {
                string[] split = test.Split('\t');
                string[] input = split[0].Split(',');
                string[] result = split[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                Assert.AreEqual(Math.Round(double.Parse(result[0]), 6), Math.Round(TestTrigger.Centre.Latitude, 6),TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[1]), 6), Math.Round(TestTrigger.Centre.Longitude, 6), TestTrigger.LocationID);
            }
        }
        [TestMethod]
        public void LocationTriggerBoundingBox()
        {
            Data.TestLocationTriggerData testData = new Data.TestLocationTriggerData();
            string[] tests = Helpers.OpenFile("TestData\\LocationTestData.txt");
            foreach (string test in tests)
            {
                string[] split = test.Split('\t');
                string[] input = split[0].Split(',');
                string[] result = split[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                Assert.AreEqual(Math.Round(double.Parse(result[2]), 6), Math.Round(TestTrigger.BoundingBox.Northwest.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[3]), 6), Math.Round(TestTrigger.BoundingBox.Northwest.Longitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[4]), 6), Math.Round(TestTrigger.BoundingBox.Southeast.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[5]), 6), Math.Round(TestTrigger.BoundingBox.Southeast.Longitude, 6), TestTrigger.LocationID);
            }
        }
        [TestMethod]
        public void LocationTriggerAddPoints()
        {
            Data.TestLocationTriggerData testData = new Data.TestLocationTriggerData();
            string[] tests = Helpers.OpenFile("TestData\\AddPointTestData.txt");
            foreach (string test in tests)
            {
                string[] split = test.Split('\t');
                string[] input = split[0].Split(',');
                string[] result = split[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                List<MapCoordinate> coordinates = new List<MapCoordinate>();
                for(int i =1;i< input.Length;i+=2)
                {
                    coordinates.Add(new MapCoordinate(double.Parse(input[i]), double.Parse(input[i + 1])));
                }
                TestTrigger.AddRange(coordinates);
                Assert.AreEqual(Math.Round(double.Parse(result[0]), 6), Math.Round(TestTrigger.Centre.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[1]), 6), Math.Round(TestTrigger.Centre.Longitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[2]), 6), Math.Round(TestTrigger.BoundingBox.Northwest.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[3]), 6), Math.Round(TestTrigger.BoundingBox.Northwest.Longitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[4]), 6), Math.Round(TestTrigger.BoundingBox.Southeast.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[5]), 6), Math.Round(TestTrigger.BoundingBox.Southeast.Longitude, 6), TestTrigger.LocationID);
            }

        }
        [TestMethod]
        public void LocationTriggerRemovePoints()
        {
            Data.TestLocationTriggerData testData = new Data.TestLocationTriggerData();
            string[] tests = Helpers.OpenFile("TestData\\RemovePointTestData.txt");
            foreach (string test in tests)
            {
                string[] split = test.Split('\t');
                string[] input = split[0].Split(',');
                string[] result = split[1].Split(',');
                BasicLocationTrigger TestTrigger = (BasicLocationTrigger)testData.GetLocation(input[0]);
                for (int i = 1; i < input.Length; i += 1)
                {
                    TestTrigger.RemovePoint(int.Parse(input[i]));
                }
                Assert.AreEqual(Math.Round(double.Parse(result[0]), 6), Math.Round(TestTrigger.Centre.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[1]), 6), Math.Round(TestTrigger.Centre.Longitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[2]), 6), Math.Round(TestTrigger.BoundingBox.Northwest.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[3]), 6), Math.Round(TestTrigger.BoundingBox.Northwest.Longitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[4]), 6), Math.Round(TestTrigger.BoundingBox.Southeast.Latitude, 6), TestTrigger.LocationID);
                Assert.AreEqual(Math.Round(double.Parse(result[5]), 6), Math.Round(TestTrigger.BoundingBox.Southeast.Longitude, 6), TestTrigger.LocationID);
            }

        }
        static LocationTriggerCollection<TestLocationTrigger> TestLocations = new LocationTriggerCollection<TestLocationTrigger>();
        [TestMethod]
        public void LocationTriggerDerived()
        {

            string[] tests = Helpers.OpenFile("TestData\\DerivedClassTestData.txt");
            foreach (string test in tests)
            {
                string[] split = test.Split('\t');
                string[] input = split[0].Split(',');
                string[] result = split[1].Split(',');
                TestLocationTrigger testTrigger = new TestLocationTrigger(input[0], result[0]);
                TestLocations.Add(testTrigger);
                Assert.AreEqual(result[0], TestLocations.FindByID(input[0]).Result);
            }
        }
    }
}
