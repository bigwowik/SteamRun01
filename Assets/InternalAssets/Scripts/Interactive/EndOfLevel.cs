using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfLevel : MonoBehaviour, IInteractive
{
    public void OnInteract()
    {
        //
        TrackManager.Instance.EndLevel();

    }
}
