using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public int currentPlayMode;  //0 - infinity, 1 - levels

    public DanielLochner.Assets.SimpleScrollSnap.SimpleScrollSnap scrollSnap;


    public void ChangeCurrentPlayMode()
    {
        currentPlayMode = scrollSnap.CurrentPanel;

        Debug.Log("New play mode: " + currentPlayMode);
    }


    public void StartPlay()
    {
        switch (currentPlayMode)
        {
            case 0:
                //TrackManager.Instance.SetStartLivesEndless();
                //GameManager.Instance.SetStartRunningEndless();
                GameManager.Instance.StartRunningEndless();

                break;
            case 1:
                //TrackManager.Instance.SetStartLivesLevels();
                //GameManager.Instance.SetStartRunningLevels();
                GameManager.Instance.StartRunningLevels();
                break;
            default:
                Debug.Log("No playmode." );
                break;
        }
    }
}
