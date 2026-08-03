using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountai_Entry : MonoBehaviour
{
    public Collider2D[] MountainColliders;
    public Collider2D[] BoundaryColliders;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            foreach (Collider2D MountainCollider in MountainColliders)
            {
                MountainCollider.enabled = false;
            }
            foreach (Collider2D BoundaryCollider in BoundaryColliders)
            {
                BoundaryCollider.enabled = true;
            }
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 15;
        }
       
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
