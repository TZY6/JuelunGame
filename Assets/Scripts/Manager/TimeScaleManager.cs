using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    public static TimeScaleManager Instance { get; private set; }
    public  int currentOnUI = 0;
    public   int currentOnShop = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 可选：场景切换不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StopTimeScale(bool isShop = false)
    {
        if(!isShop) //如果是非商店UI
        {
            currentOnUI++;
        }
        else //如果是商店UI
        {
            currentOnShop++;
        }
            
        CheckCurrentOnUI();
    }
    public void StartTimeScale(bool isShop = false)
    {
        if (!isShop) //如果是非商店UI
        {
            currentOnUI--;
        }
        else //如果是商店UI
        {
            currentOnShop--;
        }
        CheckCurrentOnUI();
    }
    public void CheckCurrentOnUI()
    {
        // 控制菜单显示：仅在 currentOnUI 不为 0 时显示
        bool showMenu = (currentOnUI != 0);
        MenuManager.instance.menuCanvasGroup.alpha = showMenu ? 1 : 0;
        MenuManager.instance.menuCanvasGroup.interactable = showMenu;
        MenuManager.instance.menuCanvasGroup.blocksRaycasts = showMenu;

        // 控制时间暂停：只要 currentOnShop 或 currentOnUI 任一不为 0 则暂停
        bool shouldPause = (currentOnShop != 0 || currentOnUI != 0);
        if (shouldPause)
        {
            Time.timeScale = 0f;
            Debug.Log("时间暂停");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("时间恢复流逝");
        }
    }
}
