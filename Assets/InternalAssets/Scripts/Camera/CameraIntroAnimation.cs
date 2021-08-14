using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraIntroAnimation : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.onGameStateChanged.AddListener(OnStartRun);



    }

    private void OnDisable()
    {
        if(GameManager.Instance != null)
         GameManager.Instance.onGameStateChanged.RemoveListener(OnStartRun);
    }

    //������ �������� � ������ �������� ������
    void OnStartRun(GameManager.GameState _currentGameState, GameManager.GameState previousGameState)
    {
        if(_currentGameState == GameManager.GameState.EndlessRunning || _currentGameState == GameManager.GameState.LevelsRunning)
        {
            GetComponent<Animator>().SetTrigger("PlayMode");
            float timeToDisable = 0.75f; //����� ������� ��� �����
            Invoke("DisableCamera", timeToDisable);
        }
        
    }

    void DisableCamera()
    {
        gameObject.SetActive(false);
    }
    
}
