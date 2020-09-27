using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    public class InvalidCoordinateException : System.Exception
    {
        public InvalidCoordinateException() : base() {}
        public InvalidCoordinateException(string message) : base(message) { }
    }
}
