using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private float smooth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, player.position, smooth);
        transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, smooth / 2);
        transform.rotation = Quaternion.Euler(new Vector3(0, player.rotation.eulerAngles.y, 0));
    }
}
