using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {  get; private set; }
    public SceneControl sceneControl;
    //public DialogueHistoryTracker historyTracker;
    [Header("持久化对象")]
    public GameObject[] ToAlwaysPersistObjs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            PersistObjAlways();
        }            
        else
        {
            ClearAndDestroy();
            return;
        }
            
    }
    public void SceneLoadWithOutTrigger()
    {
        sceneControl = GameObject.FindWithTag("SceneControl").GetComponent<SceneControl>();
        sceneControl.OnSceneInitialization();
    }
    public void PersistObjAlways()
    {
        foreach (GameObject obj in ToAlwaysPersistObjs)
        {
            if(obj!= null)
            DontDestroyOnLoad (obj);
        }
    }
    public void ClearAndDestroy()
    {
        foreach(GameObject obj in ToAlwaysPersistObjs)
        {
            Destroy(obj);
        }
        Destroy(gameObject);
    }
}
