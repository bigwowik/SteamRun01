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
        if ((currentGameState == GameManager.GameState.LevelsRunning || currentGameState == GameManager.GameState.EndlessRunning))
        {
            for (int i = 0; i < TrackManager.Instance.StartLives; i++)
            {
                SpawnLiveImage();
            }
        }
    }

    private void OnDisable()
    {
        if(TrackManager.Instance != null)
            TrackManager.Instance.onPlayerLivesChanged.RemoveListener(OnLivesChanged);
    }

    void OnLivesChanged(int amount)
    {
        for (int i = 0; i < livesImagesList.Count; i++)
        {
            Destroy(livesImagesList[i]);
        }
        livesImagesList.Clear();

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
