using System.Collections;
using System.Collections.Generic;
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
        no_checkpoints = 0,
        fitness = 0,
        sensor_fit = 0;

    // Start is called before the first frame update
    void Awake()
    {
        start_rot = transform.eulerAngles;
        start_pos = transform.position;
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
            FindObjectOfType<PopulationManager>().Death(0);
        else
            FindObjectOfType<PopulationManager>().Death(fitness);

            Reset();
    }

    private void Fitness()
    {
        distance_traveled += Vector3.Distance(transform.position, last_pos);

        avg_speed = distance_traveled / time_passed;
        sensor_fit += (sensor_data1 + sensor_data2 + sensor_data3) * sensor_multiplier / 3; 

        fitness = /*(avg_speed * speed_multiplier) */
                //+ distance_traveled * distance_multiplier
                + sensor_fit
                + (no_checkpoints * checkpoint_multiplier);

        //Debug.Log("speed" + (avg_speed * speed_multiplier).ToString());
        //Debug.Log("sensor" + (((sensor_data1 + 1.2f - Mathf.Abs(sensor_data2 + sensor_data3)) / 2) * sensor_multiplier).ToString());
        //Debug.Log("checkpoint" + (no_checkpoints * checkpoint_multiplier).ToString());
        //Debug.Log(fitness);
        //Debug.Break();

        //if (time_passed > 10 && fitness < 40)
        //{
        //    Death();
        //}
        //else if (fitness > 50 && no_checkpoints < 1)
        //{
        //    Death();
        //}

        if (fitness >= 1000)
        {
            //Saves network to a JSON
            Debug.Log("WINER!!!");
            Death();
        }

    }

    private void CheckIfStoped()
    {
        if(fitness - fitness_gained_10sec < 10f)
        {
            Death();
        }

        if(checkpoint_check - no_checkpoints < 1)
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
        Vector3 ray_dir2 = transform.forward + transform.right;
        Vector3 ray_dir3 = transform.forward - transform.right;

        Ray r = new Ray(transform.position, ray_dir1);
        RaycastHit hit;

        if(Physics.Raycast(r, out hit, 1000f, track))
        {
            sensor_data1 = hit.distance / 50;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = ray_dir2;

        if (Physics.Raycast(r, out hit, 1000f, track))
        {
            sensor_data2 = hit.distance / 50;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = ray_dir3;

        if (Physics.Raycast(r, out hit, 1000f, track))
        {
            sensor_data3 = hit.distance / 50;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }
    }

    public (float, float, float) GetSensorData()
    {
        return (sensor_data1, sensor_data2, sensor_data3);
    }
}
