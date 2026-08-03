using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public CanvasGroup infoPannel;
    public TMP_Text itemNameText;
    public TMP_Text itemDescribeText;
    public TMP_Text[] itemStateText;
    public RectTransform infoPannelRect;
    public Vector3 MouseOffset;
    void Start()
    {
        //infoPannelRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ShowItemInfo(ItemSO itemSO)
    {
        infoPannel.alpha = 1.0f;
        itemNameText.text = itemSO.name;
        itemDescribeText.text = itemSO.ItemDescirbe;
        List<string> stats = new List<string>();
        if(itemSO.damage > 0 ) stats.Add("攻击力增加:"+itemSO.damage.ToString());
        if (itemSO.MaxHealthUp > 0) stats.Add("生命值上限:" +itemSO.MaxHealthUp.ToString());
        if(itemSO.CurrentHealthUp>0) stats.Add("恢复生命值:"+itemSO.CurrentHealthUp.ToString());
        if (itemSO.speedUp > 0) stats.Add("移速增加:" + itemSO.speedUp.ToString());
        if(stats.Count < 0)
            return;
        else
        {
            for(int i = 0; i < itemStateText.Length; i++)
            {
                if(i<stats.Count)
                {
                    itemStateText[i].text = stats[i];
                    itemStateText[i].gameObject.SetActive(true);
                }
                else
                {
                    itemStateText[i].gameObject.SetActive(false);
                }

            }
        }
    }
    public void HideItemInfo()
    {
        infoPannel.alpha = 0f;
        itemNameText.text = "";
        itemDescribeText.text = "";
    }
    public void InfoFollowMouse()
    {
        infoPannelRect.position = Input.mousePosition+MouseOffset;
    }
}
