using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothInteractive : MonoBehaviour
{
    public int tapCountRequired = 0;
    [HideInInspector]
    public int currentCount = 0;

    float timer;
    bool wasTaped = false;
    bool wasEnded = false;

    private void Start()
    {
        wasTaped = false;
    }

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
    public void TapCloth()
    {
        if (!wasTaped)
        {
            wasTaped = true;
            currentCount = 1;
            timer = Time.time;
        }
        else if(Time.time < timer + TrackManager.Instance.timeToDoubleTap)
        {
            currentCount = 2;
        }


    }

    public void SingleTap()
    {
        currentCount = 1;
    }

    public void DoubleTap()
    {
        currentCount = 2;


    }

    private void Update()
    {
        if(!wasEnded && wasTaped && Time.time >= timer + TrackManager.Instance.timeToDoubleTap)
        {
            BadTap();
            wasEnded = true;
        }
    }

    void BadTap()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.black;
        GetComponentInChildren<MeshRenderer>().material.mainTexture = null;
        TrackManager.Instance.UpScore(-1);
        GetComponent<Collider>().enabled = false;
        
    }

    void GoodTap()
    {

    }

    void EndTaping()
    {

    }
}

public enum ClothState
{
    NotReady,
    Ready,
    BadCondition
}


