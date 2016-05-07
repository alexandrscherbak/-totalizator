using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.RatesAndCoefficients
{
    public class CoefficientsCalculator
    {
        public static double[] calculateRealCoefficients(double[] prediction)
        {
            double[] realCoefficients = new double[3];
            for (int i = 0; i < 3; i++)
                realCoefficients[i] = 1 / prediction[i];
            return realCoefficients;
        }
        public static double[] calculateAdjustedCoefficients(double[] prediction)
        {
            double[] adjustedCoefficients = new double[3];
            Dictionary<string, int> indexes = new Dictionary<string, int>();
            double changes = 0.15;
            double additionalChanges = 0;
            int i = 0;
            for (i = 0; i < 3; i++)
            {
                if (prediction[i] == prediction.Max())
                    indexes["max"] = i;
                else if (prediction[i] == prediction.Min() && !indexes.ContainsKey("min"))
                    indexes["min"] = i;
                else
                    indexes["medium"] = i;
            }
            if ((prediction[indexes["min"]] - 0.125) > 0)
            {
                if ((prediction[indexes["min"]] - 0.125) > 0.07)
                    additionalChanges = 0.07;
                else
                    additionalChanges = prediction[indexes["min"]] - 0.125;
            }
            prediction[indexes["min"]] -= additionalChanges;
            prediction[indexes["max"]] += changes * 0.6818 + additionalChanges * 0.6818;
            prediction[indexes["medium"]] += changes * 0.3182 + additionalChanges * 0.3182;
            for (i = 0; i < 3; i++)
                adjustedCoefficients[i] = 1 / prediction[i];
            return adjustedCoefficients;
        }
    }
}