using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Wander : MonoBehaviour
{
    public Vector2 StartingPos;
    public float wanderWidth = 5;
    public float wanderHeight = 5;

    private Rigidbody2D rb;
    public float speed=3 ;
    public Vector2 target;
    private Animator animator;
    [Header("停顿设置")]
    public float pauseDuration = 1;
    public bool isPause;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }
    private void OnEnable()
    {
        StartCoroutine(PauseAndRestart());
    }
    public Vector2 GetRandomTarget()
    {
        float halfWidth = wanderWidth / 2;
        float halfHeight = wanderHeight / 2;
        int edge = Random.Range(0, 4);

        return edge switch
        {
            0 => new Vector2(StartingPos.x - halfWidth, Random.Range(StartingPos.y - halfHeight, StartingPos.y + halfHeight)),
            1 => new Vector2(StartingPos.x + halfWidth, Random.Range(StartingPos.y - halfHeight, StartingPos.y + halfHeight)),
            2 => new Vector2(Random.Range(StartingPos.x - halfWidth, StartingPos.x + halfWidth), StartingPos.y - halfHeight),
            _ => new Vector2(Random.Range(StartingPos.x - halfWidth, StartingPos.x + halfWidth), StartingPos.y + halfHeight)
        };
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        if(isPause)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if(Vector2.Distance(transform.position,target)<.1f)
        {            
            StartCoroutine(PauseAndRestart());
        }
        Vector2 direction = target - new Vector2( transform.position.x, transform.position.y);
        rb.velocity = direction.normalized * speed;
        if (direction.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(PauseAndRestart());
    }
    IEnumerator PauseAndRestart()
    {
        isPause = true;
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(pauseDuration);
        target = GetRandomTarget();
        isPause = false;
        animator.SetBool("isWalking", true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(StartingPos, new Vector3(wanderWidth, wanderHeight, 0));
    }
}
