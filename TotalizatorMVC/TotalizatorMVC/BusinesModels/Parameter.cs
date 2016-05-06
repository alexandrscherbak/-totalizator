using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.BusinesModels
{
    public class Parameter
    {
        public bool isStimulator;
        public int weight;
        public double value;
        public FuzzyLimit fuzzyLimit;
        public Parameter(bool _isStimalutor, int _weigth, double _value)
        {
            isStimulator = _isStimalutor;
            weight = _weigth;
            value = _value;
            fuzzyLimit = new FuzzyLimit(-1, 0);
        }
        public Parameter(bool _isStimalutor, int _weigth, double _value, FuzzyLimit _fuzzyLimit):this(_isStimalutor, _weigth, _value)
        {
            fuzzyLimit = _fuzzyLimit;
        }
    }
}