using LocationTriggering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Xamarin.Essentials;

namespace SampleAndTesting.Data
{
    public class TestLocationTriggerData
    {
        private List<LocationTrigger> _testData;
        Assembly resources;
        string resourcePrefix;
        public TestLocationTriggerData()
        {
            _testData = new List<LocationTrigger>();
            resourcePrefix = "SampleAndTesting.";
#if __IOS__
            resourcePrefix = "SampleAndTesting.IOS.";
#endif
#if __ANDROID__
            resourcePrefix = "SampleAndTesting.Droid.";
#endif

            resources = typeof(TestLocationTriggerData).GetTypeInfo().Assembly;

            BasicLocationTrigger _testLocation = (BasicLocationTrigger)OpenKMLFile("Belfast.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Cork.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Derry.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("DistrictOfColumbia.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Dublin.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Europe.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Galway.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Iceland.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Ireland.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Johannesburg.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Limerick.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("London.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Moscow.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("NewZealand.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Paris.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("SaoPaulo.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("SouthAmerica.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Tokyo.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Waterford.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Wellington.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Guildhall.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("TowerMuseum.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("craigavonbridge.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("HandsAcrossDivde.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("PeaceBridge.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("AppenticeBoysMemorialHall.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("FreeDerryCorner.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("RiverFoyle.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Bogside.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("Bogsidemurals.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("seigemuseum.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("St Augustines.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

            _testLocation = (BasicLocationTrigger)OpenKMLFile("StColumbsCathedral.kml");
            if (_testLocation != null) _testData.Add(_testLocation);
            _testLocation = (BasicLocationTrigger)OpenKMLFile("Hawaii.kml");
            if (_testLocation != null) _testData.Add(_testLocation);

        }
        public LocationTrigger OpenKMLFile(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            Stream stream = resources.GetManifestResourceStream(resourcePrefix + "TestLocations." + fileName);
            xmlDoc.Load(stream);
            XmlNodeList coordinatesNode = xmlDoc.GetElementsByTagName("coordinates");
            if (coordinatesNode.Count > 0)
            {
                return new BasicLocationTrigger(fileName, coordinatesNode.Item(0).InnerText.Replace("\n","").Replace("\t",""), ',', ' ',true);
            }
            else
            {
                return null;
            }
        }
        public LocationTrigger GetLocation(string id)
        {
            foreach(LocationTrigger LT in _testData)
            {
                if (id == LT.LocationID) return LT;
            }
            return null;
        }
        public List<LocationTrigger> TestData { get => _testData; }
    }
}
