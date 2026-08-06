using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurt_Canvas : MonoBehaviour
{
    public static Hurt_Canvas instance {  get; private set; }
    private void OnEnable()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
