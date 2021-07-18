using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollider : MonoBehaviour
{
    protected void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Cloth"))
        {
            Debug.Log("Cloth was trigger entered.");
            
        }
    }
}
