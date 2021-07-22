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
            //Debug.Log("Cloth was trigger entered.");
            
        }
    }
    protected void OnTriggerExit(Collider c)
    {
        if (c.gameObject.layer == LayerMask.NameToLayer("Cloth"))
        {
            CheckClothes(c.GetComponent<ClothInteractive>());
            playerMovement.interactiveCollider = null;
            //Debug.Log("Cloth was trigger exited.");

        }
    }

    void CheckClothes(ClothInteractive clothObj)
    {

        //Debug.Log("TapCloth");
        switch (clothObj.CheckState(gameObject))
        {
            case ClothState.NotReady:
                break;
            case ClothState.Ready:
                clothObj.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                playerMovement.trackManager.UpScore(1);
                break;
            case ClothState.BadCondition:
                clothObj.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                playerMovement.trackManager.UpScore(-1);
                clothObj.GetComponent<Collider>().enabled = false;
                break;
        }
    }
}