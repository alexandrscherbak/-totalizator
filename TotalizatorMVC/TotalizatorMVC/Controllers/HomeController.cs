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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Results()
        {
            var matches = db.Matches.ToList();
            var teams = db.Teams.ToList();

            foreach (var match in matches)
            {
                match.FirstTeam = teams.Where(t => t.Id == match.FirstTeamId).ToList()[0];
                match.SecondTeam = teams.Where(t => t.Id == match.SecondTeamId).ToList()[0];
            }
            ViewBag.Matches = matches;

            return View();
        }

        //[Authorize (Roles = "Admin")]
        public ActionResult About()
        {
            ViewBag.ResultsEncoder = ResultsEncoder.results;
            ViewBag.Matches = getMatchesInfo();

            return View();
        }

        public ActionResult MakeRate(int  id)
        {
            var rates = ratesDb.Rates.ToList();
            Rate rate = new Rate();
            List<MatchInfo> matchesInfo = getMatchesInfo();
            rate.UserId = User.Identity.GetUserId();
            rate.MatchId = matchesInfo.Find(m => m.id == id).id;
            rate.Result = 1;
            rate.Amount = 100.25m;
            rate.Coefficient = (3.25).ToString();
            ratesDb.Rates.Add(rate);
            ratesDb.SaveChanges();

            ViewBag.ResultsEncoder = ResultsEncoder.results;
            ViewBag.Matches = matchesInfo;
            return View("About");
        }

        private List<MatchInfo> getMatchesInfo()
        {
            var matches = db.Matches.ToList();
            var teams = db.Teams.ToList();
            List<Team> teamsToUpdate = new List<Team>();
            List<MatchInfo> matchesInfo = new List<MatchInfo>();
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

        public ActionResult Contact()
        {
            var outputLayerWeights = neuralNetworkDb.OutputLayerWeights.ToList();
            var teachedRBFs = neuralNetworkDb.TeachedRBFs.ToList();
            List<MatchInfo> matchesInfo = getMatchesInfo();
            GeneralPredictor generalPredictor = new GeneralPredictor(matchesInfo, outputLayerWeights, teachedRBFs);
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
            foreach (MatchInfo matchInfo in matchesInfo)
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
            ViewBag.Matches = matchesInfo;
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