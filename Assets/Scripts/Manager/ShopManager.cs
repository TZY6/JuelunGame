using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class ShopManager : MonoBehaviour
{
    public static ShopManager instance { get; private set; }
    public List<ShopItem> AciveItems = new List<ShopItem>();
    public Button ShowItemButton;
    public Button ShowWeapnButton;
    public Button ShowDefenButton;
    public ShopKeeper DefaultShopKeeper;
    public ShopKeeper currentShopKeeper;
    [SerializeField] private ShopSlot[] shopSlots;
    // Start is called before the first frame update
    public void FillShopSlots(List<ShopItem> shopItems)
    {
        AciveItems = shopItems;
        for (int i = 0; i < shopSlots.Length && i<shopItems.Count; i++)
        {
            ShopItem shopitem = shopItems[i];
            shopSlots[i].Initialize(shopitem.shopItemSO, shopitem.price);
            shopSlots[i].gameObject.SetActive(true);
        }
        for (int i = shopItems.Count; i < shopSlots.Length;i++)
        {
            shopSlots[i].gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        //if(GetactiveShop!=null)
        ShopKeeper.onShopStateChanged += SetChangeButton;

    }
    private void OnDisable()
    {
        ShopKeeper.onShopStateChanged -= SetChangeButton;
    }
    public void SetChangeButton(ShopKeeper activeShop, bool isShopping)
    {
        currentShopKeeper = activeShop;
        if (activeShop == null)
        {
            activeShop = DefaultShopKeeper;
        }
        else
        {
            ShowItemButton.onClick.AddListener(activeShop.showItemShop);
            ShowWeapnButton.onClick.AddListener(activeShop.showWeaponShop);
            ShowDefenButton.onClick.AddListener(activeShop.showDefenShop);
        }
            
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void SellItem(ItemSO itemSO)
    {
        if (itemSO == null)
            return;
        foreach(var slot in AciveItems)
        {
            if(slot.shopItemSO == itemSO)
            {
                DataManager.instance.GoldAmount += slot.price;
                InventoryManager.Instance.Gold_Text.text = DataManager.instance.GoldAmount.ToString();
                return;
            }
        }
    }
    public void TryBuyItem(ItemSO itemSO,int pirce)
    {
        //如果是武器类型商品的话
        if(itemSO != null && DataManager.instance.GoldAmount >= pirce && itemSO.isWeapon)
        {
            //首先判断是否与当前武器一致
            if(DataManager.instance.CurrentWeapon == itemSO.ItemName)
            {
                //一致的话，给出重复提示
                NoticeManager.instance.ShowNotice("与当前使用武器状态一致");
                return;
            }
            else
            {                
                DataManager.instance.GoldAmount -= pirce;
                DataManager.instance.CurrentWeapon = itemSO.ItemName;
                Player_DataUI.instance.UpdateWeapon();
                NoticeManager.instance.ShowNotice("购买成功，武器已替换");
            }
                
        }
        //如果不是武器类型商品
        else if(itemSO !=  null && DataManager.instance.GoldAmount>=pirce)
        {
            if(HasSpaceForItem(itemSO))
            {

                DataManager.instance.GoldAmount -= pirce;
                InventoryManager.Instance.Gold_Text.text = DataManager.instance.GoldAmount.ToString();
                InventoryManager.Instance.AddItem(itemSO, 1);
                NoticeManager.instance.ShowNotice("购买成功");
            }
            else
            {
                NoticeManager.instance.ShowNotice("请清理仓库槽位");
            }
        }
        else
        {
            NoticeManager.instance.ShowNotice("金币不足");
        }
    }
    private bool HasSpaceForItem(ItemSO itemSO)
    {
        foreach(var slot in InventoryManager.Instance.inventorySlots)
        {
            if(slot.ItemSO == itemSO && slot.quantity< itemSO.maxAmountPerPlot)
            {
                return true;
            }
            else if (slot.ItemSO == null)
            {
                return true;
            }
            
        }
        return false;
    }
}

