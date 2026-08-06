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
    public InventoryData GetSaveData()
    {
        InventoryData data = new InventoryData();

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.ItemSO != null && slot.quantity > 0)
            {
                ItemSlotData itemData = new ItemSlotData();

                itemData.itemID = slot.ItemSO.ItemID;
                itemData.quantity = slot.quantity;

                data.items.Add(itemData);
            }
        }

        return data;
    }
    public void LoadData(InventoryData data)
    {
        if (data == null)
        {
            Debug.LogWarning("InventoryData 为空，无法加载");
            return;
        }

        // 1. 清空所有槽位
        foreach (InventorySlot slot in inventorySlots)
        {
            slot.ItemSO = null;
            slot.quantity = 0;
            slot.UpdateUI();
        }

        // 2. 检查数据库是否就绪
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase.Instance 为 null，无法加载物品数据");
            return;
        }

        // 3. 加载物品
        int loadedCount = 0;
        foreach (ItemSlotData itemData in data.items)
        {
            if (itemData == null || string.IsNullOrEmpty(itemData.itemID))
            {
                Debug.LogWarning("跳过无效的物品数据");
                continue;
            }

            ItemSO item = ItemDatabase.Instance.GetItemByID(itemData.itemID);
            if (item == null)
            {
                Debug.LogWarning($"未找到物品 ID: {itemData.itemID}，跳过");
                continue;
            }

            // 按顺序放入第一个空槽
            bool placed = false;
            for (int i = 0; i < inventorySlots.Length; i++) // 如果是数组用 Length，List 用 Count
            {
                InventorySlot slot = inventorySlots[i];
                if (slot.ItemSO == null)
                {
                    slot.ItemSO = item;
                    slot.quantity = Mathf.Max(1, itemData.quantity);
                    slot.UpdateUI();
                    Debug.Log($"成功加载物品 {item.name}，数量 {slot.quantity} 到槽位 {i}");
                    placed = true;
                    loadedCount++;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"背包已满，无法放置物品 {item.name}（ID: {itemData.itemID}）");
            }
        }

        Debug.Log($"Inventory 加载完成，共加载 {loadedCount} 个物品（存档共有 {data.items.Count} 条记录）");
    }
}

[System.Serializable]
public class InventoryData
{
    public List<ItemSlotData> items = new List<ItemSlotData>();
}

[System.Serializable]
public class ItemSlotData
{
    public string itemID;
    public int quantity;
}