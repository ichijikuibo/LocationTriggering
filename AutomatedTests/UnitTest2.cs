using LocationTriggering;
using LocationTriggering.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AutomatedTests
{
    [TestClass]
    public class Requirement2Tests
    {
        [TestMethod]
        public void PointInPolygonTest()
        {
            Data.TestLocationTriggerData testData = new Data.TestLocationTriggerData();
            string[] tests = Helpers.OpenFile("TestData\\ContainsPointTestData.txt");
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
    }
}

