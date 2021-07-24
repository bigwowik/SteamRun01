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
    public TrackManager trackManager;

    public Transform objectRoot;
    public Transform collectibleTransform;



    [HideInInspector]
    public float[] obstaclePositions;

    public GameObject spawnedObject;

    private void Update()
    {
        if(trackManager.characterController.transform.position.z >= transform.position.z + trackManager.trackSegmentDistance)
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
       

        transform.position += new Vector3(0, 0, trackManager.trackSegmentDistance * trackManager.trackSegmentCount);
        
            SpawnObjects();

    }

    void SpawnObjects()
    {
        if (Vector3.Distance(transform.position, trackManager.characterController.transform.position) > trackManager.startSpawnObjectDistance)
        {
            if (spawnedObject != null)
                Destroy(spawnedObject);
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


            if (rnd <= trackManager.emptyClothPercent) //пустые
            {
                //пустые объекты
                Debug.Log("Empty cloth.");
            }

            if (rnd <= trackManager.upClothPercent) //верхние
            { 
                var newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + randomX, trackManager.jumpHeight * 0.75f, transform.position.z);
                spawnedObject = Instantiate(trackManager.upClothes[Random.Range(0, trackManager.upClothes.Length)], newSpawnObjPos, Quaternion.identity, transform);
            }
            else //  или нижние объекты
            {
                var newSpawnObjPos = new Vector3(trackManager.horizontalStepDistance / 2 + randomX, 0f, transform.position.z);
                spawnedObject = Instantiate(trackManager.clothes[Random.Range(0, trackManager.clothes.Length)], newSpawnObjPos, Quaternion.identity, transform);
            }
        }

        
    }
}
