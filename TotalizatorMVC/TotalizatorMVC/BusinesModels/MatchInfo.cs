using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TotalizatorMVC.Models;
using TotalizatorMVC.Predictors;

namespace TotalizatorMVC.BusinesModels
{
    public class MatchInfo
    {
        public int id;
        public TeamInfo firstTeam;
        public TeamInfo secondTeam;
        public int realResult;

        public MatchInfo(Match matchInfo, TeamInfo _firstTeam, TeamInfo _secondTeam)
        {
            id = matchInfo.Id;
            int.TryParse(matchInfo.Result, out realResult);
            firstTeam = _firstTeam;
            secondTeam = _secondTeam;
        }
        public double[] getMatchParams()
        {
            double[] matchParams = new double[18];
            List<Parameter> firstTeamParameters = firstTeam.getListOfParameters();
            List<Parameter> secondTeamParameters = secondTeam.getListOfParameters();
            int i = 0;
            foreach (Parameter param in firstTeamParameters)
            {
                matchParams[i++] = param.value;
            }
            foreach (Parameter param in secondTeamParameters)
            {
                matchParams[i++] = param.value;
            }
            return matchParams;
        }
    }
}