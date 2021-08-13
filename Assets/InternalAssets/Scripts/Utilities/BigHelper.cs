using UnityEngine;
using System.Collections;


public static class BigHelper
{
    private static float DOT_THREASHOLD = 0.5f;


    /// <summary>
    /// Return random bool with similar weight (50%)
    /// </summary>
    /// <returns></returns>
    public static bool RandomBool(float v)
    {
        if (Random.Range(0, 1f) >= v)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    
    public static bool IsFacingTarget(Transform transform, Transform target)
    {
        var vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vectorToTarget);

        return dot >= DOT_THREASHOLD;
    }

    



}
