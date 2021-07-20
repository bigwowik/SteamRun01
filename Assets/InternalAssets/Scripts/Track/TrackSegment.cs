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

        var randomX = Random.Range(-1, +2) * manager.stepDistance;
        var newSpawnObjPos = new Vector3(randomX, 0f, transform.position.z);

        spawnedObject = Instantiate(manager.clothes[Random.Range(0, manager.clothes.Length)], newSpawnObjPos, Quaternion.identity, transform);
    }
}
