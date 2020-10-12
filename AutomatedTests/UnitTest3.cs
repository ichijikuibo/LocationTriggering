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
        [TestMethod]
        public void DistanceTo()
        {
            Data.TestLocationTriggerData testData = new Data.TestLocationTriggerData();
            string[] tests = Helpers.OpenFile("TestData\\2PointTestData.txt");
            foreach(string s in tests)
            {
                string[] test = s.Split('\t');
                string[] input = test[0].Split(',');
                string[] result = test[1].Split(',');
                MapCoordinate MC1 = new MapCoordinate(double.Parse(input[0]), double.Parse(input[1]));
                MapCoordinate MC2 = new MapCoordinate(double.Parse(input[2]), double.Parse(input[3]));
                double distance = Helpers.RoundToSignificantDigits(MC1.DistanceTo(MC2), 4);
                Assert.AreEqual(distance, double.Parse(result[0]), String.Format("Expected for '{0}': {1}; Actual: {2}", Helpers.StringFromArray(input), result[0],distance));
            }
        }

    }
}

