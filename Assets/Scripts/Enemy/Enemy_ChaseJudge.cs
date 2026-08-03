using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ChaseJudge : MonoBehaviour
{
    private Enemy_Movement parentMovement;
    // Start is called before the first frame update
    void Awake()
    {
        parentMovement = GetComponentInParent<Enemy_Movement>();
        if (parentMovement == null)
            Debug.LogError("Enemy_Movement component not found on parent", this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (parentMovement == null) return;
        GameObject player = collision.gameObject;
        parentMovement.OnTriggerEnterDIY(collision, player, true);
        //Debug.Log("Åö×²½øÈë");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (parentMovement == null) return;
        GameObject player = collision.gameObject;
        parentMovement.OnTriggerExitDIY(collision, false);
        //Debug.Log("Åö×²½áÊø");
    }

}

