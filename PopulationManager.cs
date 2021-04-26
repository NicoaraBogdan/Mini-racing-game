using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.UI;


public class PopulationManager : MonoBehaviour
{
    [Header("Evolution variables")]
    public float mutation_rate; 
    public float rc_to_mutate = 5;
    public float cross_points;
    public float best_selection;
    public float deviation = 0.2f;

    public int
        pop_size,
        layer_count,
        neurons_count;

    public Movment car;

    private List<NNet> population;

    [Header("View and scale modifiers")]
    public Slider sld_time_scale;
    public Slider POV;
    public Camera[] cameras;

    [Header("View population evolution")]
    [SerializeField]
    private int current_generation = 0;
    [SerializeField]
    private int current_pos_in_pop = 0;

    private void Awake()
    {
        CreatePopulation();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void CreatePopulation()
    {
        population = new List<NNet>();
        InitializePopulation(population, 0);
        ResetToCurrentGenom();
    }

    private void InitializePopulation(List<NNet> population, int starting_index)
    {
        for (int i = starting_index; i < pop_size; i++)
        {
            population.Add(new NNet());
            population[i].Initialize(layer_count, neurons_count);
        }
    }

    private void ResetToCurrentGenom()
    {
        car.ResetWithNetwork(population[current_pos_in_pop]);
    }

    public void Death(float fitness)
    {
        if(current_pos_in_pop < pop_size - 1)
        {
            population[current_pos_in_pop].fitness = fitness;
            current_pos_in_pop++;
            ResetToCurrentGenom();
        }
        else
        {
            Repopulate();
        }
    }

    void Repopulate()
    {
        List<NNet> new_pop = new List<NNet>();
        new_pop = SelectBest();

        population.Clear();

        for (int i = 0; i < new_pop.Count; i++)
        {
            population.Add(new_pop[i].GetCopy(layer_count, neurons_count));
        }

        current_generation++;
        current_pos_in_pop = 0;
    }

    private void PrintPopResults()
    {
        float sum = 0;
        for(int i = 0; i < pop_size; i++)
        {
            sum += population[i].fitness;
        }

        Debug.Log((current_generation + 1).ToString() + " AVG: " + (sum / pop_size).ToString());
    }

    List<NNet> SelectBest()
    {
        List<NNet> new_pop = new List<NNet>();
        Sort();
        //CHECKED

        PrintPopResults();
        //for (int i = 0; i < pop_size; i++)
        //{
        //    Debug.Log(population[i].fitness);
        //}
        //Debug.Break();

        for (int i = 0; i < pop_size / 2; i++)
        {
            new_pop.Add(population[i].GetCopy(layer_count, neurons_count));
        }
        //CHECKED

        List<NNet> offspings = new List<NNet>();
        offspings = Cross(new_pop);
        
        for(int i = 0; i < offspings.Count; i++)
        {
            new_pop.Add(offspings[i]);
        }

        for (int i = pop_size / 4; i < pop_size / 2; i++)
        {
            new_pop[i] = Mutate(new_pop[i]);
        }

        return new_pop;
    }

    private List<NNet> Cross(List<NNet> new_pop)
    {
        List<NNet> offsprings = new List<NNet>();

        for (int i = pop_size / 2; i < pop_size; i++)
        {
            int i_cross = Random.Range(0, pop_size / 2);
            int j_cross = Random.Range(0, pop_size / 2);

            NNet offspirng = new NNet();
            offspirng.Initialize(layer_count, neurons_count);

            for (int cross_point = 0; cross_point < cross_points - 1; cross_point+= 2)
            {
                //WEIGHTS CROSSING
                for (int j = cross_point; j < new_pop[i_cross].weights.Count / (cross_points - cross_point); j++)
                {
                    offspirng.weights[j] = new_pop[i_cross].weights[j];
                }

                for (int j = (int)(new_pop[i_cross].weights.Count / (cross_points - cross_point)); j < new_pop[i_cross].weights.Count / cross_points - cross_point - 1; j++)
                {
                    offspirng.weights[j] = new_pop[j_cross].weights[j];
                }

                //BIASES CROSSING
                for (int j = 0; j < new_pop[i_cross].biases.Count / 2; j++)
                {
                    offspirng.biases[j] = new_pop[i_cross].biases[j];
                }

                for (int j = new_pop[j_cross].biases.Count / 2; j < new_pop[j_cross].biases.Count; j++)
                {
                    offspirng.biases[j] = new_pop[j_cross].biases[j];
                }
            }
            offsprings.Add(Mutate(offspirng));
        }

        return offsprings;
    }

    NNet Mutate(NNet offspring)
    {
        if(Random.Range(0f, 1f) < mutation_rate)
        {
            for(int i = 0; i<offspring.weights.Count; i++)
            {
                offspring.weights[i] = MutateMatrix(offspring.weights[i]);
            }

            MutateBiases(offspring.biases);
        }
        return offspring;
    }

    Matrix<float> MutateMatrix(Matrix<float> f)
    {
        int no_random_points = (int)Random.Range(1f, f.RowCount * f.ColumnCount / rc_to_mutate);
        
        Matrix<float> randomized = f.Clone();

        for (int _ = 0; _ < no_random_points; _++)
        {
            int random_row = (int)Random.Range(0, randomized.RowCount);
            int random_column = (int)Random.Range(0, randomized.ColumnCount);


            //RANDOMIZE ROW
            for(int i = 0; i < randomized.ColumnCount; i++)
            {
                float value = randomized[random_row, i] + Random.Range(-deviation, deviation);
                randomized[random_row, i] = Mathf.Clamp(value, -1f, 1f);
            }


            //RANDOMIZE COLUMN
            for (int i = 0; i < randomized.RowCount; i++)
            {
                float value = randomized[i, random_column] + Random.Range(-deviation, deviation);
                randomized[i, random_column] = Mathf.Clamp(value, -1f, 1f);
            }
        }

        return randomized;
    }

    void MutateBiases(List<float> biases)
    {
        for (int i = 0; i< biases.Count; i++)
        {
            biases[i] = Mathf.Clamp(biases[i] + Random.Range(-deviation, deviation), -1f , 1f);
        }
    }

    void Sort()
    {
        for (int i = 0; i < pop_size; i++)
        {
            for (int j = i + 1; j < pop_size; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    var aux = population[i];
                    population[i] = population[j];
                    population[j] = aux;
                }
            }
        }
    }

    int Diffrences(NNet parent, NNet child)
    {
        int diff = 0;

        for(int x= 0; x < parent.weights.Count; x++)
        {
            for(int i = 0; i < parent.weights[x].RowCount; i++)
            {
                for (int j = 0; j < parent.weights[x].ColumnCount; j++)
                {
                    if (parent.weights[x][i, j] != child.weights[x][i, j])
                        diff++;
                }
            }
        }

        for (int x = 0; x < parent.biases.Count; x++)
        {
            if (parent.biases[x] != child.biases[x])
                diff++;
        }
        return diff;
    }

    void UpdateUI()
    {
        Time.timeScale = sld_time_scale.value;

        for(int i = 0; i < cameras.Length; i++)
        {
            if (POV.value == i)
            {
                cameras[i].gameObject.SetActive(true);
                Debug.Log(POV.value);
            }
            else
                cameras[i].gameObject.SetActive(false);
        }
    }
}
