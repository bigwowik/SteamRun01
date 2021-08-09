using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;



public class GameManager : Singleton<GameManager>
{


    public enum GameState
    {
        PREGAME,
        EndlessRunning,
        LevelsRunning,
        PAUSED
    }
    public EventGameState OnGameStateChanged; 

    public GameState _currentGameState = GameState.PREGAME;

    private GameState gameStateBeforePause;

    #region GameStates
    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        private set { _currentGameState = value; }
    }
    void UpdateState(GameState state)
    {
        GameState previousGameState = _currentGameState;
        _currentGameState = state;

        switch (_currentGameState)
        {
            case GameState.PREGAME:
                // Initialize any systems that need to be reset
                Time.timeScale = 1.0f;
                break;

            case GameState.EndlessRunning:
                //  режим игры
                Time.timeScale = 1.0f;
                break;
            case GameState.LevelsRunning:
                // меню паузы
                Time.timeScale = 1.0f;
                break;
            case GameState.PAUSED:
                // меню паузы
                Time.timeScale = 0.0f;
                break;

            default:
                break;
        }

        OnGameStateChanged.Invoke(_currentGameState, previousGameState);
        Debug.Log(name + " : game mode was changed. Current state : " + _currentGameState + ". Previous state : " + previousGameState);
    }

    public void TogglePause()
    {
        //Debug.Log("Toggle Pause");
        if (_currentGameState != GameState.PAUSED)
        {
            gameStateBeforePause = _currentGameState;
            UpdateState(GameState.PAUSED);
        }
        else
        {
            UpdateState(gameStateBeforePause);
        }
    }

    public void SetStartRunningEndless()
    {
        if (_currentGameState != GameState.EndlessRunning)
        {
            gameStateBeforePause = _currentGameState;
            UpdateState(GameState.EndlessRunning);
        }
    }
    public void SetStartRunningLevels()
    {
        if (_currentGameState != GameState.LevelsRunning)
        {
            gameStateBeforePause = _currentGameState;
            UpdateState(GameState.LevelsRunning);
        }
    }
    #endregion

    public void RestartLevel() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Start()
    {
       // UpdateState(GameState.RUNNING);
    }




}

[System.Serializable] public class EventGameState : UnityEvent<GameManager.GameState, GameManager.GameState> { }
