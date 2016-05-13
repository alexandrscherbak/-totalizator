using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.Predictors
{
    public static class Serializer
    {
        public static string SerializeToString(double[] array)
        {
            string result = "";
            foreach (double item in array)
            {
                if (result.Length > 0)
                    result += '|';
                result += item.ToString();
            }
            return result;
        }
        public static double[] DeserializeFromString(string serializedArray)
        {
            string[] deserializedArray = serializedArray.Split('|');
            double[] result = new double[deserializedArray.Length];
            int i = 0;
            foreach (string item in deserializedArray)
            {
                result[i++] = double.Parse(item, CultureInfo.InvariantCulture);
            }
            return result;
        }
    }
}