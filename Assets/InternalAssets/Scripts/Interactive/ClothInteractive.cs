using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothInteractive : MonoBehaviour
{
    public int tapCountRequired = 0;
    public int currentCount = 0;

    

    private void OnEnable()
    {
        currentCount = 0;
    }
    public ClothState Interact(GameObject gameObject)
    {
        currentCount++;
        if (currentCount < tapCountRequired)
        {
            return ClothState.NotReady;
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
