using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutomatedTests
{
    static class Helpers
    {
        public static string[] OpenFile(string path)
        {
            List<string> lines = new List<string>();
            StreamReader file = new StreamReader(File.OpenRead(path));
            bool reading = true;
            while(reading)
            {
                string s = file.ReadLine();
                if (s != null&& s!="") lines.Add(s);
                else reading = false;
            }
            return lines.ToArray();
        }
        public static string StringFromArray(string[] array)
        {
            string result = "";
            foreach(string s in array)
            {
                result += s + ",";
            }
            return result.TrimEnd(',');
        }
    }
}
