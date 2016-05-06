using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.BusinesModels
{
    public class FuzzyLimit
    {
        public int leftLimit;
        public int rightLimit;
        public FuzzyLimit(int _leftLimit, int _rightLimit)
        {
            leftLimit = _leftLimit;
            rightLimit = _rightLimit;
        }
    }
}