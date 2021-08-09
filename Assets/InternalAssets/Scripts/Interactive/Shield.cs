using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IInteractive
{
    public float shieldTime = 10f;
    public void OnInteract()
    {
        TrackManager.Instance.SetShield(shieldTime);
    }
}
