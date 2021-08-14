using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateEnabler : MonoBehaviour
{
    [Header("На каком режиме объект должен работать.")]
    public GameManager.GameState gameStateEnabled = GameManager.GameState.PREGAME;
    public GameObject gameObjectToEnable;


    private void Awake()
    {
        if (gameObjectToEnable == null)
            gameObjectToEnable = transform.GetChild(0).gameObject;
    }
    private void Start()
    {
        GameManager.Instance.onGameStateChanged.AddListener(OnStartRun);


        

        OnStartRun(GameManager.Instance.CurrentGameState);
    }
    
    private void OnDisable()
    {
        //if(GameManager.Instance != null)
           // GameManager.Instance.OnGameStateChanged.RemoveListener(OnStartRun);
    }

    void OnStartRun(GameManager.GameState _currentGameState, GameManager.GameState previousGameState)
    {
        gameObjectToEnable.SetActive(_currentGameState == gameStateEnabled);
    }
    void OnStartRun(GameManager.GameState _currentGameState)
    {
        gameObjectToEnable.SetActive(_currentGameState == gameStateEnabled);
    }

}
