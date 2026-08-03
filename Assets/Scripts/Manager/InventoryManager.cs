using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public TMP_Text Gold_Text;
    public GameObject LootPrefab;
    public GameObject Player;
    public static InventoryManager Instance { get; private set; }
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        //Gold_Text.text = DataManager.instance.GoldAmount.ToString();//首次初始化
    }
    private void OnEnable()
    {
        if(DataManager.instance!=null)
            Gold_Text.text = DataManager.instance.GoldAmount.ToString();//每当InventoryManager脚本激活时同步
        Loot.OnItemLooted += AddItem;
    }
    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
    }
    public void AddItem(ItemSO item,int amount)
    {
        if(item.isGold)
        {
            DataManager.instance.GoldAmount += amount;
            Gold_Text.text = DataManager.instance.GoldAmount.ToString();
            return;
        }
        if(item.isExperience)
        {
            ExpManager.instance.GetExperience(amount);
            return;
        }
        else
        {
            foreach(var slot in inventorySlots)
            {
                if( slot.ItemSO == item && slot.quantity < item.maxAmountPerPlot) //如果已经有槽位是该物品
                {
                    int availableSpace = item.maxAmountPerPlot - slot.quantity;
                    int amountToAdd = Mathf.Min(availableSpace, amount);
                    slot.quantity += amountToAdd;
                    amount -= amountToAdd;
                    slot.UpdateUI();
                    if (amount <= 0)    //放完了
                    {
                        return;
                    }                   

                }

            }

            foreach (var slot in inventorySlots)
            {
                if (slot.ItemSO == null)
                {
                    int toPlace = Mathf.Min(amount, item.maxAmountPerPlot);
                    slot.ItemSO = item;
                    slot.quantity = toPlace;
                    amount -= toPlace;
                    slot.UpdateUI();
                    if (amount <= 0) return;
                }
            }
            if(amount > 0)
            {
                DropLoot(item, amount);
            }
        }
    }
    public void DropLoot(ItemSO item, int amount)
    {
        GameObject DroppedLoot =  Instantiate(LootPrefab, Player.transform.position,Quaternion.identity);
        DroppedLoot.GetComponent<Loot>().Initialize(item, amount);
        
    }
    public void UseItem(InventorySlot slot)
    {
        
        if(slot.ItemSO != null && slot.quantity >0)
        {
            Debug.Log("尝试使用:" + slot.ItemSO.ItemName);
            slot.quantity--;
            CheckIfStatusChange(slot);
            if(slot.quantity <=0)
            {
                slot.ItemSO=null;   
            }
            slot.UpdateUI() ;
        }
    }
    public void CheckIfStatusChange(InventorySlot slot)
    {
        if(slot.ItemSO.MaxHealthUp > 0)
        {
            DataManager.instance.UpdateMaxHealth(slot.ItemSO.MaxHealthUp);
        }
        if (slot.ItemSO.CurrentHealthUp > 0)
        {
            if(DataManager.instance.currentHealth>=DataManager.instance.maxHealth)
            {

            }
            else
            {
                DataManager.instance.UpdateCurrentHealth(slot.ItemSO.CurrentHealthUp);
            }
               
        }
        if (slot.ItemSO.speedUp > 0)
        {
            DataManager.instance.UpdateSpeed(slot.ItemSO.speedUp);
        }
        if(slot.ItemSO.duration > 0)
        {
            StartCoroutine(EffectTimer(slot.ItemSO ,slot.ItemSO.duration));
        }

    }
    private IEnumerator EffectTimer(ItemSO item, float duration)//恢复增加的生命值上线和速度，当前生命值不回退
    {
        yield return new WaitForSeconds(duration);
        if (item.MaxHealthUp > 0)
        {
            DataManager.instance.UpdateMaxHealth(-item.MaxHealthUp);
            if(DataManager.instance.currentHealth>=DataManager.instance.maxHealth)
            {
                DataManager.instance.currentHealth = DataManager.instance.maxHealth;
            }
        }
        
        if (item.speedUp > 0)
        {
            DataManager.instance.UpdateSpeed(-item.speedUp);
        }
    }
    public bool HasItem(ItemSO itemSO)
    {
        foreach(var slot in inventorySlots)
        {
            if(slot.ItemSO == itemSO && slot.quantity >0)
            {
                return true;
            }
        }
        return false;
    }
    public int GetItemCount(ItemSO itemSO)
    {
        int count = 0;
        if(HasItem(itemSO))
        {
            foreach (var slot in inventorySlots)
            {
                if(slot.ItemSO == itemSO )
                {
                    count += slot.quantity;
                }
            }
            return count;
        }
        else
        {
            return 0;
        }
    }
}
