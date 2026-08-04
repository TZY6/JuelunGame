using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfinerFinder : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        CinemachineConfiner2D confiner = GetComponentInChildren<CinemachineConfiner2D>();
        if(confiner != null )
        {
            confiner.m_BoundingShape2D = GameObject.FindWithTag("Confiner").GetComponent<PolygonCollider2D>();
        }
        else
        {
            Debug.LogError("未找到confiner");
        }

    }
}
