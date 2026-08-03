using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoticeManager : MonoBehaviour
{
    public static NoticeManager instance {  get; private set; }
    public GameObject NoticePannel;
    public GameObject noticePrefab;
    //public CanvasGroup CanvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
       
    }
    public void ShowNotice(string content)
    {
        if (noticePrefab == null)
        {
            Debug.LogError("noticePrefab Î´¸³Öµ£¡");
            return;
        }
        if (NoticePannel == null)
        {
            Debug.LogError("NoticePannel Î´¸³Öµ£¡");
            return;
        }
        GameObject newNotice = Instantiate(noticePrefab,NoticePannel.transform);
        newNotice.transform.localPosition = Vector3.zero;
        TextMeshProUGUI noticeText = newNotice.GetComponentInChildren<TextMeshProUGUI>();
        noticeText.text = content;
        Animator animator = newNotice.GetComponentInChildren<Animator>();
        animator.Play("NoticeUI");
    }
}
