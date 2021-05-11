using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class PopulationManager : MonoBehaviour
{
    [Header("Evolution variables")]
    public float mutation_rate;
    public float rc_to_mutate = 5;
    public float cross_points;
    public float deviation = 0.2f;

    public int
        pop_size,
        layer_count,
        neurons_count;

    public Movment car;

    private List<NNet> population = new List<NNet>();

    [Header("View and scale modifiers")]
    public Slider sld_time_scale;
    public Slider POV;
    public Camera[] cameras;

    [Header("View population evolution")]
    [SerializeField]
    public int current_generation = 0;
    [SerializeField]
    private int current_pos_in_pop = 0;
    [SerializeField]
    private int best_checkpoint = 0;
    [SerializeField]
    private float global_best_fitness = 0;

    private void Awake()
    {
        population.Add(ReadFile(@"E:\Unity\Projects\MiniRace\Assets\Scripts\Data\init.txt").GetCopy(layer_count, neurons_count));
        CreatePopulation();
    }
    private void Update()
    {
        UpdateUI();
    }

    private void CreatePopulation()
    {
        current_pos_in_pop = 0;
        InitializePopulation(population, 1);
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
        //Save(population[current_pos_in_pop], 111f);

        car.ResetWithNetwork(population[current_pos_in_pop]);
    }
    private void Save(NNet indv, float time = 0f)
    {
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

    public void Death(float fitness, int no_checkpoints)
    {
        if (current_pos_in_pop < pop_size - 1)
        {
            population[current_pos_in_pop].fitness = fitness;
            current_pos_in_pop++;
            ResetToCurrentGenom();
        }
        else
        {
            Repopulate();
        }

        if (no_checkpoints > best_checkpoint)
        {
            best_checkpoint = no_checkpoints;
        }
    }

    //OK
    void Repopulate()
    {
        List<NNet> new_pop = new List<NNet>();

        new_pop.Add(SelectBest());

        while (new_pop.Count < pop_size / 2)
        {
            new_pop.Add(Turnir());
        }

        new_pop = Cross(new_pop);

        new_pop = MutatePop(new_pop);

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
        for (int i = 0; i < pop_size; i++)
        {
            sum += population[i].fitness;
        }

        Debug.Log((current_generation + 1).ToString() + " AVG: " + (sum / pop_size).ToString());
    }

    //OK
    NNet Turnir()
    {
        int indv1 = Random.Range(0, pop_size - 1);
        int indv2 = Random.Range(0, pop_size - 1);
        int indv3 = Random.Range(0, pop_size - 1);

        float best_fitness = population[indv1].fitness;
        int best = indv1;

        if (population[indv2].fitness > best_fitness)
        {
            best_fitness = population[indv2].fitness;
            best = indv2;
        }

        if (population[indv3].fitness > best_fitness)
        {
            best_fitness = population[indv3].fitness;
            best = indv3;
        }

        return population[best].GetCopy(layer_count, neurons_count);
    }

    //OK
    NNet SelectBest()
    {
        float best_fitness = 0;
        int best_index = 0;
        float avg = 0;

        for (int i = 0; i < pop_size; i++)
        {
            if (population[i].fitness > best_fitness)
            {
                best_fitness = population[i].fitness;
                best_index = i;
            }
            avg += population[i].fitness;
        }

        if (best_fitness > global_best_fitness)
            global_best_fitness = best_fitness;

        Debug.Log("At generation: " + current_generation + "BEST fitness is:" + best_fitness);
        Debug.Log("At generation: " + current_generation + "AVG fitness is:" + avg / pop_size);
        return population[best_index].GetCopy(layer_count, neurons_count);
    }

    //OK
    private List<NNet> Cross(List<NNet> new_pop)
    {
        List<NNet> offsprings = new List<NNet>();

        for (int i = pop_size / 2; i < pop_size; i++)
        {
            int i_cross = Random.Range(0, pop_size / 2);
            int j_cross = Random.Range(0, pop_size / 2);

            NNet offspirng = new NNet();
            offspirng.Initialize(layer_count, neurons_count);

            for (int cross_point = 0; cross_point < cross_points - 1; cross_point += 2)
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
            offsprings.Add(offspirng);
        }

        for (int i = 0; i < offsprings.Count; i++)
        {
            new_pop.Add(offsprings[i]);
        }

        return new_pop;
    }

    List<NNet> MutatePop(List<NNet> new_pop)
    {
        for (int i = 0; i < new_pop.Count; i++)
        {
            new_pop[i] = Mutate(new_pop[i].GetCopy(layer_count, neurons_count));
        }

        return new_pop;
    }

    NNet Mutate(NNet offspring)
    {
        if (Random.Range(0f, 1f) < mutation_rate)
        {
            for (int i = 0; i < offspring.weights.Count; i++)
            {
                offspring.weights[i] = MutateMatrix(offspring.weights[i]);
            }

            MutateBiases(offspring.biases);
        }
        return offspring.GetCopy(layer_count, neurons_count);
    }

    //OK
    Matrix<float> MutateMatrix(Matrix<float> f)
    {
        int no_random_points = (int)Random.Range(1f, f.RowCount * f.ColumnCount / rc_to_mutate);

        Matrix<float> randomized = f.Clone();

        for (int _ = 0; _ < no_random_points; _++)
        {
            int random_row = Random.Range(0, randomized.RowCount - 1);
            int random_column = Random.Range(0, randomized.ColumnCount - 1);


            //RANDOMIZE ROW
            for (int i = 0; i < randomized.ColumnCount; i++)
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

    //OK
    void MutateBiases(List<float> biases)
    {
        for (int i = 0; i < biases.Count; i++)
        {
            biases[i] = Mathf.Clamp(biases[i] + Random.Range(-deviation, deviation), -1f, 1f);
        }
    }


    NNet ReadFile(string path)
    {
        List<string> lines = File.ReadLines(path).ToList();
        NNet network = new NNet();
        network.Initialize(layer_count, neurons_count);

        List<string> aux_s = new List<string>();
        List<float> aux_f = new List<float>();

        bool is_biases = false;
        string line;

        int w_count = 0;

        for(int i = 1; i <lines.Count; i++)
        {
            line = lines[i];

            if (Regex.Matches(line, @"Biases").Count > 0)
            {
                is_biases = true;
            }
            else if (line.Length > 0)
            {
                aux_s = line.Split(' ').ToList();
                aux_s.RemoveAt(aux_s.Count - 1);

                foreach(var nr in aux_s)
                {
                    aux_f.Add(float.Parse(nr));
                    //if (is_biases) Debug.Log(nr + "       " + float.Parse(nr));
                }
            }
            
            else if(line.Length == 0)
            {
                if (aux_f.Count > 0)
                {
                    if (is_biases)
                    {
                        for (int j = 0; j < aux_f.Count; j++)
                        {
                            network.biases[j] = aux_f[j];
                        }
                    }
                    else
                    {
                        int x = 0;
                        for (int j = 0; j < network.weights[w_count].RowCount; j++)
                        {
                            for (int jj = 0; jj < network.weights[w_count].ColumnCount; jj++)
                            {
                                network.weights[w_count][j, jj] = aux_f[x++];
                            }
                        }
                    }

                    aux_f.Clear();
                    w_count++;
                }
            }
        }
        return network;
    }

    void ShowDiffrences()
    {
        List<string> list = new List<string>();
        for (int i = 0; i < population.Count; i++)
        {
            string aux = "";
            for (int j = 0; j < population.Count; j++)
            {
                aux += Diffrences(population[i], population[j]);
                aux += " ";
            }
            list.Add(aux);
        }


        using (StreamWriter sw = File.AppendText(@"E:\Unity\Projects\MiniRace\Assets\Scripts\Data\debug.txt"))
        {
            foreach(var line in list)
            {
                sw.WriteLine(line);
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
        //Time.timeScale = sld_time_scale.value;

        for(int i = 0; i < cameras.Length; i++)
        {
            if (POV.value == i)
            {
                cameras[i].gameObject.SetActive(true);

            }
            else
                cameras[i].gameObject.SetActive(false);
        }
    }
}
