using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TotalizatorMVC.BusinesModels;

namespace TotalizatorMVC.Predictors
{
    interface IPredictor
    {
        double[] predict(MatchInfo matchInfo);
    }
}
