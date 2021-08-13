using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : MonoBehaviour, IInteractive
{
    public void OnInteract()
    {
        TrackManager.Instance.Damage();
        GetComponent<Collider>().enabled = false;

        //GetComponentInChildren<MeshRenderer>().material.color = Color.red;
    }
}
