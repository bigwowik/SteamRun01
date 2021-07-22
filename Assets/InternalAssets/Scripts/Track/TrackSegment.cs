using UnityEngine;
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
    public TrackManager manager;

    public Transform objectRoot;
    public Transform collectibleTransform;



    [HideInInspector]
    public float[] obstaclePositions;

    public GameObject spawnedObject;

    private void Update()
    {
        if(manager.characterController.transform.position.z >= transform.position.z + manager.trackSegmentDistance)
        {
            NewSpawn();
        }
    }

    private void Start()
    {
        SpawnObjects();
    }



    void NewSpawn()
    {
       

        transform.position += new Vector3(0, 0, manager.trackSegmentDistance * manager.trackSegmentCount);
        SpawnObjects();

    }

    void SpawnObjects()
    {
        if (spawnedObject != null)
            Destroy(spawnedObject);
        float randomX;
        if(Random.Range(0,1f) > 0.5f)
        {
            randomX = manager.stepDistance/2;
        }
        else
        {
            randomX = -manager.stepDistance/2;
        }
        
        

        if (Utilities.BoolWithChance(manager.upClothPercent)) //верхние
        {
            var newSpawnObjPos = new Vector3(manager.stepDistance/2 + randomX, manager.jumpHeight * 0.75f, transform.position.z);
            spawnedObject = Instantiate(manager.upClothes[Random.Range(0, manager.upClothes.Length)], newSpawnObjPos, Quaternion.identity, transform);
        }
        else //  или нижние объекты
        {
            var newSpawnObjPos = new Vector3(manager.stepDistance / 2 + randomX, 0f , transform.position.z);
            spawnedObject = Instantiate(manager.clothes[Random.Range(0, manager.clothes.Length)], newSpawnObjPos, Quaternion.identity, transform);
        }

        
    }
}
