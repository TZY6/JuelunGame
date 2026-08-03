using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.EventSystems  
public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public ItemSO ItemSO;
    public int quantity;
    public Image itemIcon;
    public TMP_Text amountText;
    private static ShopKeeper GetactiveShop;
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
        //if(GetactiveShop!=null)
        ShopKeeper.onShopStateChanged += HandleShopStateChanged;
        
    }
    private void OnDisable()
    {
        ShopKeeper.onShopStateChanged -= HandleShopStateChanged;
    }
    public void UpdateUI()
    {
        if (quantity <= 0)
        {
            ItemSO = null;
        }
        if (ItemSO!=null)
        {
            itemIcon.sprite = ItemSO.icon;
            itemIcon.gameObject.SetActive(true);
            amountText.text = quantity.ToString();
        }
        else
        {
            amountText.text = "";
            itemIcon.gameObject.SetActive (false);
        }
        
    }
    private void OnValidate()
    {
        if(ItemSO!=null)
        {
            gameObject.name = ItemSO.ItemName + "_物品栏";
            itemIcon.sprite = ItemSO.icon;
        }
        
        
    }
    private void HandleShopStateChanged(ShopKeeper activeShop,bool isShopping)
    {
        GetactiveShop = isShopping ? activeShop : null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(quantity>0)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if(GetactiveShop!=null)//如果处于购物状态，左键出售
                {
                    ShopManager.instance.SellItem(ItemSO);
                    quantity--;
                    UpdateUI();
                }
                else//如果不处于购物状态，左键使用
                {
                    InventoryManager.Instance.UseItem(this);
                    Debug.Log("使用了"+ ItemSO);
                }
                    
            }
            if(eventData.button== PointerEventData.InputButton.Right)
            {
                InventoryManager.Instance.DropLoot(ItemSO, 1);
                quantity--;
                if(quantity<=0)
                {
                    ItemSO = null;
                    quantity = 0;
                }
                UpdateUI();
            }
        }
    }
}
