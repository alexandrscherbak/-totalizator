using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TotalizatorMVC.BusinesModels;

namespace TotalizatorMVC.Predictors
{
    public class FuzzyPredictor : IPredictor
    {
        private List<MatchInfo> matchesInfo;
        public FuzzyPredictor(List<MatchInfo> _matchesInfo)
        {
            matchesInfo = _matchesInfo;
        }
        public double[] predict(MatchInfo matchInfo)
        {
            string[] matchesFuzzyInfo = new string[matchesInfo.Count];
            int i = 0;
            string matchFuzzyInfo = "";

            foreach (MatchInfo item in matchesInfo)
            {
                matchesFuzzyInfo[i++] = item.firstTeam.fuzzyInfo + item.secondTeam.fuzzyInfo;
                if (item.id == matchInfo.id)
                    matchFuzzyInfo = matchesFuzzyInfo[i - 1];
            }
            int[] maxSimilarMatchesIds = new int[] { 0, 0, 0 };
            int[] similarCounts = new int[] { 0, 0, 0 };

            int currentSimilar;
            int matchResultType;
            for (i = 0; i < matchesInfo.Count; i++)
            {
                if (matchesInfo[i].id == matchInfo.id || matchesInfo[i].realResult == 3)
                    continue;
                currentSimilar = 0;
                matchResultType = matchesInfo[i].realResult;
                for (int j = 0; j < matchFuzzyInfo.Length; j++)
                {
                    if (matchFuzzyInfo[j] == matchesFuzzyInfo[i][j])
                        currentSimilar++;
                    if (currentSimilar > similarCounts[matchResultType])
                    {
                        similarCounts[matchResultType] = currentSimilar;
                        maxSimilarMatchesIds[matchResultType] = i;
                    }
                }
            }
            double[] prediction = new double[] {
                similarCounts[0] / matchFuzzyInfo.Length,
                similarCounts[1] / matchFuzzyInfo.Length,
                similarCounts[2] / matchFuzzyInfo.Length,
            };

            return prediction;
        }
        public static string analizeTeamInfo(List<Parameter> teamParameters)
        {
            string result = "";
            for (int i = 0; i < teamParameters.Count; i++)
            {
                double value = teamParameters[i].value;
                int leftLimit = teamParameters[i].fuzzyLimit.leftLimit;
                int rigtLimit = teamParameters[i].fuzzyLimit.rightLimit;
                if (value < leftLimit)
                    result += "1";
                else if (value > rigtLimit)
                    result += "3";
                else
                    result += "2";
            }
            return result;
        }
    }
}