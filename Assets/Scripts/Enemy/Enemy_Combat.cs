using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class Enemy_Combat : MonoBehaviour
{
    public int damage = -1;
    public float cooldownTime = 3f;
    [SerializeField]
    private bool CanDamage = true;
    public Transform AttackPoint;
    
    public float AttackRange;
    public LayerMask playerLayer;
    public float Force;
    public float stunTime;
    [SerializeField]
    private bool CanCompat = true;
    // Start is called before the first frame update
    void Start()
    {
        AttackRange = transform.parent.GetComponent<Enemy_Movement>().attackRange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && CanDamage && CanCompat)
        {
            collision.gameObject.GetComponent<PlayerHealth>().HealthChange(damage);
            Debug.Log("发生碰撞，玩家血量-1");
            CanDamage = false;
            StartCoroutine(DamageCoolDown());
        }
    }
    private IEnumerator DamageCoolDown()
    {
        yield return new WaitForSeconds(cooldownTime);
        CanDamage = true;
        //Debug.Log("冷却结束，可以再次造成伤害");
    }
    public void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(AttackPoint.transform.position, AttackRange, playerLayer);
        if(hits.Length > 0 )
        {
            hits[0].GetComponent<PlayerHealth>().HealthChange(damage);
            hits[0].GetComponent<PlayerMovement>().KnockBack(transform,Force,stunTime);
        }
    }
}
