using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TotalizatorMVC.Models;
using TotalizatorMVC.BusinesModels;
using TotalizatorMVC.Predictors;
using System.Data;
using System.Data.Entity;
using TotalizatorMVC.RatesAndCoefficients;
using Microsoft.AspNet.Identity;

namespace TotalizatorMVC.Controllers
{
    public class HomeController : Controller
    {
        GamesContext db = new GamesContext();
        NeuralNetworkContext neuralNetworkDb = new NeuralNetworkContext();
        RatesContext ratesDb = new RatesContext();
        List<MatchInfo> matchesInfo = new List<MatchInfo>();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EditMatches()
        {
            var matches = db.Matches.ToList();
            var teams = db.Teams.ToList();
            foreach (var match in matches)
            {
                match.FirstTeam = teams.Where(t => t.Id == match.FirstTeamId).ToList()[0];
                match.SecondTeam = teams.Where(t => t.Id == match.SecondTeamId).ToList()[0];
            }
            return View(matches);
        }

        // CreateMatch
        public ActionResult CreateMatch()
        {
            List<Team> teams = db.Teams.ToList();
            bool isFirst = true;
            List<int> listOfFirstTeamIds = new List<int>();
            List<int> listOfSecondTeamIds = new List<int>();
            foreach (Team team in teams)
            {
                if (isFirst)
                    listOfFirstTeamIds.Add(team.Id);
                else
                    listOfSecondTeamIds.Add(team.Id);
                isFirst = !isFirst;
            }
            SelectList _listOfFirstTeamIds = new SelectList(listOfFirstTeamIds);
            SelectList _listOfSecondTeamIds = new SelectList(listOfSecondTeamIds);
            ViewBag.ListOfFirstTeamIds = _listOfFirstTeamIds;
            ViewBag.ListOfSecondTeamIds = _listOfSecondTeamIds;
            ViewBag.FirstTeamsId = listOfFirstTeamIds[0];
            ViewBag.SecondTeamsId = listOfSecondTeamIds[0];
            return PartialView("CreateMatch");
        }
        [HttpPost]
        public ActionResult CreateMatch(Match match)
        {
            db.Matches.Add(match);
            db.SaveChanges();
            return RedirectToAction("EditMatches");
        }

        //EditMatch
        public ActionResult EditMatch(int id)
        {
            Match match = db.Matches.Find(id);
            if (match != null)
            {
                return PartialView("EditMatch", match);
            }
            return RedirectToAction("EditMatches");
        }
        [HttpPost]
        public ActionResult EditMatch(Match match)
        {
            db.Entry(match).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("EditMatches");
        }

        public ActionResult Results()
        {
            ViewBag.ResultsEncoder = ResultsEncoder.results;
            ViewBag.Matches = getMatchesInfo().Where(m => m.realResult != 3).ToList();

            return View();
        }

        [HttpGet]
        public ActionResult MakeRate(int matchId, int result, double coefficient)
        {
            Rate rate = new Rate();
            rate.MatchId = matchId;
            rate.Result = result;
            rate.Amount = 0m;
            rate.Coefficient = coefficient.ToString();

            return PartialView("MakeRate", rate);
        }

        [HttpPost]
        public ActionResult MakeRate(Rate rate)
        {
            rate.UserId = User.Identity.GetUserId();
            rate.Coefficient = double.Parse(rate.Coefficient).ToString("0.00");
            ratesDb.Rates.Add(rate);
            ratesDb.SaveChanges();

            return RedirectToAction("FutureEvents");
        }

        [HttpGet]
        public ActionResult RatesInfo()
        {
            string userId = User.Identity.GetUserId();
            List<Rate> rates = ratesDb.Rates.Where(r => String.Equals(r.UserId, userId)).ToList();
            List<MatchInfo> _matchesInfo = getMatchesInfo();
            List<MatchInfo> _ratedMatchesInfo = new List<MatchInfo>();
            foreach (Rate rate in rates)
            {
                _ratedMatchesInfo.Add(_matchesInfo.Find(m => m.id == rate.MatchId));
            }

            ViewBag.Rates = rates;
            ViewBag.Matches = _ratedMatchesInfo;
            ViewBag.i = 0;
            ViewBag.ResultsEncoder = ResultsEncoder.results;
            return View();
        }

        private List<MatchInfo> getMatchesInfo()
        {
            if (matchesInfo.Count == 0)
            {
                var matches = db.Matches.ToList();
                var teams = db.Teams.ToList();
                List<Team> teamsToUpdate = new List<Team>();
                List<TeamInfo> teamesInfo = new List<TeamInfo>();

                foreach (var match in matches)
                {
                    Team firstTeamFromDb = teams.Where(t => t.Id == match.FirstTeamId).ToList()[0];
                    Team secondTeamFromDb = teams.Where(t => t.Id == match.SecondTeamId).ToList()[0];
                    match.FirstTeam = firstTeamFromDb;
                    match.SecondTeam = secondTeamFromDb;
                    TeamInfo firstTeam = new TeamInfo(match.FirstTeam);
                    TeamInfo secondTeam = new TeamInfo(match.SecondTeam);
                    if (firstTeam.fuzzyInfo == "-")
                    {
                        firstTeam.fuzzyInfo = FuzzyPredictor.analizeTeamInfo(firstTeam.getListOfParameters());
                        firstTeamFromDb.FuzzyInfo = firstTeam.fuzzyInfo;
                        teamsToUpdate.Add(firstTeamFromDb);
                    }
                    if (secondTeam.fuzzyInfo == "-")
                    {
                        secondTeam.fuzzyInfo = FuzzyPredictor.analizeTeamInfo(secondTeam.getListOfParameters());
                        secondTeamFromDb.FuzzyInfo = secondTeam.fuzzyInfo;
                        teamsToUpdate.Add(secondTeamFromDb);
                    }
                    MatchInfo matchInfo = new MatchInfo(match, firstTeam, secondTeam);
                    matchesInfo.Add(matchInfo);
                }
                if (teamsToUpdate.Count > 0)
                    updateTeamsTable<Team>(teamsToUpdate);
            }
            return matchesInfo;
        }

        private void updateTeamsTable<T>(List<T> itemsToUpdate) where T : class
        {
            foreach (T item in itemsToUpdate)
            {
                db.Entry(item).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public ActionResult FutureEvents()
        {
            var outputLayerWeights = neuralNetworkDb.OutputLayerWeights.ToList();
            var teachedRBFs = neuralNetworkDb.TeachedRBFs.ToList();
            List<MatchInfo> _matchesInfo = getMatchesInfo().Where(m => m.realResult == 3).ToList();
            GeneralPredictor generalPredictor = new GeneralPredictor(_matchesInfo, outputLayerWeights, teachedRBFs);
            if (NeuralNetworkPredictor.isNetworkChanged())
            {
                List<OutputLayerWeights> outputLayerWeightsForSaving = new List<OutputLayerWeights>();
                List<TeachedRBF> teachedRBFsForSaving = new List<TeachedRBF>();
                NeuralNetworkPredictor.prepareDataForSaving(outputLayerWeightsForSaving, teachedRBFsForSaving);
                saveNeuralNetwokToDataBase(outputLayerWeightsForSaving, teachedRBFsForSaving);
            }

            List<double[]> predictions = new List<double[]>();
            List<double[]> realCoefficients = new List<double[]>();
            List<double[]> adjustedCoefficients = new List<double[]>();
            double[] empty = new double[] { 0, 0, 0 };
            foreach (MatchInfo matchInfo in _matchesInfo)
            {
                if (matchInfo.realResult != 3)
                {
                    predictions.Add(empty);
                    realCoefficients.Add(empty);
                    adjustedCoefficients.Add(empty);
                }
                else
                {
                    predictions.Add(generalPredictor.predict(matchInfo));
                    realCoefficients.Add(CoefficientsCalculator.calculateRealCoefficients(generalPredictor.predict(matchInfo)));
                    adjustedCoefficients.Add(CoefficientsCalculator.calculateAdjustedCoefficients(generalPredictor.predict(matchInfo)));
                }
            }

            ViewBag.Message = "Общее предсказание";
            ViewBag.Matches = _matchesInfo;
            ViewBag.Predictions = predictions;
            ViewBag.RealCoefficients = realCoefficients;
            ViewBag.AdjustedCoefficients = adjustedCoefficients;
            ViewBag.i = 0;
            ViewBag.ResultsEncoder = ResultsEncoder.results;

            return View();
        }

        private void saveNeuralNetwokToDataBase(List<OutputLayerWeights> outputLayerWeights, List<TeachedRBF> teachedRBFs)
        {
            for (int i = 0; i < outputLayerWeights.Count; i++)
            {
                neuralNetworkDb.TeachedRBFs.Add(teachedRBFs[i]);
                neuralNetworkDb.OutputLayerWeights.Add(outputLayerWeights[i]);
            }
            neuralNetworkDb.SaveChanges();
        }
    }
}