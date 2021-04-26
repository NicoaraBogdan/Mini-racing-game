using System.Collections;
using System.Collections.Generic;
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

    private NNet network;

    private float s1, s2, s3, speed_net, turn_net;

    // Start is called before the first frame update
    void Awake()
    {
        network = GetComponent<NNet>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        (s1, s2, s3) = GetComponent<Sensor>().GetSensorData();
        (speed_net, turn_net) = network.RunNetwork(s1, s2, s3);

        Drive(speed_net);
        Turn(turn_net);
        ExtraGravity(); 
    }

    public void ResetWithNetwork(NNet network)
    {
        this.network = network;
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
