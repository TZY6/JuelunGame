using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
#if UNITY_EDITOR
[CustomEditor(typeof(NPC_Walkaround))]
public class NPC_WalkaroundEditor:Editor
{
    private SerializedProperty pauseTimeProp;
    private void OnEnable()
    {
        pauseTimeProp = serializedObject.FindProperty("PauseTime");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(pauseTimeProp, new GUIContent("暂停时间"));
        // 绘制除了 PauseTime 以外的所有属性（保持默认英文）
        DrawPropertiesExcluding(serializedObject, "PauseTime");
        
        serializedObject.ApplyModifiedProperties();
    }
}

#endif
public class NPC_Walkaround : MonoBehaviour
{
    public List<Vector2> patrolPoints = new List<Vector2>();
    public float PauseTime = 1.5f;
    public int currentPatrolTarget = 0;
    public Vector2 target;
    public float speed = 2f;
    private Rigidbody2D rb;
    private Animator animator;
    public bool isPause= false;
    public bool isWaiting=false;
    // Start is called before the first frame update
    void Start()
    {
        if(rb == null)
          rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        target = patrolPoints[currentPatrolTarget];
    }

    // Update is called once per frame
    void Update()
    {
        if (isPause)
        {
            SetVelocity(false);
            return;
        }
        Vector2 direction = (Vector3)target - transform.position;
        SetVelocity(true);
        if (direction.x > 0 && transform.localScale.x<0)
            transform.localScale =new Vector3 (transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        else if(direction.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        if (direction.magnitude < .1f && !isWaiting)
        {
            animator.SetBool("isWalking", false);
            StartCoroutine(NextPatrolTarget());
        }

    }
    public void SetVelocity(bool isWalking)
    {
        Vector2 direction = (Vector3)target - transform.position;
        if (isWalking)
        {
            rb.velocity = direction.normalized * speed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

    }
    IEnumerator NextPatrolTarget()
    {
        isWaiting = true;
        isPause = true;
        if (currentPatrolTarget< patrolPoints.Count-1)
        {
            currentPatrolTarget++;
        }
        else
        {
            currentPatrolTarget = 0;
        }
        rb.velocity = new Vector2(0,0);        
        target = patrolPoints[currentPatrolTarget];        
        yield return new WaitForSeconds(PauseTime);
        isPause = false;
        animator.SetBool("isWalking", true);
        isWaiting = false;
    }
}
