using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MathNet.Numerics.LinearAlgebra;

using Random = UnityEngine.Random;
using System.IO;

public class NNet
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 5);

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;

    public void Initialize(int hiddenLayerCount, int neuronsCount)
    {
        inputLayer.Clear();
        outputLayer.Clear();
        hiddenLayers.Clear();
        weights.Clear();
        biases.Clear();

        for (int i = 0; i < hiddenLayerCount; i++)
        {
            Matrix<float> temp = Matrix<float>.Build.Dense(1, neuronsCount);
            hiddenLayers.Add(temp);
            biases.Add(Random.Range(-1f, 1f));

            if (i == 0)
            {
                temp = Matrix<float>.Build.Dense(5, neuronsCount);
                weights.Add(temp);
            }
            else
            {
                temp = Matrix<float>.Build.Dense(neuronsCount, neuronsCount);
                weights.Add(temp);
            }
        }

        weights.Add(Matrix<float>.Build.Dense(neuronsCount, 2));
        biases.Add(Random.Range(-1f, 1f));

        InitializeWeights();
    }

    void InitializeWeights()
    {
        for(int x = 0; x < weights.Count; x++)
        {
            for (int i = 0; i < weights[x].RowCount; i++)
            {
                for(int j = 0; j < weights[x].ColumnCount; j++)
                {
                    weights[x][i, j] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public NNet GetCopy(int layers, int neurons)
    {
        NNet copy = new NNet();
        List<Matrix<float>> copied_weight = new List<Matrix<float>>();

        for (int x = 0; x < weights.Count; x++)
        {
            Matrix<float> aux = Matrix<float>.Build.Dense(weights[x].RowCount, weights[x].ColumnCount);

            for (int i = 0; i < aux.RowCount; i++)
            {
                for(int j = 0; j < aux.ColumnCount; j++)
                {
                    aux[i, j] = weights[x][i, j];
                }
            }
            copied_weight.Add(aux);
        }

        List<float> copied_biases = new List<float>();

        for (int i = 0; i < biases.Count; i++)
        {
            copied_biases.Add(biases[i]);
        }

        copy.weights = copied_weight;
        copy.biases = copied_biases;
        copy.fitness = 0;

        copy.InitializeHidden(layers, neurons);
        return copy;
    }

    void InitializeHidden(int layers, int neurons)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for(int i = 0; i < layers; i++)
        {
            hiddenLayers.Add(Matrix<float>.Build.Dense(1, neurons));
        }
    }

    public (float, float) RunNetwork(float a, float b, float c, float d, float e)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;
        inputLayer[0, 3] = d;
        inputLayer[0, 4] = e;
        inputLayer = inputLayer.PointwiseTanh();

        hiddenLayers[0] = (inputLayer * weights[0] + biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = (hiddenLayers[i - 1] * weights[i] + biases[i]).PointwiseTanh();
        }

        outputLayer = (hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1] 
                       + biases[biases.Count - 1]).PointwiseTanh();


        return (Sigmoid(outputLayer[0, 0]), outputLayer[0, 1]);
    }

    private void Save(NNet indv, float time = 0f)
    {
        Debug.Log("saving");
        List<string> indv_to_save = new List<string>();
        string aux;

        indv_to_save.Add("Time: " + time);
        indv_to_save.Add("");
        indv_to_save.Add("");


        indv_to_save.Add("Weights: ");
        for (int x = 0; x < indv.weights.Count; x++)
        {
            aux = "";
            for (int i = 0; i < indv.weights[x].RowCount; i++)
            {
                for (int j = 0; j < indv.weights[x].ColumnCount; j++)
                {
                    aux += indv.weights[x][i, j];
                    aux += " ";
                }
                aux += "\n";
            }
            indv_to_save.Add(aux);
            indv_to_save.Add("");
        }

        aux = "";
        indv_to_save.Add("Biases: ");
        for (int i = 0; i < indv.biases.Count; i++)
        {
            aux += indv.biases[i];
            aux += " ";
        }
        indv_to_save.Add(aux);
        indv_to_save.Add("");
        indv_to_save.Add("");

        string path = @"E:\Unity\Projects\MiniRace\Assets\Scripts\Data\debug.txt";

        using (StreamWriter sw = File.AppendText(path))
        {
            foreach (var line in indv_to_save)
                sw.WriteLine(line);
        }
    }

    float Sigmoid(float value)
    {
        return 1 / (1 + Mathf.Exp(-value));
    }
}
