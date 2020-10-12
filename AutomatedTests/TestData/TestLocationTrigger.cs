using LocationTriggering;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AutomatedTests.TestData
{
    class TestLocationTrigger:LocationTrigger
    {
        public string Result;
        public TestLocationTrigger(string kmlFile,string expectedResult):base(kmlFile)
        {
            Result = expectedResult;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("TestLocations/" + kmlFile);
            XmlNodeList coordinatesNode = xmlDoc.GetElementsByTagName("coordinates");
            string coordinates = coordinatesNode.Item(0).InnerText;

                coordinates = coordinates.TrimEnd('\n').TrimEnd('\r').TrimEnd('\n').Replace("\r", "\n").Replace("\n\n", "\n");
            
            string[] splitCoordinates = coordinates.Split('\n');
            List<MapCoordinate> newPoints = new List<MapCoordinate>();
            foreach (string s in splitCoordinates)
            {
                string latLng = s;
                string[] splitCoordinate = latLng.Split(',');
                if (splitCoordinate.Length < 2) continue;
                MapCoordinate newCoordinate;
                    newCoordinate = new MapCoordinate(double.Parse(splitCoordinate[1]), double.Parse(splitCoordinate[0]));

                if (!Contains(newCoordinate)) newPoints.Add(newCoordinate);
            }
            AddRange(newPoints);
        }
    }
}
