using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.Models
{
    public class OutputLayerWeights
    {
        public int Id { get; set; }
        public string OutputLayerWeight { get; set; }
    }
    public class TeachedRBF
    {
        public int Id { get; set; }
        public double StandartDeviation { get; set; }
        public string Expectation { get; set; }
    }
}