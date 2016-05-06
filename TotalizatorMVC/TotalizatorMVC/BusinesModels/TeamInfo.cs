using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TotalizatorMVC.Models;

namespace TotalizatorMVC.BusinesModels
{
    public class TeamInfo
    {
        public string name;
        public Parameter position;
        public Parameter points;
        public Parameter goals;
        public Parameter missingBalls;
        public Parameter injuredPlayers;
        public Parameter daysOfRest;
        public Parameter zeroGames;
        public Parameter goalGames;
        public Parameter homeMatch;
        public string fuzzyInfo;

        public TeamInfo(Team teamInfo)
        {
            name = teamInfo.Name;
            position = new Parameter(false, 8, teamInfo.Position, new FuzzyLimit(5, 12));
            points = new Parameter(true, 9, teamInfo.Points, new FuzzyLimit(5, 10));
            goals = new Parameter(true, 7, teamInfo.Goals, new FuzzyLimit(5, 10));
            missingBalls = new Parameter(false, 7, teamInfo.MissingBalls, new FuzzyLimit(5, 10));
            injuredPlayers = new Parameter(false, 6, teamInfo.InjuredPlayers, new FuzzyLimit(2, 5));
            daysOfRest = new Parameter(true, 4, teamInfo.DaysOfRest, new FuzzyLimit(8, 13));
            zeroGames = new Parameter(true, 5, teamInfo.ZeroGames, new FuzzyLimit(1, 3));
            goalGames = new Parameter(true, 5, teamInfo.GoalGames, new FuzzyLimit(2, 4));
            homeMatch = new Parameter(true, 6, teamInfo.HomeMatch, new FuzzyLimit(1, 0));
            fuzzyInfo = teamInfo.FuzzyInfo;
        }
        public List<Parameter> getListOfParameters()
        {
            List<Parameter> listOfParameters = new List<Parameter>();
            listOfParameters.Add(position);
            listOfParameters.Add(points);
            listOfParameters.Add(goals);
            listOfParameters.Add(missingBalls);
            listOfParameters.Add(injuredPlayers);
            listOfParameters.Add(daysOfRest);
            listOfParameters.Add(zeroGames);
            listOfParameters.Add(goalGames);
            listOfParameters.Add(homeMatch);
            return listOfParameters;
        }
    }
}