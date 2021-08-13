using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour, IInteractive
{
    public void OnInteract()
    {
        TrackManager.Instance.Healing();
        GetComponent<Collider>().enabled = false;
        GetComponentInChildren<MeshRenderer>().material.color = Color.white;
    }
}
