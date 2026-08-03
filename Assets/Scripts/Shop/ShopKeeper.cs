using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    public Animator IConAnim;
    //public Collider2D ShopKeerper;
    public delegate void OnShopStateChanged(ShopKeeper activeShop, bool isShopping);
    public static event OnShopStateChanged onShopStateChanged;
    [SerializeField] private List<ShopItem> shopItems;
    [SerializeField] private List<ShopItem> shopWeapon;
    [SerializeField] private List<ShopItem> shopDefen;
    public CanvasGroup ShopCanvasGroup;
    private bool isPlayerNear;
    public bool isShopOpen = false;
    [Header("用于显示商店激活时左侧的店主预览画面")]
    public Camera NpcCamera;//用于显示商店激活时左侧的店主预览画面
    [SerializeField]private Vector3 cameraOffset = new Vector3(0,0,-1);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(isPlayerNear)
        {
            if(Input.GetButtonDown("Interact"))
            {
                if(!isShopOpen)
                {
                    isShopOpen = true;
                    ShopCanvasGroup.alpha = 1;
                    ShopCanvasGroup.interactable = true;
                    onShopStateChanged?.Invoke(this, true);
                    NpcCamera.gameObject.SetActive(true);
                    NpcCamera.transform.position = transform.position + cameraOffset;
                    TimeScaleManager.Instance.StopTimeScale(true);
                    ShopCanvasGroup.blocksRaycasts = true;
                    showItemShop();
                }
                else
                {
                    isShopOpen = false;
                    ShopCanvasGroup.alpha = 0;
                    ShopCanvasGroup.interactable = false;
                    onShopStateChanged?.Invoke(this, false);
                    NpcCamera.gameObject.SetActive(false);
                    TimeScaleManager.Instance.StartTimeScale(true);
                    ShopCanvasGroup.blocksRaycasts = false;
                }
            }
        }
    }
    public void showItemShop()
    {
        ShopManager.instance.FillShopSlots(shopItems);
    }
    public void showWeaponShop()
    {
        ShopManager.instance.FillShopSlots(shopWeapon);
    }
    public void showDefenShop()
    {
        ShopManager.instance.FillShopSlots(shopDefen);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            IConAnim.SetBool("IsPlayerNear", true);
            isPlayerNear = true;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IConAnim.SetBool("IsPlayerNear", false);
            isPlayerNear = false;
        }
    }
}
[Serializable]
public class ShopItem
{
    public ItemSO shopItemSO;
    public int price;

}
