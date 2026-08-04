using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour
{
    public string SceneToLoad;
    public string SceneToLoadWithoutTrigger;
    public Animator animator;
    public float FadeTime = 0.5f;
    private Transform Player;
    public Vector2 StandardOutPos;
    public Vector2 NewPlayerPos;//切换场景后玩家刷新的位置
    private Vector2 PosOffset;
    //public bool isInitialization=false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnEnable()
    {
        animator.Play("FadeFromWhite");
    }
    public void OnSceneInitialization()
    {
        MenuManager.instance.ClocsESCCanvas();
        animator.SetBool("Ready", true);
        Debug.Log("触发场景加载初始化");
        StartCoroutine(DelayFade(SceneToLoadWithoutTrigger, false));
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player = collision.gameObject.GetComponent<Transform>();
            PosOffset = new Vector2(Player.position.x - StandardOutPos.x, Player.position.y - StandardOutPos.y);
            animator.Play("FadeCanvas");
            StartCoroutine(DelayFade(SceneToLoad, true));
        }
    }
    IEnumerator DelayFade(string targetScene, bool isTrigger)
    {
        yield return new WaitForSeconds(FadeTime);
        if (isTrigger)
            Player.position = NewPlayerPos + PosOffset;
        SceneManager.LoadScene(targetScene);
    }
}
