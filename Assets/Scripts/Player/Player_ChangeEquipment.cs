using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_ChangeEquipment : MonoBehaviour
{
    public Player_Combat Player_Combat;
    public Player_Bow Player_Bow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("ChangeEquipment"))
        {
            Player_Bow.enabled = !Player_Bow.enabled;
            Player_Combat.enabled = !Player_Combat.enabled;
        }
    }
}
