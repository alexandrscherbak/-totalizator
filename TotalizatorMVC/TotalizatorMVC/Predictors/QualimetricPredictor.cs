using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TotalizatorMVC.BusinesModels;

namespace TotalizatorMVC.Predictors
{
    public class QualimetricPredictor : IPredictor
    {
        public double[] predict(MatchInfo matchInfo)
        {
            double[] teamPowers = TeamPowerCalculator.calculateTeamPowers(matchInfo.firstTeam.getListOfParameters(), matchInfo.secondTeam.getListOfParameters());
            double[] prediction = new double[] {
                0,
                teamPowers[0],
                teamPowers[1]
            };
            return prediction;
        }
    }

    public static class TeamPowerCalculator
    {
        public static double[] calculateTeamPowers(List<Parameter> firstTeamParameters, List<Parameter> secondTeamParameters)
        {
            double[] teamPowers = new double[2] { 0, 0 };
            double[] teamsRelativeParemeters = new double[2];
            for (int i = 0; i < firstTeamParameters.Count; i++)
            {
                teamsRelativeParemeters = calculateRelativeParameters(firstTeamParameters[i], secondTeamParameters[i]);
                teamPowers[0] += teamsRelativeParemeters[0] * firstTeamParameters[i].weight;
                teamPowers[1] += teamsRelativeParemeters[1] * secondTeamParameters[i].weight;
            }
            double teamPowersSum = teamPowers[0] + teamPowers[1];
            teamPowers[0] /= teamPowersSum;
            teamPowers[1] /= teamPowersSum;
            return teamPowers;
        }
        static double[] calculateRelativeParameters(Parameter firstTeamParemeter, Parameter secondTeamParameter)
        {
            double[] relativeParameters = new double[2];
            double sum = firstTeamParemeter.value + secondTeamParameter.value;
            if (sum == 0)
            {
                relativeParameters[0] = 0;
                relativeParameters[1] = 0;
            }
            else
            {
                double firstTeamRelativeParemeter = firstTeamParemeter.value / sum;
                if (!firstTeamParemeter.isStimulator)
                    firstTeamRelativeParemeter = 1 - firstTeamRelativeParemeter;
                double secondTeamRelativeParemeter = secondTeamParameter.value / sum;
                if (!secondTeamParameter.isStimulator)
                    secondTeamRelativeParemeter = 1 - firstTeamRelativeParemeter;
                relativeParameters[0] = firstTeamRelativeParemeter;
                relativeParameters[1] = secondTeamRelativeParemeter;
            }
            return relativeParameters;
        }
    }
}