using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Bow : MonoBehaviour
{
    public Transform BowPoint;
    public GameObject arrowPrefab;
    public Vector2 AimDirection;
    public float ArrowTimer;
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(ArrowTimer > 0)
        {
            ArrowTimer -= Time.deltaTime;
        }
        OnHandleAiming();
        if (Input.GetButtonDown("Attack") )
        //if (Input.GetButtonDown("Arrow") && (ArrowTimer <= 0))
        {
            animator.SetBool("isShooting", true);
            //Shoot();
            Debug.Log("射出一发箭矢");            
        }
        if (animator.GetBool("isShooting"))
        {
            PlayerMovement.Instance.enabled = false;
            PlayerMovement.Instance.rb.velocity = Vector2.zero;//攻击过程中霸体
        }
        else
        {
            PlayerMovement.Instance.enabled = true;
        }
    }
    public void OnEnable()
    {
        animator.SetLayerWeight(0, 0);
        animator.SetLayerWeight(1, 1);
    }
    public void OnDisable()
    {
        animator.SetLayerWeight(0, 1);
        animator.SetLayerWeight(1, 0);
    }
    public void OnHandleAiming()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        if(horizontal!=0 ||  vertical!=0)
        {
            AimDirection  = new Vector2 (horizontal, vertical).normalized;
        }
        else
        {
            AimDirection = new Vector2( transform.localScale.x,0);
        }
        animator.SetFloat("aimX", AimDirection.x);
        animator.SetFloat("aimY", AimDirection.y);
    }
    public void Shoot()
    {
        
            //Debug.Log("射出一发箭矢");
            Arrow arrow = Instantiate(arrowPrefab, BowPoint.position, Quaternion.identity).GetComponent<Arrow>();
            //arrow.transform.SetParent(BowPoint, false);
            arrow.ArrowDirection = AimDirection;
            //Destroy(gameObject, DataManager.instance.ArrowLife);
            ArrowTimer = DataManager.instance.ArrowCoolDown;
            animator.SetBool("isShooting",false);
        

    }
}
