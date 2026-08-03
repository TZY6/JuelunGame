using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountain_Exit : MonoBehaviour
{
    public Collider2D[] MountainColliders;
    public Collider2D[] BoundaryColliders;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            foreach (Collider2D MountainCollider in MountainColliders)
            {
                MountainCollider.enabled = true;
            }
            foreach (Collider2D BoundaryCollider in BoundaryColliders)
            {
                BoundaryCollider.enabled = false;
            }
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }

    }
    public void Start()
    {
        
    }
}
