using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RunningMenu : MonoBehaviour
{
    //жизни героя
    public Transform livesParent;
    public GameObject livesImagePrefab;
    private List<GameObject> livesImagesList = new List<GameObject>();
    //жизни героя



    void Start()
    {
        GameManager.Instance.OnGameStateChanged.AddListener(OnGameStart);
        TrackManager.Instance.onPlayerLivesChanged.AddListener(OnLivesChanged);
    }
    private void OnGameStart(GameManager.GameState currentGameState, GameManager.GameState previusGameState)
    {
        
        if ((currentGameState == GameManager.GameState.EndlessRunning))
        {
            OnStartImages();
            livesParent.gameObject.SetActive(true);
        }
        else if(currentGameState == GameManager.GameState.LevelsRunning)
        {
            livesParent.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if(TrackManager.Instance != null)
            TrackManager.Instance.onPlayerLivesChanged.RemoveListener(OnLivesChanged);
    }

    void OnStartImages()
    {
        
        Debug.Log("On start lives images.");

        for (int i = 0; i < TrackManager.Instance.StartLives; i++)
        {
            livesImagesList.Add(Instantiate(livesImagePrefab, livesParent));
        }
    }

    void OnLivesChanged()
    {
        for (int i = 0; i < livesImagesList.Count; i++)
        {
            Destroy(livesImagesList[i]);
        }
        livesImagesList.Clear();
        Debug.Log("On lives changed.");

        for (int i = 0; i < TrackManager.Instance.currentLives; i++)
        {
            livesImagesList.Add(Instantiate(livesImagePrefab, livesParent));
        }
    }

    #region Lives changes
    void SpawnLiveImage()
    {
        livesImagesList.Add(Instantiate(livesImagePrefab, livesParent));
    }
    void RemoveLiveImage()
    {
        Destroy(livesImagesList[livesImagesList.Count - 1]);
        livesImagesList.RemoveAt(livesImagesList.Count - 1);
        Debug.LogFormat("Lives was removed.");
    }
    #endregion
}
