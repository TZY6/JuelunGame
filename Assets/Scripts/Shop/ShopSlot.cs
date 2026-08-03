using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerMoveHandler
{
    public ItemSO itemSO;
    public TMP_Text itemName;
    public TMP_Text priceText;
    //public TMP_Text descriptionText;
    public Image itemImage;
    private int price;
    [SerializeField]private ShopInfo shopInfo;
    [SerializeField]private ShopManager shopManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Initialize(ItemSO item , int price)
    {
        itemSO = item;
        itemName.text = item.ItemName;
        this.price = price;
        priceText.text = price.ToString();
        //descriptionText.text = item.ItemDescirbe;
        itemImage.sprite = item.icon;
    }
    public void OnButtonClicked()
    {
        shopManager.TryBuyItem(itemSO,price);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemSO != null)
        {
            shopInfo.ShowItemInfo(itemSO);
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        shopInfo.HideItemInfo();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (itemSO != null)
        {
            shopInfo.InfoFollowMouse();
        }

    }
}
