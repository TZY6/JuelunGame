using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public List<ItemSO> items;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ItemSO GetItemByID(string id)
    {
        foreach (ItemSO item in items)
        {
            if (item.ItemID == id)
                return item;
        }

        return null;
    }
}
