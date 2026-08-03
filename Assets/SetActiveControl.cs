using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class SetActiveControl : MonoBehaviour
{
    public static SetActiveControl instance {  get; private set; }
    public GameObject[] OpenObjs;
    public GameObject[] CloseObjs;
    public CanvasGroup[] ToMuteCanvas;
    public GameObject HurtPannel;
    public Canvas HurtCanvas;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject obj in OpenObjs)
        {
            obj.SetActive(true);
        }
        foreach (GameObject obj in CloseObjs)
        {
            obj.SetActive(false);
        }
        foreach(var obj in ToMuteCanvas)
        {
            obj.alpha = 0;
            obj.interactable = false;
            obj.blocksRaycasts = false;
        }
    }
   

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        Application.targetFrameRate = 60;
    }
}
