using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Movment : MonoBehaviour
{
    [SerializeField]
    private new Rigidbody rigidbody;

    [SerializeField]
    private float 
        speed,
        top_speed,
        turn_speed,
        gravity_multiplayer;

    public NNet network;

    private float s1, s2, s3, s4, s5, speed_net, turn_net;

    public float
        time_start,
        time;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        (s1, s2, s3, s4, s5) = GetComponent<Sensor>().GetSensorData();

        if (network.weights.Count > 0)
            (speed_net, turn_net) = network.RunNetwork(s1, s2, s3, s4, s5);

        Drive(speed_net);
        Turn(turn_net);
        ExtraGravity(); 
    }

    public void ResetWithNetwork(NNet network)
    {
        this.network = network;
        time_start = Time.time;
    }

    private void Save(float time, NNet indv)
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

    private void Drive()
    {
        float drive_dir = Input.GetAxisRaw("Vertical");
        rigidbody.AddRelativeForce(new Vector3(Vector3.forward.x, 0, Vector3.forward.z) * speed * drive_dir);

        Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        localVelocity.x = 0;
        rigidbody.velocity = transform.TransformDirection(localVelocity);
    }

    void Turn()
    {
        Vector3 update_rotation = Vector3.zero;
        float turn_dir = Input.GetAxisRaw("Horizontal");

        update_rotation = Vector3.up * turn_speed * turn_dir;
        rigidbody.AddTorque(update_rotation);
    }

    private void Drive(float network_drive_dir)
    {
        float drive_dir = Input.GetAxisRaw("Vertical");
        rigidbody.AddRelativeForce(new Vector3(Vector3.forward.x, 0, Vector3.forward.z) * speed * network_drive_dir);

        Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        localVelocity.x = 0;
        rigidbody.velocity = transform.TransformDirection(localVelocity);
    }

    void Turn(float network_turn)
    {
        Vector3 update_rotation = Vector3.zero;
        float turn_dir = Input.GetAxisRaw("Horizontal");

        update_rotation = Vector3.up * turn_speed * network_turn;

        rigidbody.AddTorque(update_rotation);
    }

    private void ExtraGravity()
    {
        rigidbody.AddForce(Vector3.down * gravity_multiplayer);
    }
}
