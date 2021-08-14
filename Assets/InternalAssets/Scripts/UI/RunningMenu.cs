using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RunningMenu : MonoBehaviour
{
    //жизни героя
    public Transform livesParent;
    public Image livesImageFiller;
    private float[] fillAmounts = { 0, 0.187f, 0.332f, 0.48f, 0.635f, 1f };

    public GameObject runningMenuObject;
    public GameObject pauseBackground;
    public GameObject blurEffect;

    void Start()
    {
        GameManager.Instance.onGameStateChanged.AddListener(OnGameStart);



        TrackManager.Instance.onPlayerLivesChanged.AddListener(OnLivesChanged);

        runningMenuObject.SetActive(false);

        blurEffect.SetActive(false);

    }
    private void OnGameStart(GameManager.GameState currentGameState, GameManager.GameState previusGameState)
    {
        
        if ((currentGameState == GameManager.GameState.EndlessRunning))
        {
            //OnLivesChanged();
            livesParent.gameObject.SetActive(true);
            runningMenuObject.SetActive(true);
            pauseBackground.SetActive(false);

            blurEffect.SetActive(false);
        }
        else if(currentGameState == GameManager.GameState.LevelsRunning)
        {
            livesParent.gameObject.SetActive(false);
            runningMenuObject.SetActive(true);
            pauseBackground.SetActive(false);


            blurEffect.SetActive(false);
        }
        else if(currentGameState == GameManager.GameState.FAILURE || currentGameState == GameManager.GameState.WIN)
        {
            blurEffect.SetActive(true);
            runningMenuObject.SetActive(false);
        }
        else if (currentGameState == GameManager.GameState.PAUSED)
        {
            pauseBackground.SetActive(true);
            blurEffect.SetActive(true);

        }
        

    }

    private void OnDisable()
    {
        if(TrackManager.Instance != null)
            TrackManager.Instance.onPlayerLivesChanged.RemoveListener(OnLivesChanged);
    }

    void OnStartImages()
    {
        SetLivesImage(TrackManager.Instance.StartLives);
        Debug.Log("On start lives images.");
    }

    void OnLivesChanged()
    {
        Debug.Log("On lives changed.");
        SetLivesImage(TrackManager.Instance.currentLives);

    }
    void SetLivesImage(int livesCount)
    {
        float fill = fillAmounts[livesCount];
        livesImageFiller.fillAmount = fill;
    }


    
    public void PauseToggle()
    {
        GameManager.Instance.TogglePause();
        //Debug.Log("Pause to");
    }



}
