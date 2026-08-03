using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Arrow : MonoBehaviour
{
    public Rigidbody2D rb;
    public Vector2 ArrowDirection = Vector2.right;
    public LayerMask EnemyLayer ;
    public LayerMask ObstaclerLayer ;
    public Sprite buriedSprite;
    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = ArrowDirection * DataManager.instance.ArrowSpeed;
        RotateArrow();
        Destroy(gameObject,DataManager.instance.ArrowLife);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RotateArrow()
    {
        float angle = Mathf.Atan2(ArrowDirection.y, ArrowDirection.x) * Mathf.Rad2Deg;//先获取弧度，再获取实际可用的角度
        transform.rotation = Quaternion.Euler(new Vector3 (0,0,angle));
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag!="Player")
        {
            rb.velocity = new Vector2(0, 0);
            transform.SetParent(collision.transform, true);
            GetComponent<BoxCollider2D>().isTrigger = true;
            if ((EnemyLayer & (1 << collision.gameObject.layer)) != 0)
            {
                Debug.Log("击中敌人");
                collision.gameObject.GetComponent<Enemy_Health>().HealthChange(DataManager.instance.ArrowHurt);
                collision.gameObject.GetComponent<Enemy_Movement>().EnemyKonckedBack(transform, DataManager.instance.ArrowBackForce, DataManager.instance.ArrowBackTime, DataManager.instance.ArrowBackStunTime);
            }
            else if ((ObstaclerLayer & (1 << collision.gameObject.layer)) != 0)
            {
                Debug.Log("击中障碍物");
            }
            GetComponent<SpriteRenderer>().sprite = buriedSprite;
        }
        


    }
    
}
