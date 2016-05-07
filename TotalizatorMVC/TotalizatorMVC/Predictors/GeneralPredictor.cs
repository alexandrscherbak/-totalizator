using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TotalizatorMVC.BusinesModels;
using TotalizatorMVC.Models;

namespace TotalizatorMVC.Predictors
{
    public class GeneralPredictor : IPredictor
    {
        private QualimetricPredictor qualimetricPredictor;
        private FuzzyPredictor fuzzyPredictor;
        private NeuralNetworkPredictor neuralNetworkPredictor;
        public GeneralPredictor(List<MatchInfo> matchesInfo, List<OutputLayerWeights> outputLayerWeights, List<TeachedRBF> teachedRBFs)
        {
            qualimetricPredictor = new QualimetricPredictor();
            fuzzyPredictor = new FuzzyPredictor(matchesInfo);
            neuralNetworkPredictor = new NeuralNetworkPredictor(matchesInfo, outputLayerWeights, teachedRBFs, false);
        }
        public double[] predict(MatchInfo matchInfo)
        {
            double[] qualimetricPrediction = qualimetricPredictor.predict(matchInfo);
            double[] fuzzyPrediction = fuzzyPredictor.predict(matchInfo);
            double[] neuralNetworkPrediction = neuralNetworkPredictor.predict(matchInfo);
            double[] finalPrediction = new double[3];
            for (int i = 0; i < 3; i++)
            {
                finalPrediction[i] = neuralNetworkPrediction[i];
                finalPrediction[i] += fuzzyPrediction[i] / 3;
                finalPrediction[i] += qualimetricPrediction[i] * 0.3;
                if (finalPrediction[i] < 0.15)
                    finalPrediction[i] = 0.15 + i / 100;
            }
            double sum = finalPrediction.Sum();
            for (int i = 0; i < 3; i++)
                finalPrediction[i] /= sum;
            return finalPrediction;
        }
    }
}