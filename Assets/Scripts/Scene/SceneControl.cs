using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour
{
    public static SceneControl Instance {  get; private set; }
    public string SceneToLoad;
    public string SceneToLoadWithoutTrigger;
    public Animator animator;
    public float FadeTime = 0.5f;
    public CanvasGroup LoadCanvasGroup;
    private Transform Player;
    public Vector2 StandardOutPos;
    public Vector2 NewPlayerPos;//切换场景后玩家刷新的位置
    private Vector2 PosOffset;
    public LoadSlot[] loadSlots;
    //public bool isInitialization=false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetLoadCanvasGroup(bool open)
    {
        SaveManager.instance.RefreshAllLoadSlots(loadSlots);
        QuestLogUI.instance.SetCanvasGroupActive(LoadCanvasGroup, open);
    }
    private void OnEnable()
    {
        animator.Play("FadeFromWhite");
    }
    public void OnSceneInitialization()
    {
        // 1. 检查存档是否存在，若不存在则创建默认存档（强制覆盖，不触发确认框）
        if (!SaveManager.instance.HasSaveData("save0"))
        {
            SaveManager.instance.SaveGame("save0", overwrite: true); // 传入 true 跳过覆盖检查
            Debug.Log("未找到存档 save0，已自动创建初始存档");
        }

        // 2. 加载空档
        SaveManager.instance.LoadGame("save0");
        Debug.Log("载入默认存档");
        EnterRPG();
    }
    public void BackToMainMenu()
    {
        MenuManager.instance.ClocsESCCanvas();
        animator.SetBool("Ready", true);
        Debug.Log("返回主菜单");
        StartCoroutine(DelayFade("Ready", false));
    }
    public void EnterRPG(string TargetScene=null)
    {
        MenuManager.instance.ClocsESCCanvas();
        animator.SetBool("Ready", true);
        Debug.Log("触发场景加载初始化");
        if(TargetScene == null)
            StartCoroutine(DelayFade(SceneToLoadWithoutTrigger, false));
        else
            StartCoroutine(DelayFade(TargetScene, false));
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
