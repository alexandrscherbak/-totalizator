using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TotalizatorMVC.Models;
using TotalizatorMVC.BusinesModels;

namespace TotalizatorMVC.Predictors
{
    public class NeuralNetworkPredictor : IPredictor
    {
        private static RadialBasisFunctionNetwork rbfNetwork;
        private static bool _isNetworkChanged;
        public NeuralNetworkPredictor(List<MatchInfo> matchesInfo, List<OutputLayerWeights> outputLayerWeights, List<TeachedRBF> teachedRBFs)
        {
            _isNetworkChanged = false;
            initializeNeuralNetwork(matchesInfo, outputLayerWeights, teachedRBFs);
        }
        private void initializeNeuralNetwork(List<MatchInfo> matchesInfo, List<OutputLayerWeights> outputLayerWeights, List<TeachedRBF> teachedRBFs)
        {
            if (outputLayerWeights.Count > 0 && outputLayerWeights.Count == teachedRBFs.Count)
            {
                rbfNetwork = new RadialBasisFunctionNetwork(matchesInfo[0].firstTeam.getListOfParameters().Count * 2,
                    3, teachedRBFs.Count, teachedRBFs, outputLayerWeights,
                    new char[] { '0', '1', '2' });
            }
            else
            {
                Dictionary<char, List<double[]>> classes = new Dictionary<char, List<double[]>>();
                List<double[]> zeroMatches = new List<double[]>();
                List<double[]> vinMatches = new List<double[]>();
                List<double[]> looseMatches = new List<double[]>();
                foreach (MatchInfo matchInfo in matchesInfo)
                {
                    double[] matchParams = matchInfo.getMatchParams();
                    if (matchInfo.realResult == 0)
                        zeroMatches.Add(matchParams);
                    else if (matchInfo.realResult == 1)
                        vinMatches.Add(matchParams);
                    else
                        looseMatches.Add(matchParams);
                }
                classes['0'] = zeroMatches;
                classes['1'] = vinMatches;
                classes['2'] = looseMatches;

                while (true)
                {
                    rbfNetwork = new RadialBasisFunctionNetwork(classes, 1, 0.1);
                    if (rbfNetwork.iterationCount != 1000000)
                        break;
                }
                _isNetworkChanged = true;
            }
        }
        public static void prepareDataForSaving(List<OutputLayerWeights> outputLayerWeights, List<TeachedRBF> teachedRBFs)
        {
            rbfNetwork.prepareDataForSaving(outputLayerWeights, teachedRBFs);
        }
        public double[] predict(MatchInfo matchInfo)
        {
            Dictionary<char, double> prediction = rbfNetwork.ClassifyMatch(matchInfo.getMatchParams());
            double[] convertedPrediction = new double[] {
                prediction['0'],
                prediction['1'],
                prediction['2'],
            };
            return convertedPrediction;
        }
        public static bool isNetworkChanged()
        {
            return _isNetworkChanged;
        }
    }

    public class RadialBasisFunctionNetwork
    {
        private RBF_teachInfo[] rbf_teached;
        private double[,] outputLayer_weights;
        private int inputCount;
        private int outputCount;
        private int rbfCount;
        private double alfa;
        public double maxErrorTreshold { get; private set; }
        public int iterationCount { get; private set; }
        private char[] classesNames;

        public RadialBasisFunctionNetwork(int _inputCount, int _outputCount, int _rbfCount, List<TeachedRBF> _teachedRBFs, List<OutputLayerWeights> _outputLayerWeights, char[] _classesNames)
        {
            inputCount = _inputCount;
            outputCount = _outputCount;
            rbfCount = _rbfCount;
            rbf_teached = new RBF_teachInfo[rbfCount];
            outputLayer_weights = new double[rbfCount, outputCount];
            int i = 0;
            foreach (TeachedRBF teachedRBF in _teachedRBFs)
            {
                RBF_teachInfo rbf = new RBF_teachInfo();
                rbf.standardDeviation = teachedRBF.StandartDeviation;
                rbf.expectations = Serializer.DeserializeFromString(teachedRBF.Expectation);
                rbf_teached[i++] = rbf;
            }
            i = 0;
            outputLayer_weights = new double[rbfCount, outputCount];
            for (i = 0; i < rbfCount; i++)
            {
                for (int j = 0; j < outputCount; j++)
                {
                    outputLayer_weights[i, j] = Serializer.DeserializeFromString(_outputLayerWeights[i].OutputLayerWeight)[j];
                }
            }
            classesNames = _classesNames;
            iterationCount = 0;
        }

        public RadialBasisFunctionNetwork(Dictionary<char, List<double[]>> classes, double alfa, double maxError)
        {
            int size = classes.First().Value.First().Length;

            int i = 0;
            classesNames = new char[classes.Count];
            foreach (KeyValuePair<char, List<double[]>> tmp in classes)
                classesNames[i++] = tmp.Key;

            this.alfa = alfa;
            this.maxErrorTreshold = maxError;

            iterationCount = TeachNeuralNetwork(classes);
        }

        private int TeachNeuralNetwork(Dictionary<char, List<double[]>> classes)
        {
            inputCount = classes.First().Value.First().Length;
            outputCount = classes.Count;
            rbfCount = outputCount;

            TeachRBFNeurons(classes);

            double[][][] rbfNeurons = CalculateRBFNeuronsForTrainingSet(classes);

            outputLayer_weights = new double[rbfCount, outputCount];
            Randomize();
            double[] outputNeurons = new double[outputCount];
            List<double> maxErrors = new List<double>();
            int iterCount = 0;
            double errorSumPrev = double.MaxValue;
            double[] outputErrors = new double[outputCount];
            double[] outputErrorsAbs = new double[outputCount];

            double mu = 1;
            while (true)
            {
                maxErrors.Clear();
                int classNumber = 0;

                foreach (List<double[]> matches in classes.Values)
                {
                    int matchNumber = 0;
                    foreach (double[] match in matches)
                    {

                        for (int k = 0; k < outputCount; k++)
                        {
                            double sum = 0;
                            for (int j = 0; j < rbfCount; j++)
                                sum += outputLayer_weights[j, k] * rbfNeurons[classNumber][matchNumber][j];

                            outputNeurons[k] = sum;
                        }
                        Array.Clear(outputErrors, 0, outputErrors.Length);
                        outputErrors[classNumber] = 1;
                        for (int k = 0; k < outputCount; k++)
                            outputErrors[k] -= outputNeurons[k];
                        outputErrorsAbs = outputErrors.Select(x => Math.Abs(x)).ToArray();
                        maxErrors.Add(outputErrorsAbs.Max());
                        for (int j = 0; j < rbfCount; j++)
                        {
                            for (int k = 0; k < outputCount; k++)
                            {
                                outputLayer_weights[j, k] = mu * outputLayer_weights[j, k] +
                                    alfa * outputErrors[k] * rbfNeurons[classNumber][matchNumber][j];
                            }
                        }
                        matchNumber++;
                    }
                    classNumber++;
                }
                iterCount++;
                if (iterCount == 1000000)
                    break;

                double maxError = maxErrors.Max();
                if (maxError < maxErrorTreshold)
                    break;
                double errorSum = outputErrorsAbs.Sum();
                if (errorSum >= errorSumPrev)
                {
                    Randomize();
                    errorSumPrev = double.MaxValue;
                }
                else
                    errorSumPrev = errorSum;
            }
            return iterCount;
        }

        private void TeachRBFNeurons(Dictionary<char, List<double[]>> classes)
        {
            List<RBF_teachInfo> RBFs = new List<RBF_teachInfo>();

            foreach (List<double[]> matchClass in classes.Values)
            {
                foreach (double[] match in matchClass)
                {
                    RBF_teachInfo rbf = new RBF_teachInfo();
                    rbf.expectations = match;
                    RBFs.Add(rbf);
                }
            }
            rbfCount = RBFs.Count;
            rbf_teached = RBFs.ToArray();
            if (rbfCount == 1)
            {
                double sko = 0;
                for (int k = 0; k < inputCount; k++)
                    sko += Math.Pow((rbf_teached[0].expectations[k]), 2);
                rbf_teached[0].standardDeviation = Math.Sqrt(sko);
                return;
            }

            List<double>[] deviations = new List<double>[rbfCount];
            for (int i = 0; i < deviations.Length; i++)
                deviations[i] = new List<double>();
            for (int i = 0; i < rbfCount; i++)
            {
                for (int j = i + 1; j < rbfCount; j++)
                {
                    double sko = 0;
                    for (int k = 0; k < inputCount; k++)
                        sko += Math.Pow((rbf_teached[i].expectations[k] - rbf_teached[j].expectations[k]), 2);
                    sko = Math.Sqrt(sko);

                    deviations[i].Add(sko);
                    deviations[j].Add(sko);
                }
            }
            for (int i = 0; i < rbfCount; i++)
                rbf_teached[i].standardDeviation = deviations[i].Min();
        }

        private double[][][] CalculateRBFNeuronsForTrainingSet(Dictionary<char, List<double[]>> classes)
        {
            double[][][] result = new double[classes.Count][][];

            int classCount = 0;
            foreach (List<double[]> matchClass in classes.Values)
            {
                result[classCount] = new double[matchClass.Count][];

                int matchCount = 0;
                foreach (double[] match in matchClass)
                {
                    result[classCount][matchCount] = new double[rbfCount];

                    for (int j = 0; j < rbfCount; j++)
                    {
                        double sum = 0;
                        for (int i = 0; i < inputCount; i++)
                            sum += (match[i] - rbf_teached[j].expectations[i]) * (match[i] - rbf_teached[j].expectations[i]);

                        result[classCount][matchCount][j] = Math.Exp(-sum / Math.Pow(rbf_teached[j].standardDeviation, 2));
                    }
                    matchCount++;
                }
                classCount++;
            }
            return result;
        }

        private void Randomize()
        {
            Random random = new Random(RandomProvider.Next() ^ Environment.TickCount);

            for (int i = 0; i < outputLayer_weights.GetLength(0); i++)
            {
                for (int j = 0; j < outputLayer_weights.GetLength(1); j++)
                    outputLayer_weights[i, j] = 2 * random.NextDouble() - 1;
            }
        }

        public Dictionary<char, double> ClassifyMatch(double[] match)
        {
            if (inputCount != match.Length)
                throw new Exception("Match has wrong size.");

            double[] rbfNeurons = new double[rbfCount];
            double[] outputNeurons = new double[outputCount];

            for (int j = 0; j < rbfCount; j++)
            {
                double sum = 0;
                for (int i = 0; i < inputCount; i++)

                    sum += (match[i] - rbf_teached[j].expectations[i]) * (match[i] - rbf_teached[j].expectations[i]);

                rbfNeurons[j] = Math.Exp(-sum / Math.Pow(rbf_teached[j].standardDeviation, 2));
            }

            for (int k = 0; k < outputCount; k++)
            {
                double sum = 0;
                for (int j = 0; j < rbfCount; j++)
                    sum += outputLayer_weights[j, k] * rbfNeurons[j];
                if (sum < 0)
                    sum = 0;
                outputNeurons[k] = sum;
            }

            Dictionary<char, double> result = new Dictionary<char, double>();
            int t = 0;
            foreach (char className in classesNames)
            {
                result.Add(classesNames[t], outputNeurons[t]);
                t++;
            }

            return result;
        }

        public void prepareDataForSaving(List<OutputLayerWeights> outputLayerWeights, List<TeachedRBF> teachedRBFs)
        {
            int i = 1;
            foreach (var item in rbf_teached)
            {
                TeachedRBF teachedRBF = new TeachedRBF();
                teachedRBF.Id = i++;
                teachedRBF.StandartDeviation = item.standardDeviation;
                teachedRBF.Expectation = Serializer.SerializeToString(item.expectations);
                teachedRBFs.Add(teachedRBF);
            }
            for (i = 0; i < rbfCount; i++)
            {
                OutputLayerWeights outputLayerWeightsItem = new OutputLayerWeights();
                outputLayerWeightsItem.Id = i + 1;
                outputLayerWeightsItem.OutputLayerWeight = Serializer.SerializeToString(
                    new double[] { outputLayer_weights[i, 0],
                        outputLayer_weights[i, 1],
                        outputLayer_weights[i, 2] });
                outputLayerWeights.Add(outputLayerWeightsItem);
            }
        }
    }

    public struct RBF_teachInfo
    {
        public double[] expectations { get; set; }
        public double standardDeviation { get; set; }
    }

    public static class RandomProvider
    {
        private static readonly Random Rnd = new Random();
        private static object _sync = new object();
        public static int Next()
        {
            lock (_sync)
            {
                return Rnd.Next();
            }
        }
        public static int Next(int max)
        {
            lock (_sync)
            {
                return Rnd.Next(max);
            }
        }
        public static int Next(int min, int max)
        {
            lock (_sync)
            {
                return Rnd.Next(min, max);
            }
        }
    }
}