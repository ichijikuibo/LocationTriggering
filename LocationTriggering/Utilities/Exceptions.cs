using System;
using System.Collections.Generic;
using System.Text;

namespace LocationTriggering.Utilities
{
    /// <summary>
    /// Exception that the thrown when an invlaid GPS location is created
    /// </summary>
    public class InvalidCoordinateException : System.Exception
    {
        public InvalidCoordinateException() : base() {}
        public InvalidCoordinateException(string message) : base(message) { }
    }
}
