using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get;private set; }
    //public float speed = 5;
    
    public Rigidbody2D rb;
    [SerializeField]
    private Animator animator;
    public int FacingDirection = 1;
    [SerializeField]
    private bool isKnockback;
    public Player_Combat Player_Combat;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator =GetComponent<Animator>();
    }
    private void Update()
    {
        if(Player_Combat.enabled)
        {
            if (Input.GetButtonDown("Attack"))
            {
                Player_Combat.PlayerAttack();
            }
            if (Input.GetButtonDown("ICE") && DataManager.instance.isAbleToPlayICE)
            {
                Player_Combat.PlayerMagic("ICE");
            }
            if (Input.GetButtonDown("FIRE") && DataManager.instance.isAbleToPlayFIRE)
            {
                Player_Combat.PlayerMagic("FIRE");
            }
            if (Input.GetButtonDown("THUNDER") && DataManager.instance.isAbleToPlayTHUNDER)
            {
                Player_Combat.PlayerMagic("THUNDER");
            }
        }

        
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!isKnockback)
        {
            float horizontal = Input.GetAxis("Horizontal");
            if (horizontal < 0 && FacingDirection > 0 || horizontal > 0 && FacingDirection < 0)
            {
                FacingDirection = FacingDirection * (-1);
                transform.localScale = new Vector3(FacingDirection, 1, 1);
            }
            float vertical = Input.GetAxis("Vertical");

            animator.SetFloat("horizontal", Mathf.Abs(horizontal));
            animator.SetFloat("vertical", Mathf.Abs(vertical));

            rb.velocity = new Vector2(horizontal, vertical) * DataManager.instance.speed;
        }
        
    }
    public void KnockBack(Transform enemy,float force,float stunTime)
    {
        isKnockback = true;
        Vector2 direction = (transform.position - enemy.position).normalized;
        rb.velocity = direction * force;
        StartCoroutine(KnockBackConter(stunTime));
    }
    IEnumerator KnockBackConter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.velocity = Vector2.zero;
        isKnockback = false;
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
