using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This defines a "piece" of the track. This is attached to the prefab and contains data such as what obstacles can spawn on it.
/// It also defines places on the track where obstacles can spawn. The prefab is placed into a ThemeData list.
/// </summary>
public class TrackSegment : MonoBehaviour
{
    public Transform pathParent;
    public TrackManager trackManager;

    public Transform objectRoot;
    public Transform collectibleTransform;



    [HideInInspector]
    public float[] obstaclePositions;

    public List<GameObject> spawnedObjects = new List<GameObject>();

    public int segmentCountIndex;

    public UnityEngine.UI.Text countText;




    private void Update()
    {
        if (trackManager.characterController.transform.position.z >= transform.position.z + trackManager.trackSegmentTeleportDistance)
        {
            NewSpawn();
        }

        countText.gameObject.SetActive(trackManager.godMode);  // убрать потом
    }

    private void Start()
    {
        Spawn();
    }



    void NewSpawn()
    {
        transform.position += new Vector3(0, 0, trackManager.trackSegmentDistance * trackManager.trackSegmentCount); //перемещение платформы

        Spawn();
    }

    void Spawn()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.EndlessRunning)
        {
            //SpawnRandomObjects();
            SpawnPatterns();
        }
        else if (GameManager.Instance.CurrentGameState == GameManager.GameState.LevelsRunning)
        {
            SpawnLevels();
        }
    }

    void ResetSegment()
    {
        foreach(GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    void SpawnPatterns()
    {
        ResetSegment();

        //номер сегмента
        segmentCountIndex = trackManager.GetNewSegmentIndexWithIncrement();
        countText.text = segmentCountIndex + "";
        gameObject.name = "platform - " + segmentCountIndex;

        if (trackManager.GetLastSpawnedSegmentIndex() < trackManager.startSpawnObjectIndexDelay)
            return;

        Debug.Log(gameObject.name +" was spawned");



        //спавн баффов и дебаффов
        if (trackManager.isLastSegmentOfPattern() && SpawnBuffAndDebuff())
        {
            Debug.Log("Buff and debuff spawned.");
            return;
        }
            




        var currentPattern = trackManager.GetCurrentPattern();
        
        var currentTiledata = currentPattern.levelTileDatas[segmentCountIndex - trackManager.lastPatternStartTileId ];
        
        if (currentTiledata != null)
        {
            var xPosition = trackManager.horizontalStepDistance / 2;

            if (trackManager.isCurrentPatternInversed)
                xPosition = -xPosition;

            SelectAndSpawnClothes(currentTiledata.leftSide, -xPosition);
            SelectAndSpawnClothes(currentTiledata.rightSide, xPosition);
        }

    }

    bool SpawnBuffAndDebuff()
    {
        float randomX;
        if (Random.Range(0, 1f) > 0.5f)
        {
            randomX = trackManager.horizontalStepDistance / 2;
        }
        else
        {
            randomX = -trackManager.horizontalStepDistance / 2;
        }

        var rnd = Random.Range(0, 100f);

        if (trackManager.GetLastSpawnedSegmentIndex() - trackManager.lastBuffSegmentIndex > trackManager.buffInterval)
        {
            if (rnd <= trackManager.buffPercent) //  или нижние объекты
            {

                SpawnClothes(trackManager.buffObjects[Random.Range(0, trackManager.buffObjects.Length)], randomX);
                trackManager.lastBuffSegmentIndex = segmentCountIndex;
                Debug.Log("buff on " + segmentCountIndex);
                return true;

            }
        }
        else if (trackManager.GetLastSpawnedSegmentIndex() - trackManager.lastDebuffSegmentIndex > trackManager.debuffInterval)
        {
            if (rnd <= trackManager.buffPercent + trackManager.debuffPercent) //  или нижние объекты
            {

                SpawnClothes(trackManager.debuffObjects[Random.Range(0, trackManager.debuffObjects.Length)], randomX);
                trackManager.lastDebuffSegmentIndex = segmentCountIndex;
                Debug.Log("debuff on " + segmentCountIndex);
                return true;

            }
        }
        
        return false;
        
    }


    void SpawnRandomObjects()
    {
        ResetSegment();

        //номер сегмента
        segmentCountIndex = trackManager.GetNewSegmentIndexWithIncrement();
        countText.text = segmentCountIndex + "";

        if (trackManager.GetLastSpawnedSegmentIndex() > trackManager.startSpawnObjectIndexDelay)
        {
            //поиск положения
            float randomX;
            if (Random.Range(0, 1f) > 0.5f)
            {
                randomX = trackManager.horizontalStepDistance / 2;
            }
            else
            {
                randomX = -trackManager.horizontalStepDistance / 2;
            }


            var rnd = Random.Range(0, 100f);


            if (trackManager.GetLastSpawnedSegmentIndex() - trackManager.lastBuffSegmentIndex > trackManager.buffInterval)
            {
                if (rnd <= trackManager.buffPercent) //  или нижние объекты
                {

                    SpawnClothes(trackManager.buffObjects[Random.Range(0, trackManager.buffObjects.Length)], randomX);
                    trackManager.lastBuffSegmentIndex = segmentCountIndex;
                    Debug.Log("buff on " + segmentCountIndex);
                    return;

                }
            }
            if (trackManager.GetLastSpawnedSegmentIndex() - trackManager.lastDebuffSegmentIndex > trackManager.debuffInterval)
            {
                if (rnd <= trackManager.buffPercent + trackManager.debuffPercent) //  или нижние объекты
                {

                    SpawnClothes(trackManager.debuffObjects[Random.Range(0, trackManager.debuffObjects.Length)], randomX);
                    trackManager.lastDebuffSegmentIndex = segmentCountIndex;
                    Debug.Log("debuff on " + segmentCountIndex);
                    return;

                }
            }
            rnd = Random.Range(0, 100f);

            if (rnd <= trackManager.upClothPercent) //верхние
            {
                SelectAndSpawnClothes("up", randomX);
            }
            else if (rnd <= trackManager.upClothPercent + trackManager.cloth0Percent) //  или нижние объекты
            {
                SelectAndSpawnClothes("0", randomX);
            }
            else if (rnd <= trackManager.upClothPercent + trackManager.cloth0Percent + trackManager.cloth1Percent) //  или нижние объекты
            {
                SelectAndSpawnClothes("1", randomX);
            }
            else if (rnd <= trackManager.upClothPercent + trackManager.cloth0Percent + trackManager.cloth1Percent + trackManager.cloth2Percent) //  или нижние объекты
            {
                SelectAndSpawnClothes("2", randomX);
            }
            else if (rnd <= trackManager.upClothPercent + trackManager.cloth0Percent + trackManager.cloth1Percent + trackManager.cloth2Percent + trackManager.emptyClothPercent) //пустые
            {
                //пустые объекты
                //Debug.Log("Empty cloth.");
            }

        }


    }
    void SpawnLevels()
    {
        ResetSegment();


        segmentCountIndex = trackManager.GetNewSegmentIndexWithIncrement();
        countText.text = segmentCountIndex + "";


        Debug.Log(gameObject.name + " - " + segmentCountIndex);

        LevelsCollection levelsCollection = trackManager.levelsCollection;

        if (levelsCollection.levelDataDict[GameManager.Instance.LEVELPROGRESS]?.levelTileDatas.Count <= segmentCountIndex)
        {
            Debug.Log("Level Ended." + segmentCountIndex);
            return;
        }


        var currentTiledata = levelsCollection.levelDataDict[GameManager.Instance.LEVELPROGRESS].levelTileDatas[segmentCountIndex];

        if (currentTiledata != null)
        {
            var randomX = trackManager.horizontalStepDistance / 2;

            SelectAndSpawnClothes(currentTiledata.leftSide, -randomX);
            SelectAndSpawnClothes(currentTiledata.rightSide, randomX);
        }


    }
    void SelectAndSpawnClothes(string tileData, float xPos)
    {
        Vector3 newSpawnObjPos;
        switch (tileData)
        {

            case "0":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, -0.6f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.clothes[0], newSpawnObjPos, Quaternion.identity, transform));
                break;
            case "1":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, -0.6f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.clothes[1], newSpawnObjPos, Quaternion.identity, transform));
                break;
            case "2":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, -0.6f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.clothes[2], newSpawnObjPos, Quaternion.identity, transform));
                break;
            case "up":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, trackManager.jumpHeight * 0.75f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.upClothes[Random.Range(0, trackManager.upClothes.Length)], newSpawnObjPos, Quaternion.identity, transform));
                break;
            case "end":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, trackManager.jumpHeight * 0.75f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.endLevelTrigger, newSpawnObjPos, Quaternion.identity, transform));
                break;
            case "heal":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, -0.6f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.buffObjects[0], newSpawnObjPos, Quaternion.identity, transform));
                break;
            case "shield":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, -0.6f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.buffObjects[1], newSpawnObjPos, Quaternion.identity, transform));
                break;
            case "puddle":
                newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, -0.6f, transform.position.z);
                spawnedObjects.Add(Instantiate(trackManager.debuffObjects[0], newSpawnObjPos, Quaternion.identity, transform));
                break;

            default:
                //Debug.Log("empty");
                break;
        }
    }

    void SpawnClothes(GameObject gameObjectToSpawn, float xPos)
    {
        Vector3 newSpawnObjPos;
        newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + xPos, -0.6f, transform.position.z);
        spawnedObjects.Add(Instantiate(gameObjectToSpawn, newSpawnObjPos, Quaternion.identity, transform));
    }
}

