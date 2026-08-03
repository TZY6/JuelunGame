using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPanel;
    //public string skillDescribe;
    //public string tooltipText;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            tooltipPanel.GetComponentInChildren<TextMeshProUGUI>().text = transform.parent.GetComponent< SkillSlot>().skillSO.skillDescribe ;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Awake()
    {
        tooltipPanel = GameObject.FindWithTag("Tooltip");
    }
}
