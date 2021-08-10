using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using GameObject = UnityEngine.GameObject;
using UnityEngine.UI;

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



    [Header("Спавн")]
    public float startSpawnObjectDistance = 20f;

    public int upClothPercent = 35;
    public int emptyClothPercent = 20;


    public float trackSegmentDistance = 10f;//длина сегмента
    public int trackSegmentCount = 10; //start count of segments
    public float trackSegmentTeleportDistance = 20f;//расстояние для телепорта


    //clothes objects
    public GameObject[] clothes;
    //up objects
    public GameObject[] upClothes;
    //up objects
    public GameObject endLevelTrigger;

    protected const float k_SegmentRemovalDistance = -30f; // когда удаляются платформы

    [Header("Счет и жизни")]
    public int currentLives = 2;
    public int startLives = 2;
    public float worldDistance { get { return m_TotalWorldDistance; } }
    public float speedRatio { get { return (currentSpeed - minSpeed) / (maxSpeed - minSpeed); } }




    public bool invincible = false;


    public GameObject segmentPrefab;

    public List<TrackSegment> segments { get { return m_Segments; } }


    protected float m_CurrentSegmentDistance;

    protected List<TrackSegment> m_Segments = new List<TrackSegment>();


    protected List<TrackSegment> m_PastSegments = new List<TrackSegment>();


    const float k_FloatingOriginThreshold = 10000f;

    protected const float k_Acceleration = 0.2f;



    public Transform cinemachineCamera;


    protected float m_TotalWorldDistance;
    protected int lastSegmentCount;



    int clothesScore;

    public Text clothesScoreText;
    public Text speedText;
    public Text livesText;
    public Image damageImg;
    public GameObject failureWindow;

    public Text currentLevelInt;

    public bool isMoving = false;

    public float timeToDoubleTap = 0.5f;

    public int currentLevel = 0;

    public LevelsCollection levelsCollection;


    [HideInInspector]
    public bool godMode;


    IEnumerator shieldEnumerator;
    public bool isProtected { get; private set; }

    public Image shieldTimerImage;

    public bool wasDied = false;
    bool ADSwasWatched = false;

    #region Start
    private void Start()
    {
        GameManager.Instance.OnGameStateChanged.AddListener(OnStartRun);

        OnStartRun(GameManager.Instance.CurrentGameState, GameManager.GameState.PREGAME); // not super correct

        levelsCollection.UpdateLevels();
    }
    private void OnStartRun(GameManager.GameState currentGameState, GameManager.GameState previusGameState)
    {
        if ((currentGameState == GameManager.GameState.EndlessRunning || currentGameState == GameManager.GameState.LevelsRunning) && !wasDied)
        {
            Debug.Log("Start game.");
            StartGame();
        }
        else if ((currentGameState == GameManager.GameState.EndlessRunning || currentGameState == GameManager.GameState.LevelsRunning) && wasDied)
        {
            Debug.Log("Continue game.");
            ConinueGame();
        }
        else
        {
            //
        }
    }

    void StartGame()
    {
        foreach (TrackSegment seg in m_Segments)
            Destroy(seg.gameObject);
        
        m_Segments.Clear();
        _spawnedSegments = 0;




        wasDied = false;
        LastSpawnedSegmentCount = 0;
        clothesScore = 0;
        currentLives = startLives;
        livesText.text = currentLives + "";
        damageImg.gameObject.SetActive(false);
        failureWindow.SetActive(false);
        
        minSpeed = levelsCollection.startSpeedLevels[GameManager.Instance.LEVELPROGRESS];
        currentSpeed = minSpeed;


        currentLevelInt.text = GameManager.Instance.LEVELPROGRESS + "";
        isMoving = true;
        StartEndlessMove();

        
        Debug.Log("new level progress: " + GameManager.Instance.LEVELPROGRESS);
    }

    void ConinueGame()
    {
        isMoving = true;
        wasDied = false;
        damageImg.gameObject.SetActive(false);
        currentLives = startLives;
        livesText.text = currentLives + "";

    }

    private void PauseMenu()
    {
        isMoving = false;
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
        
    }

    public void ContinueADS()
    {
        GameManager.Instance.SetStartRunningLevels();

    }



    #endregion


    private int _parallaxRootChildren = 0;
    private int _spawnedSegments = 0;

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

            Debug.Log("need recenter");



            foreach (TrackSegment trackSegment in m_Segments)
            {
                trackSegment.transform.position -= currentPos;
            }
            characterController.transform.position -= currentPos;
            cinemachineCamera.gameObject.SetActive(false);
            cinemachineCamera.position -= currentPos;
            cinemachineCamera.gameObject.SetActive(true);
        }


        //ускорение со временем
        SpeedUp();

        //обновление скорсти на экране
        UpdateSpeedUI();


    }



    private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);
    public IEnumerator SpawnNewSegment()
    {
        while (_spawnedSegments < trackSegmentCount)
        {
            Vector3 newPos = new Vector3(horizontalStepDistance / 2, -1f, _spawnedSegments * trackSegmentDistance);
            GameObject newSegmentGameObject = Instantiate(segmentPrefab, newPos, Quaternion.identity);

            TrackSegment newSegment = newSegmentGameObject.GetComponent<TrackSegment>();
            newSegment.trackManager = this;
            m_Segments.Add(newSegment);
            yield return new WaitForSeconds(0.0001f);
            _spawnedSegments++;
        }

        yield return null;

    }


    public void UpScore(int amount)
    {
        if (amount > 0)
        {
            clothesScore += amount;
            UpdateUI();
        }
        else
        {
            Damage();
        }

    }

    public void Damage()
    {
        if (isProtected)
            return;

        currentLives--;
        livesText.text = currentLives + "";
        if (currentLives != 0)
            damageImg.gameObject.SetActive(true);
        else
        {
            YouFail();
        }
    }

    

    public void Healing()
    {
        //if (currentLives + 1 >= startLives)
            currentLives = startLives;
        //else
        //    currentLives++;
        livesText.text = currentLives + "";
        damageImg.gameObject.SetActive(false);
        
    }

    public void SetShield(float shieldTime)
    {
        shieldEnumerator = ShieldStart(shieldTime);
        StartCoroutine(shieldEnumerator);
    }

    IEnumerator ShieldStart(float shieldTime)
    {
        isProtected = true;
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
        isProtected = false;
        yield return null;
    }


    void UpdateUI()
    {
        clothesScoreText.text = clothesScore + "";
    }

    public void StartEndlessMove()
    {
        StartCoroutine(SpawnNewSegment());
    }


    public void YouFail()
    {
        isMoving = false;
        failureWindow.SetActive(true);
        wasDied = true;
        GameManager.Instance.SetFailureState();


        //Invoke("Restart", 3f);


    }

    void SpeedUp()
    {
        if (currentSpeed < maxSpeed)
            currentSpeed += k_Acceleration * Time.deltaTime;
        else
            currentSpeed = maxSpeed;
    }

    void UpdateSpeedUI()
    {
        speedText.text = currentSpeed + "";
    }

    void Restart()
    {
        GameManager.Instance.RestartLevel();
    }

    private int lastSpawnedSegmentCount;
    public int LastSpawnedSegmentCount
    {
        get
        {
            return lastSpawnedSegmentCount++;
        }
        private set
        {
            lastSpawnedSegmentCount = value;
        }






    }
}