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
    public class PredictionController : Controller
    {
        NeuralNetworkContext neuralNetworkDb = new NeuralNetworkContext();
        public ActionResult FutureEvents()
        {
            var outputLayerWeights = neuralNetworkDb.OutputLayerWeights.ToList();
            var teachedRBFs = neuralNetworkDb.TeachedRBFs.ToList();
            List<MatchInfo> _matchesInfo = GamesController.getMatchesInfo().Where(m => m.realResult == 3).ToList();
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

        public ActionResult TeachNN()
        {
            NeuralNetworkPredictor nn = new NeuralNetworkPredictor(GamesController.getMatchesInfo(),
                new List<OutputLayerWeights>(),
                new List<TeachedRBF>(),
                true);
            List<OutputLayerWeights> outputLayerWeightsForSaving = new List<OutputLayerWeights>();
            List<TeachedRBF> teachedRBFsForSaving = new List<TeachedRBF>();
            NeuralNetworkPredictor.prepareDataForSaving(outputLayerWeightsForSaving, teachedRBFsForSaving);
            saveNeuralNetwokToDataBase(outputLayerWeightsForSaving, teachedRBFsForSaving);
            return PartialView("TeachNN");
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