using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LocationTriggering
{
    public class BasicLocationTrigger:LocationTrigger
    {
        private string _title;
        private string _description;
        public BasicLocationTrigger(string id, IEnumerable<MapCoordinate> points) : base(id, points)
        {
            
            _title = "";
            _description = "";
        }
        public BasicLocationTrigger(string id, IEnumerable<MapCoordinate> points,string title,string description) :base(id,points)
        {
            _title = title;
            _description = description;
        }
        public BasicLocationTrigger(string id, string points , char latLngSplit=',',char pointSplit=' ',bool longitudeFirst=false) : base(id, points, latLngSplit, pointSplit, longitudeFirst)
        {

            _title = id;
            _description = points;
        }

        public string Description { get => _description; set => _description = value; }
        public string Title { get => _title; set => _title = value; }
    }
}
