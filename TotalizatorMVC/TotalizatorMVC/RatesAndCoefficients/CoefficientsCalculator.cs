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
            for (int i = 0; i < 3; i++)
            {
                if (prediction[i] == prediction.Max())
                    prediction[i] += 0.17;
                else if (prediction[i] == prediction.Min())
                    prediction[i] -= 0.07;
                else
                    prediction[i] += 0.05;
                adjustedCoefficients[i] = 1 / prediction[i];
            }
            return adjustedCoefficients;
        }
    }
}