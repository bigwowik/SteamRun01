using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using GameObject = UnityEngine.GameObject;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

/// <summary>
/// The TrackManager handles creating track segments, moving them and handling the whole pace of the game.
/// 
/// The cycle is as follows:
/// - Begin is called when the game starts.
///     - if it's a first run, init the controller, collider etc. and start the movement of the track.
///     - if it's a rerun (after watching ads on GameOver) just restart the movement of the track.
/// - Update moves the character and - if the character reaches a certain distance from origin (given by floatingOriginThreshold) -
/// moves everything back by that threshold to "reset" the player to the origin. This allow to avoid floating point error on long run.
/// It also handles creating the tracks segements when needed.
/// 
/// If the player has no more lives, it pushes the GameOver state on top of the GameState without removing it. That way we can just go back to where
/// we left off if the player watches an ad and gets a second chance. If the player quits, then:
/// 
/// - End is called and everything is cleared and destroyed, and we go back to the Loadout State.
/// </summary>
public class TrackManager : Singleton<TrackManager>
{
    static public TrackManager instance { get { return s_Instance; } }
    static protected TrackManager s_Instance;


    [Header("Character & Movements")]
    public PlayerMovement characterController;
    [Header("Скорость")]
    public float minSpeedForEndless = 7.0f;
    public float minSpeed = 5.0f;
    public float maxSpeed = 10.0f;
    public float currentSpeed = 0.03f;
    [Header("Передвижение")]
    public float horizontalMovingSpeed = 4;
    public float horizontalStepDistance = 3f;
    public float horizontalSpeedRatio = 1.5f;
    [Header("Прыжок")]
    public float jumpHeight = 3f;
    public float jumpLength = 3f;
    public float jumpSpeed = 3f;


    protected const float k_FloatingOriginThreshold = 10000f;
    protected const float k_Acceleration = 0.2f;
    public float timeToDoubleTap = 0.33f;



    [Header("Спавн")]
    public int startSpawnObjectIndexDelay = 1;
    [Header("Распределние спавна")]
    public int buffPercent = 20;
    public int debuffPercent = 20;
    public int upClothPercent = 35;
    public int cloth0Percent = 5;
    public int cloth1Percent = 5;
    public int cloth2Percent = 5;
    public int emptyClothPercent = 20;
    [Header("Какие интервалы у объектов")]
    public int buffInterval = 5;
    public int debuffInterval = 5;
    [Header("Настройки сегментов")]
    public float trackSegmentDistance = 10f;//длина сегмента
    public int trackSegmentCount = 10; //start count of segments
    public float trackSegmentTeleportDistance = 20f;//расстояние для телепорта

    [HideInInspector]
    public int lastBuffSegmentIndex;
    [HideInInspector]
    public int lastDebuffSegmentIndex;

    [Header("Сегменты")]
    public GameObject[] segmentPrefabs;

    //clothes objects
    public GameObject[] clothes;
    //up objects
    public GameObject[] upClothes;
    //up objects
    public GameObject endLevelTrigger;
    //buff objects
    public GameObject[] buffObjects; // 0 - огнетушитель хиллер, 1 - щит
    //debuff objects
    public GameObject[] debuffObjects;  // 0 - лужа 


    protected const float k_SegmentRemovalDistance = -30f; // когда удаляются платформы

    [Header("Счет и жизни")]
    public int currentLives = 2;
    public int startLivesLevels = 3;
    public int startLivesEndless = 5;


    IEnumerator damageEnumerator;
    float damageEffectTime = 1.5f;

    IEnumerator successEnumerator;
    float successEffectTime = 0.5f;

    public int StartLives
    {
        get
        {
            if (GameManager.Instance.CurrentGameState == GameManager.GameState.EndlessRunning)
                return startLivesEndless;
            else if (GameManager.Instance.CurrentGameState == GameManager.GameState.LevelsRunning)
                return startLivesLevels;
            else
                return 1;
        }
    }
    public float worldDistance { get { return m_TotalWorldDistance; } }
    public float speedRatio { get { return (currentSpeed - minSpeed) / (maxSpeed - minSpeed); } }

    public UnityEvent onPlayerLivesChanged;


    protected float m_TotalWorldDistance;
    protected int lastSegmentCount;

    
    public List<TrackSegment> segments { get { return m_Segments; } }
    protected float m_CurrentSegmentDistance;
    protected List<TrackSegment> m_Segments = new List<TrackSegment>();
    protected List<TrackSegment> m_PastSegments = new List<TrackSegment>();

    private int _spawnedSegments = 0;

    private int lastSpawnedSegmentCount = 0;
    
    public int GetNewSegmentIndexWithIncrement()
    {
        lastSpawnedSegmentCount++;
        return lastSpawnedSegmentCount;
    }
    public int GetLastSpawnedSegmentIndex()
    {
        return lastSpawnedSegmentCount;
    }

    [Header("Камера")]
    public Transform cinemachineCamera;

    [Header("UI")]
    public GameObject scorePanel;
    public Text clothesScoreText;
    public Text clothesScoreTextWin;
    public Text clothesScoreTextFail;
    public Text speedText;
    public Text livesText;
    public GameObject damageImg;
    public GameObject failureWindow;
    public GameObject successImage;
    public Text currentLevelInt;
    public Image shieldTimerImage;

    [Header("Состояние")]
    int clothesScore;

    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool godMode;
    [HideInInspector]
    public bool wasDied = false; //умер ли игрок один раз уже

    [Header("Уровни")]
    public int currentLevel = 0;
    public LevelsCollection levelsCollection;
    [Header("Паттерны")]
    public int currentPatternID = 0; //номер конкретного паттерна
    public bool isCurrentPatternInversed;
    public int lastPatternStartTileId = 0;  //id тайла с которого начался последний паттери
    public LevelsCollection patternsCollection;

    // other
    IEnumerator shieldEnumerator; //таймер действия щита
    public bool isProtectedByShield { get; private set; }

    GameManager.GameState lastGameState;




    #region Start
    private void Start()
    {
        GameManager.Instance.onGameModeStart.AddListener(OnGameModeStart);

        GameManager.Instance.onGameStateChanged.AddListener(OnStartRun);



        OnStartRun(GameManager.Instance.CurrentGameState, GameManager.GameState.PREGAME); // not super correct




        levelsCollection.UpdateLevels();
        patternsCollection.UpdateLevels();
    }

    void OnGameModeStart(GameMode gameModeStart) 
    {
        if(gameModeStart == GameMode.EndlessMode)
        {
            Debug.Log("Start game endless.");

            EndlessStart();
            CommonStartGame();
        }
        else if(gameModeStart == GameMode.LevelsMode)
        {
            Debug.Log("Start game levels.");

            LevelsStart();
            CommonStartGame();
        }
        
    }
    private void OnStartRun(GameManager.GameState currentGameState, GameManager.GameState previusGameState)
    {
        if ((currentGameState == GameManager.GameState.EndlessRunning || currentGameState == GameManager.GameState.LevelsRunning) && previusGameState == GameManager.GameState.PAUSED)
        {
            ConinueGameAfterPause();

            onPlayerLivesChanged.Invoke();//обновление состояния жизней

            Debug.Log("Coninue Game After Pause");
            //StartGame();

        }
        else if ((currentGameState == GameManager.GameState.LevelsRunning) && !wasDied)
        {
            //Debug.Log("Start game.");

            LevelsStart();
            CommonStartGame();

        }

        //else if ((currentGameState == GameManager.GameState.EndlessRunning || currentGameState == GameManager.GameState.LevelsRunning) && !wasDied)
        //{
            //Debug.Log("Start game.");
            //StartGame();
            
        //}
        else if ((currentGameState == GameManager.GameState.EndlessRunning || currentGameState == GameManager.GameState.LevelsRunning) && wasDied)
        {
            Debug.Log("Continue game.");
            ConinueGameAfterADS();


            onPlayerLivesChanged.Invoke();//обновление состояния жизней
        }
        else
        {
            //
        }
    }
    


    void CommonStartGame()
    {
        //reset segments
        foreach (TrackSegment seg in m_Segments)
            Destroy(seg.gameObject);
        
        m_Segments.Clear();
        _spawnedSegments = 0;


        //включение ui
        scorePanel.SetActive(true);

        //сброс игрока  и счета и счетчика сегментов
        wasDied = false;
        clothesScore = 0;

        lastSpawnedSegmentCount = 0;
        lastPatternStartTileId = startSpawnObjectIndexDelay;
        
        //состояение жизней сброс
        livesText.text = currentLives + "";
        damageImg.gameObject.SetActive(false);
        failureWindow.SetActive(false);
        successImage.SetActive(false);


        //можно двигаться
        isMoving = true;

        StartCoroutine(SpawnNewSegment());


        onPlayerLivesChanged.Invoke();//обновление состояния жизней

        Debug.Log("new level progress: " + GameManager.Instance.LEVELPROGRESS);
    }

    void EndlessStart()
    {
        //установка жизней
        currentLives = startLivesEndless;
        //установка скорости
        minSpeed = minSpeedForEndless;
        currentSpeed = minSpeed;



    }
    void LevelsStart()
    {
        //установка жизней
        currentLives = startLivesLevels;

        //установка скорости
        minSpeed = levelsCollection.startSpeedLevels[GameManager.Instance.LEVELPROGRESS];
        currentSpeed = minSpeed;
        //установка номера уровня
        currentLevelInt.text = GameManager.Instance.LEVELPROGRESS + "";
    }

    void ConinueGameAfterADS()
    {
        scorePanel.SetActive(true);

        isMoving = true;
        wasDied = false;
        damageImg.gameObject.SetActive(false);
        currentLives = StartLives;
        livesText.text = currentLives + "";
    }

    void ConinueGameAfterPause()
    {
        isMoving = true;

        onPlayerLivesChanged.Invoke();//обновление состояния жизней
    }
    public void ContinueADS()
    {

        GameManager.Instance.ContinueGameState(lastGameState);

        onPlayerLivesChanged.Invoke();//обновление состояния жизней

    }

    public void EndLevel()
    {
        isMoving = false;
        GameManager.Instance.SetWinState();
        if (GameManager.Instance.LEVELPROGRESS + 1 >= levelsCollection.levelDataDict.Count)
        {
            GameManager.Instance.LEVELPROGRESS = 0;
        }
        else
        {
            GameManager.Instance.LEVELPROGRESS++;
        }
        //Debug.Log("new level progress: " + GameManager.LEVELPROGRESS);
    }

    public void StartNextLevel()
    {
        //isMoving = true;
        wasDied = false;
        GameManager.Instance.SetStartRunningLevels();

        onPlayerLivesChanged.Invoke();//обновление состояния жизней

    }
    public void Restart()
    {
        GameManager.Instance.RestartLevel();

    }





    #endregion

    #region Update
    void Update()
    {
        float scaledSpeed = currentSpeed * Time.deltaTime;

        m_TotalWorldDistance += scaledSpeed;

        Transform characterTransform = characterController.transform;
        Vector3 currentPos = characterTransform.position;

        // Floating origin implementation
        // Move the whole world back to 0,0,0 when we get too far away.
        bool needRecenter = currentPos.sqrMagnitude > k_FloatingOriginThreshold;

        if (needRecenter)
        {

            Debug.Log("Need to recenter.");

            foreach (TrackSegment trackSegment in m_Segments)
            {
                trackSegment.transform.position -= currentPos;
            }
            Debug.Log("Recenter.");
            characterController.SetTransformPosition(characterController.transform.position- currentPos);
            //cinemachineCamera.gameObject.SetActive(false);
            //cinemachineCamera.position -= currentPos;
            //cinemachineCamera.gameObject.SetActive(true);
        }

        //ускорение со временем
        if(isMoving)
            SpeedUp();

        //обновление скорсти на экране
        UpdateSpeedUI();


    }
    #endregion

    #region Spawn

    private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);
    public IEnumerator SpawnNewSegment()
    {
        while (_spawnedSegments < trackSegmentCount)
        {
            Vector3 newPos = new Vector3(horizontalStepDistance / 2, -1f, _spawnedSegments * trackSegmentDistance);

            var rndId = Random.Range(0, segmentPrefabs.Length);
            //var rndId = gameObject.GetInstanceID() % Random.Range(0, segmentPrefabs.Length);
            GameObject newSegmentGameObject = Instantiate(segmentPrefabs[rndId], newPos, Quaternion.identity);

            TrackSegment newSegment = newSegmentGameObject.GetComponent<TrackSegment>();
            newSegment.trackManager = this;
            m_Segments.Add(newSegment);
            //yield return new WaitForSeconds(0.0001f);
            _spawnedSegments++;
        }

        yield return null;

    }

    public LevelData GetCurrentPattern()
    {
        var patternEndTile = lastPatternStartTileId + patternsCollection.levelDataDict[currentPatternID].levelTileDatas.Count - 1;

        if (GetLastSpawnedSegmentIndex() <= patternEndTile) //если еще идет последний паттерн
        {
            return patternsCollection.levelDataDict[currentPatternID];
        }
        else
        {
            lastPatternStartTileId = GetLastSpawnedSegmentIndex();
            currentPatternID = Random.Range(0, patternsCollection.levelDataDict.Count);
            Debug.Log("New pattern: " + currentPatternID);

            isCurrentPatternInversed = BigHelper.RandomBool(0.5f);
            return patternsCollection.levelDataDict[currentPatternID];
        }
    }
    public bool isLastSegmentOfPattern()
    {
        var patternEndTile = lastPatternStartTileId + patternsCollection.levelDataDict[currentPatternID].levelTileDatas.Count - 1;
        return GetLastSpawnedSegmentIndex() == patternEndTile;
    }
    #endregion

    #region Score and lives
    public void UpScore(int amount)
    {
        if (amount > 0)
        {
            clothesScore += amount;
            UpdateUI();

            if (successEnumerator != null)
                StopCoroutine(successEnumerator);
            successEnumerator = OnOffEffectTimer(successEffectTime, successImage);
            StartCoroutine(successEnumerator);

        }
        else
        {
            Damage();
        }

    }

    public void Damage()
    {
        if (isProtectedByShield)
            return;

        currentLives--;
        livesText.text = currentLives + "";
        if (currentLives != 0)
        {

            if(GameManager.Instance.CurrentGameState == GameManager.GameState.LevelsRunning)
            {
                damageImg.SetActive(true);
            }
            else if(GameManager.Instance.CurrentGameState == GameManager.GameState.EndlessRunning)
            {
                if (damageEnumerator != null)
                    StopCoroutine(damageEnumerator);
                damageEnumerator = OnOffEffectTimer(damageEffectTime, damageImg);
                StartCoroutine(damageEnumerator);
            }

            
        }
        else
        {
            YouFail();
        }
        onPlayerLivesChanged.Invoke();
    }

    IEnumerator OnOffEffectTimer(float time, GameObject objectToOnOff)
    {
        //Debug.Log("Damage effect timer");
        objectToOnOff.SetActive(true);
        yield return new WaitForSeconds(time);
        objectToOnOff.SetActive(false);
        yield return null;
    }

    

    public void YouFail()
    {
        isMoving = false;
        failureWindow.SetActive(true);
        wasDied = true;
        lastGameState = GameManager.Instance.CurrentGameState;
        StopAllCoroutines();
        GameManager.Instance.SetFailureState();

    }
    #endregion

    #region Buffs and debuffs

    public void Healing()
    {
        if (currentLives + 1 >= StartLives)
            currentLives = StartLives;
        else
            currentLives++;

        livesText.text = currentLives + "";

        //визуальный эффект
        if (successEnumerator != null)
            StopCoroutine(successEnumerator);
        successEnumerator = OnOffEffectTimer(successEffectTime, successImage);
        StartCoroutine(successEnumerator);



        damageImg.gameObject.SetActive(false);

        onPlayerLivesChanged.Invoke();

    }
    public void SetShield(float shieldTime)
    {
        if(shieldEnumerator != null)
            StopCoroutine(shieldEnumerator);
        shieldEnumerator = ShieldStart(shieldTime);
        StartCoroutine(shieldEnumerator);
    }

    IEnumerator ShieldStart(float shieldTime)
    {
        Debug.Log("Shield  iEnumerator");
        isProtectedByShield = true;
        float timer = 0;
        shieldTimerImage.gameObject.SetActive(true);
        while (timer < shieldTime)
        {
            timer += Time.deltaTime;

            shieldTimerImage.fillAmount = (shieldTime - timer) / shieldTime;
            yield return null;
        }

        shieldTimerImage.gameObject.SetActive(false);
        //yield return new WaitForSeconds(shieldTime);
        isProtectedByShield = false;
        yield return null;
    }
    #endregion

    #region UI
    void UpdateUI()
    {
        clothesScoreText.text = clothesScore + "";
        clothesScoreTextWin.text = clothesScore + "";
        clothesScoreTextFail.text = clothesScore + "";
    }

    void UpdateSpeedUI()
    {
        speedText.text = currentSpeed + "";
    }
    #endregion

    #region Moving
    void SpeedUp()
    {
        if (currentSpeed < maxSpeed)
            currentSpeed += k_Acceleration * Time.deltaTime;
        else
            currentSpeed = maxSpeed;
    }

    

    #endregion






}