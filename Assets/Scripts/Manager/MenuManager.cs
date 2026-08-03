using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance { get; private set; }
    public CanvasGroup menuCanvasGroup;
    public GameObject ButtonToOpen;
    [SerializeField]private CanvasGroup[] Managers;
    public GameObject[] MenuBars;
    public int currentManagerNumber = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CurrentBar(GameObject obj = null)
    {
        if(obj == MenuBars[3])
            QuestLogUI.instance.ShowQuestOffer(QuestLogUI.instance.currentCheckQuest);
        if (obj == null)
        {
            foreach(var manager in Managers)
            {
                manager.alpha = 0;
                manager.interactable = false;
                manager.blocksRaycasts = false;
            }
            return;
        }
        ResetManagers();
        //Debug.Log($"Managers[0].name+{Managers[0].name}");
        Player_DataUI.instance.UpdateAllStatus();
        currentManagerNumber = obj.GetComponent<MenuBarCount>().count;

        for (int i = 0; i < Managers.Length; i++)
        {
            // ✅ 在循环开头统一过滤掉无效引用
            if (Managers[i] == null)
                continue;

            if (i == currentManagerNumber - 1)
            {
                Managers[i].alpha = 1;
                Managers[i].blocksRaycasts = true;
                Managers[i].interactable = true;
            }
            else
            {
                Managers[i].alpha = 0;
                Managers[i].blocksRaycasts = false;
                Managers[i].interactable = false;
            }
        }
        foreach (var Bar in MenuBars)
        {
            if(Bar.GetComponent<MenuBarCount>().count == currentManagerNumber)
            {
                Bar.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);                
            }
            else
            {
                Bar.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);               
            }

        }
    }
    

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Update");
        //if(Input.GetKeyDown(KeyCode.F10))
        if (Input.GetButtonDown("OpenBag"))
        {
            
            //Debug.Log("Notice按键按下");
            if (TimeScaleManager.Instance.currentOnUI==0)
            {
                if (Time.timeScale == 0)
                    return;
                CurrentBar(MenuBars[0]);      
                TimeScaleManager.Instance.StopTimeScale();
            }
            else
            {
                CurrentBar();
                TimeScaleManager.Instance.StartTimeScale();
            }

        }
        
    }
    [ContextMenu("重新指认managers")]
    public void ResetManagers()
    {
        int CanvasAmount = ButtonToOpen.transform.childCount;
        for (int i = 0; i < CanvasAmount; i++)
        {
            Managers[i] = ButtonToOpen.transform.GetChild(i).GetComponent<CanvasGroup>();
        }
    }
    private void Awake()
    {
        ResetManagers();
        // 单例初始化（与 TimeScaleManager 一致）
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 如果未在 Inspector 中赋值，尝试自动获取
        if (menuCanvasGroup == null)
        {
            menuCanvasGroup = GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                Debug.LogError("MenuManager: 找不到 CanvasGroup 组件，请在 Inspector 中手动赋值！");
            }
        }
        //if (Managers[0]==null)
        //{
        //    int CanvasAmount = ButtonToOpen.transform.childCount;
        //    for(int i = 0; i < CanvasAmount; i++)
        //    {
        //        Managers[i] = ButtonToOpen.transform.GetChild(i).GetComponent<CanvasGroup>();
        //    }
        //}
    }
}
