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
public class TrackManager : MonoBehaviour
{
    static public TrackManager instance { get { return s_Instance; } }
    static protected TrackManager s_Instance;


    [Header("Character & Movements")]
    public PlayerMovement characterController;
    public float minSpeed = 5.0f;
    public float maxSpeed = 10.0f;
    public int speedStep = 4;
    public float stepDistance = 3f;
    public float laneOffset = 1.0f;
    public float currentSpeed = 0.03f;


    public float worldDistance { get { return m_TotalWorldDistance; } }
    public float speedRatio { get { return (currentSpeed - minSpeed) / (maxSpeed - minSpeed); } }


    public bool invincible = false;

    public float trackSegmentDistance = 10f;//длина сегмента

    public int trackSegmentCount = 10; //start count of segments

    public GameObject segmentPrefab;

    public List<TrackSegment> segments { get { return m_Segments; } }


    protected float m_CurrentSegmentDistance;

    protected List<TrackSegment> m_Segments = new List<TrackSegment>();


    protected List<TrackSegment> m_PastSegments = new List<TrackSegment>();


    const float k_FloatingOriginThreshold = 10000f;

    protected const float k_Acceleration = 0.2f;


    protected const float k_SegmentRemovalDistance = -30f;

    public Transform cinemachineCamera;

    protected float m_TotalWorldDistance;

    //clothes objects
    public GameObject[] clothes;

    int clothesScore;

    public Text clothesScoreText;

    protected void Awake()
    {
        s_Instance = this;

        
    }
    private void Start()
    {
        clothesScore = 0;
        StartMove();
    }


    private int _parallaxRootChildren = 0;
    private int _spawnedSegments = 0;
    void Update()
    {
        while (_spawnedSegments < trackSegmentCount)
        {
            StartCoroutine(SpawnNewSegment());
            _spawnedSegments++;
        }

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
            cinemachineCamera.position -= currentPos;
        }


        //ускорение со временем
        SpeedUp();


    }

    

    private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);
    public IEnumerator SpawnNewSegment()
    {
        Vector3 newPos = new Vector3(0, -1f, _spawnedSegments * trackSegmentDistance);
        GameObject newSegmentGameObject = Instantiate(segmentPrefab, newPos, Quaternion.identity);

        TrackSegment newSegment = newSegmentGameObject.GetComponent<TrackSegment>();
        newSegment.manager = this;
        m_Segments.Add(newSegment);



        yield return null;

    }


    public void UpScore(int amount)
    {
        clothesScore += amount;
        UpdateUI();
    }
    void UpdateUI()
    {
        clothesScoreText.text = clothesScore + "";
    }

    public void StartMove()
    {
        currentSpeed = minSpeed;
    }

    void SpeedUp()
    {
        if (currentSpeed < maxSpeed)
            currentSpeed += k_Acceleration * Time.deltaTime;
        else
            currentSpeed = maxSpeed;
    }




}