using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TotalizatorMVC.Predictors;
using TotalizatorMVC.BusinesModels;
using TotalizatorMVC.Models;

namespace TotalizatorUnitTests
{
    [TestClass]
    public class QualimetricPredictorTests
    {
        Team strongTeam;
        Team weakTeam;
        Match match;
        TeamInfo strongTeamInfo;
        TeamInfo weakTeamInfo;
        QualimetricPredictor qualimetricPredictor;

        public QualimetricPredictorTests()
        {
            strongTeam = new Team()
            {
                Id = 1, GoalGames = 5, Goals = 15,
                DaysOfRest = 14, HomeMatch = 1,
                InjuredPlayers = 0, MissingBalls = 0,
                Points = 15, Position = 1, ZeroGames = 5
            };
            weakTeam = new Team()
            {
                Id = 2, GoalGames = 0, Goals = 0,
                DaysOfRest = 1, HomeMatch = 0,
                InjuredPlayers = 5, MissingBalls = 15,
                Points = 0, Position = 15, ZeroGames = 0
            };
            match = new Match() { Id = 1, Result = "3" };
            strongTeamInfo = new TeamInfo(strongTeam);
            weakTeamInfo = new TeamInfo(weakTeam);
            qualimetricPredictor = new QualimetricPredictor();
        }
        [TestMethod]
        public void ShouldPredictVictoryIfFirstTeamIsStronger()
        {
            MatchInfo matchInfo = new MatchInfo(match, strongTeamInfo,
                weakTeamInfo);
            double [] prediction = qualimetricPredictor.predict(matchInfo);
            Assert.AreEqual(true, (prediction[1] > prediction[2]));
        }
        [TestMethod]
        public void PredictorShouldPredictDrawIfTeamsAreEqual()
        {
            MatchInfo matchInfo = new MatchInfo(match, strongTeamInfo,
                strongTeamInfo);
            double[] prediction = qualimetricPredictor.predict(matchInfo);
            Assert.AreEqual(true, (prediction[1] == prediction[2]));
        }
        [TestMethod]
        public void ShouldPredictDefeatIfSecondTeamIsStronger()
        {
            MatchInfo matchInfo = new MatchInfo(match, weakTeamInfo,
                strongTeamInfo);
            double[] prediction = qualimetricPredictor.predict(matchInfo);
            Assert.AreEqual(true, (prediction[1] < prediction[2]));
        }
    }
}
