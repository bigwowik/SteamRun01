using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static bool BoolWithChance(int chanceInPercent)
    {
        System.Random gen = new System.Random();
        int prob = gen.Next(100);
        return prob <= chanceInPercent;
    }

    
}
