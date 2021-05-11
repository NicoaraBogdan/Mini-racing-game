using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
    
public class Sensor : MonoBehaviour
{
    public LayerMask track;

    public Transform checkpoints;

    public float
        speed_multiplier = 0.2f,
        distance_multiplier = 1.4f,
        sensor_multiplier = 0.3f,
        checkpoint_multiplier = 20f;

    private Vector3
        start_pos,
        start_rot,
        last_pos;

    private float
        time_passed,
        distance_traveled,
        avg_speed,
        time_check = 0,
        checkpoint_check = 0,
        fitness_gained_10sec = 0;

    public float
        sensor_data1,
        sensor_data2,
        sensor_data3,
        sensor_data4,
        sensor_data5,
        fitness = 0,
        sensor_fit = 0;

    private int no_checkpoints;

    // Start is called before the first frame update
    void Awake()
    {
        start_rot = transform.eulerAngles;
        start_pos = transform.position;
        last_pos = transform.position;
        fitness = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InputSensor();
        
        time_passed += Time.deltaTime;

        Fitness();

        if(time_check > 10f)
        {
            CheckIfStoped();
        }

        time_check += Time.deltaTime;
        last_pos = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("TrackWall"))
        {
            Death();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            no_checkpoints++;
            other.gameObject.SetActive(false);
        }

        if (other.CompareTag("Finish"))
        {
            Save(Time.time - GetComponent<Movment>().time_start);
            //Death();
            Debug.Log("Winner");
        }
    }

    public void Reset()
    {
        time_passed = 0;
        distance_traveled = 0;
        avg_speed = 0f;
        last_pos = start_pos;
        fitness = 0f;
        sensor_fit = 0f;
        no_checkpoints = 0;
        checkpoint_check = 0;
        time_check = 0;
        ResetCheckpoints();

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        transform.position = start_pos;
        transform.eulerAngles = start_rot;
    }

    private void ResetCheckpoints()
    {
        for(int i = 0; i < checkpoints.childCount; i++)
        {
            checkpoints.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void Death()
    {
        if (no_checkpoints == 0)
            FindObjectOfType<PopulationManager>().Death(0, no_checkpoints);
        else
            FindObjectOfType<PopulationManager>().Death(fitness, no_checkpoints);

            Reset();
    }

    private void Fitness()
    {
        distance_traveled += Vector3.Distance(transform.position, last_pos);

        avg_speed = distance_traveled / time_passed;
        sensor_fit += (sensor_data1 + sensor_data2 + sensor_data3 + sensor_data4 + sensor_data5) * sensor_multiplier / 5;

        fitness = (avg_speed * speed_multiplier)
                + distance_traveled * distance_multiplier
                + sensor_fit;
    }

    private void CheckIfStoped()
    {
        if(no_checkpoints - checkpoint_check < 1)
        {
            Death();
        }

        fitness_gained_10sec = fitness;
        time_check = 0f;
        checkpoint_check = no_checkpoints;
    }

    private void InputSensor()
    {
        Vector3 ray_dir1 = transform.forward;
        Vector3 ray_dir2 = Quaternion.AngleAxis(15f, Vector3.up) * transform.forward;
        Vector3 ray_dir3 = transform.forward + transform.right;
        Vector3 ray_dir4 = Quaternion.AngleAxis(-15f, Vector3.up) * transform.forward;
        Vector3 ray_dir5 = transform.forward - transform.right;

        Ray r = new Ray(transform.position, ray_dir1);
        RaycastHit hit;

        if(Physics.Raycast(r, out hit, 50f, track))
        {
            sensor_data1 = hit.distance / 100;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }
        else { sensor_data1 = 1f; }

        r.direction = ray_dir2;

        if (Physics.Raycast(r, out hit, 50f, track))
        {
            sensor_data2 = hit.distance / 100;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = ray_dir3;

        if (Physics.Raycast(r, out hit, 50f, track))
        {
            sensor_data3 = hit.distance / 100;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = ray_dir4;

        if (Physics.Raycast(r, out hit, 50f, track))
        {
            sensor_data4 = hit.distance / 100;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = ray_dir5;

        if (Physics.Raycast(r, out hit, 50f, track))
        {
            sensor_data5 = hit.distance / 100;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }
    }

    public (float, float, float, float, float) GetSensorData()
    {
        return (sensor_data1, sensor_data2, sensor_data3, sensor_data4, sensor_data5);
    }

    private void Save(float time)
    {
        Debug.Log("saving");
        List<string> indv_to_save = new List<string>();
        string aux;
        NNet indv = GetComponent<Movment>().network;

        indv_to_save.Add("Generation: " + FindObjectOfType<PopulationManager>().current_generation);
        indv_to_save.Add("Time: " + time);
        indv_to_save.Add("");
        indv_to_save.Add("");

        
        indv_to_save.Add("Weights: ");
        for(int x = 0; x < indv.weights.Count; x++)
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
        for (int i = 0; i< indv.biases.Count; i++)
        {
            aux += indv.biases[i];
            aux += " ";
        }
        indv_to_save.Add(aux);
        indv_to_save.Add("");
        indv_to_save.Add("");

        string path = @"E:\Unity\Projects\MiniRace\Assets\Scripts\Data\saves.txt";
        
        using (StreamWriter sw = File.AppendText(path)) {
            foreach (var line in indv_to_save)
                sw.WriteLine(line);
        }
    }
}
