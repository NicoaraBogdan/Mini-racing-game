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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Drive();
        Turn();
        ExtraGravity();
    }

    private void Drive()
    {
        if (Input.GetKey(KeyCode.W))
        {
            var current_speed = rigidbody.velocity;
            var update_speed = Vector3.Lerp(current_speed, new Vector3(Vector3.forward.x, 0, Vector3.forward.z) * speed, 0.9f);
            rigidbody.AddRelativeForce(update_speed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rigidbody.AddRelativeForce(new Vector3(Vector3.forward.x, 0, Vector3.forward.z) * -speed);
        }
        
        Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        localVelocity.x = 0;
        rigidbody.velocity = transform.TransformDirection(localVelocity);
    }

    void Turn()
    {
        Vector3 update_rotation = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            update_rotation = Vector3.up * -turn_speed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            update_rotation = Vector3.up * turn_speed;
        }
        rigidbody.AddTorque(update_rotation);
    }

    private void ExtraGravity()
    {
        rigidbody.AddForce(Vector3.down * gravity_multiplayer);
    }
}
