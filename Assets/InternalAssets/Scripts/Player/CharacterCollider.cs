using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollider : MonoBehaviour
{
    public PlayerMovement playerMovement;
    protected void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Cloth"))
        {
            playerMovement.interactiveCollider = c.gameObject;
            Debug.Log("Cloth was trigger entered.");
            
        }
    }
    protected void OnTriggerExit(Collider c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Cloth"))
        {

            playerMovement.interactiveCollider = null;
            Debug.Log("Cloth was trigger exited.");

        }
    }
}
