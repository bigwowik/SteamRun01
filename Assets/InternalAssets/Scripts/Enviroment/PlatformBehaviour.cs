using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBehaviour : MonoBehaviour
{
    [SerializeField] private Transform platformSpawn;
    [SerializeField] private float zDistanceToTeleport = -10f;
    [SerializeField] private float speed = 3f;

    private void Update()
    {
        if(transform.position.z <= zDistanceToTeleport)
        {
           // transform.position = platformSpawn.position;
        }
        //transform.Translate(new Vector3(0, 0, -speed * Time.deltaTime));
    }
}
