using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="New Item")]
public class ItemSO : ScriptableObject
{
    // Start is called before the first frame update
    public string ItemName;
    [TextArea] public string ItemDescirbe;
    public Sprite icon;
    public bool isGold;
    public bool isExperience;
    public bool isWeapon;
    [Header("物品属性")]
    public int damage;
    public int MaxHealthUp;
    public int CurrentHealthUp;
    public int speedUp;
    [Header("单格最大存储数")]
    public int maxAmountPerPlot;
    [Header("持续时间")]
    public float duration;
}
