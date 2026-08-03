using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Player_DataUI : MonoBehaviour
{
    public static Player_DataUI instance { get; private set; }
    public GameObject[] statusUI;
    public CanvasGroup CanvasGroup;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Updatedamage()
    {
        statusUI[0].GetComponentInChildren<TMP_Text>().text = "伤害:" + DataManager.instance.damage;
    }
    public void Updatespeed()
    {
        statusUI[1].GetComponentInChildren<TMP_Text>().text = "速度:" + DataManager.instance.speed;
    }
    public void Updatehealth()
    {
        statusUI[2].GetComponentInChildren<TMP_Text>().text = "最大生命:" + DataManager.instance.maxHealth;
    }
    public void UpdateWeapon()
    {
        statusUI[3].GetComponentInChildren<TMP_Text>().text = "武器:" + DataManager.instance.CurrentWeapon;
    }
    private void Start()
    {
        UpdateAllStatus();
    }
    private void Update()
    {
        
    }
    public void UpdateAllStatus()//更新属性控制中的所有属性
    {
        Updatedamage();
        Updatespeed();
        Updatehealth();
    }
}
