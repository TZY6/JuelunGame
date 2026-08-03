using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour
{
    public string SceneToLoad;
    public Animator animator;
    public float FadeTime = 0.5f;
    private Transform Player;
    public Vector2 StandardOutPos;
    public Vector2 NewPlayerPos;//切换场景后玩家刷新的位置
    private Vector2 PosOffset;
    public bool isInitialization=false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSceneInitialization()
    {
        animator.Play("FadeCanvas");
        StartCoroutine(DelayFade());
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Player = collision.gameObject.GetComponent<Transform>();
            PosOffset = new Vector2(Player.position.x - StandardOutPos.x, Player.position.y-StandardOutPos.y);
            animator.Play("FadeCanvas");
            StartCoroutine(DelayFade());
        }
    }
    IEnumerator DelayFade()
    {
        yield return new WaitForSeconds(FadeTime);
        if(!isInitialization)
            Player.position = NewPlayerPos+PosOffset;
        SceneManager.LoadScene(SceneToLoad);
    }
}
