﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeuralNetowrksAndDeepLearning.Demo.Demoer
{
    class Program
    {
        const int BATCH_SIZE = 10;
        const int EPOCH_COUNT = 30;
        const string TRAINING_IMAGES_PATH = @"C:\Users\hadis\source\repos\NeuralNetowrksAndDeepLearning\Demo\Training Data\train-images.idx3-ubyte";
        const string TRAINING_LABELS_PATH = @"C:\Users\hadis\source\repos\NeuralNetowrksAndDeepLearning\Demo\Training Data\train-labels.idx1-ubyte";
        const string TEST_IMAGES_PATH = @"C:\Users\hadis\source\repos\NeuralNetowrksAndDeepLearning\Demo\Training Data\t10k-images.idx3-ubyte";
        const string TEST_LABELS_PATH = @"C:\Users\hadis\source\repos\NeuralNetowrksAndDeepLearning\Demo\Training Data\t10k-labels.idx1-ubyte";
        const int INPUT_COUNT = 784;
        const int HIDDEN_COUNT = 15;
        const int OUTPUT_COUNT = 10;
        const double LEARNING_RATE = 3.0;

        static void Main(string[] args)
        {
            var network = new NeuralNetwork(new List<int>() { INPUT_COUNT, HIDDEN_COUNT, OUTPUT_COUNT });

            var trainingData = FetchData(TRAINING_LABELS_PATH, TRAINING_IMAGES_PATH);
            var testData = FetchData(TEST_LABELS_PATH, TEST_IMAGES_PATH);

            Console.WriteLine($"Starting cost: { network.Cost(trainingData) }, Starting validation: { network.Validate(testData, (net, o) => ValidateSample(net, o)) } / { testData.Count() }");

            network.SGD(trainingData, EPOCH_COUNT, BATCH_SIZE, LEARNING_RATE, i =>
            {
                Console.WriteLine($"Finished epoch { i }. Cost: { network.Cost(trainingData) }, Validation: { network.Validate(testData, (net, o) => ValidateSample(net, o)) } / { testData.Count() }");
            }/*, i =>
            {
                Console.WriteLine($"Finished batch at { i }.");
            }*/);

            network.Save(@"C:\Users\hadis\source\repos\NeuralNetowrksAndDeepLearning\Demo\nets\sgd.mlp");
        }

        private static bool ValidateSample(double[] activations, double[] output)
        {
            double maxActivation = double.MinValue;
            int maxActivationIndex = -2, maxOutputIndex = -1;
            for (int i = 0; i < activations.Length; i++)
            {
                if (activations[i] > maxActivation) (maxActivation, maxActivationIndex) = (activations[i], i);
                if (output[i] == 1.0) maxOutputIndex = i;
            }
            return maxActivationIndex == maxOutputIndex;
        }

        private static ITrainingSample[] FetchData(string labelPath, string imagePath)
        {
            BinaryReader labels = new BinaryReader(new FileStream(labelPath, FileMode.Open));

            int magicLabel = ReadBigInt32(labels);
            int numberOfLabels = ReadBigInt32(labels);

            BinaryReader images = new BinaryReader(new FileStream(imagePath, FileMode.Open));

            int magicNumber = ReadBigInt32(images);
            int numberOfImages = ReadBigInt32(images);
            int width = ReadBigInt32(images);
            int height = ReadBigInt32(images);

            ITrainingSample[] ret = new ITrainingSample[numberOfImages];

            Console.WriteLine($"magicLabel: { magicLabel }");
            Console.WriteLine($"numberOfLabels: { numberOfLabels }");
            Console.WriteLine();
            Console.WriteLine($"magicLabel: { magicNumber }");
            Console.WriteLine($"numberOfImages: { numberOfImages }");
            Console.WriteLine($"width: { width }");
            Console.WriteLine($"height: { height }");
            Console.WriteLine();

            for (int i = 0; i < numberOfImages; i++)
                ret[i] = new MNISTSample(images.ReadBytes(width * height), labels.ReadByte());

            return ret;
        }

        private static int ReadBigInt32(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}