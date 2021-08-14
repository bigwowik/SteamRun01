using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;




public class GameManager : Singleton<GameManager>
{
    #region New Enums
   
    

    public enum GameState
    {
        PREGAME,
        EndlessRunning,
        LevelsRunning,
        WIN,
        FAILURE,
        PAUSED
    }
    #endregion
    public EventGameState onGameStateChanged;
    public EventFadeModeStart onFadeModeStart;
    public EventOnGameModeStart onGameModeStart;

    public GameState _currentGameState = GameState.PREGAME;

    private GameState gameStateBeforePause;


    public int LEVELPROGRESS;


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
            case GameState.WIN:
                // меню паузы
                Time.timeScale = 1f;
                break;
            case GameState.FAILURE:
                // меню паузы
                Time.timeScale = 1f;
                break;
            case GameState.PAUSED:
                // меню паузы
                Time.timeScale = 0.0f;
                break;

            default:
                break;
        }

        onGameStateChanged.Invoke(_currentGameState, previousGameState);
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

    //endless mode start
    public void SetStartRunningEndless()
    {
        if (_currentGameState != GameState.EndlessRunning)
        {
            
            gameStateBeforePause = _currentGameState;
            UpdateState(GameState.EndlessRunning);
        }
    }
    public void StartRunningEndless()
    {
        
        
        onGameModeStart.Invoke(GameMode.EndlessMode);
        SetStartRunningEndless();
    }

    //levels mode start
    public void SetStartRunningLevels()
    {
        if (_currentGameState != GameState.LevelsRunning)
        {
            gameStateBeforePause = _currentGameState;
            UpdateState(GameState.LevelsRunning);
        }
    }
    public void StartRunningLevels()
    {
        
        
        onGameModeStart.Invoke(GameMode.LevelsMode);
        SetStartRunningLevels();
    }



    public void SetWinState()
    {
        if (_currentGameState != GameState.WIN)
        {
            gameStateBeforePause = _currentGameState;
            UpdateState(GameState.WIN);
        }
    }
    public void SetFailureState()
    {
        if (_currentGameState != GameState.FAILURE)
        {
            gameStateBeforePause = _currentGameState;
            UpdateState(GameState.FAILURE);
        }
    }

    public void ContinueGameState(GameState newGameState)
    {
        if (newGameState == GameState.LevelsRunning || newGameState == GameState.EndlessRunning)
        {
            gameStateBeforePause = _currentGameState;
            UpdateState(newGameState); 

        }
    }
    #endregion

    public void RestartLevel() 
    {
        float timeToRestart = 1f;
        onFadeModeStart.Invoke(FadeMode.FadeOut);
        Invoke("RestartLevelAfterTime", timeToRestart);
        
    }

    void RestartLevelAfterTime()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Start()
    {
         UpdateState(GameState.PREGAME);
    }




}

public enum FadeMode
{
    FadeIn,
    FadeOut
}
public enum GameMode
{
    EndlessMode,
    LevelsMode
}

[System.Serializable] public class EventGameState : UnityEvent<GameManager.GameState, GameManager.GameState> { }
[System.Serializable] public class EventFadeModeStart : UnityEvent<FadeMode> { }

[System.Serializable] public class EventOnGameModeStart : UnityEvent<GameMode> { }
