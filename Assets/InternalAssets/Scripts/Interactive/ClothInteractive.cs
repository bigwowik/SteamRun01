using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothInteractive : MonoBehaviour
{
    public int tapCountRequired = 0;
    [HideInInspector]
    public int currentCount = 0;

    
    public ClothState CheckState(GameObject gameObject)
    {
        //currentCount++;
        if (currentCount < tapCountRequired)
        {
            return ClothState.BadCondition;
        }
        else if (currentCount == tapCountRequired)
        {
            return ClothState.Ready;
        }
        else
        {
            return ClothState.BadCondition;
        }
    }

}

public enum ClothState
{
    NotReady,
    Ready,
    BadCondition
}
