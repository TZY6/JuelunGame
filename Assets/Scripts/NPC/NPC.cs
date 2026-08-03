using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public enum NPCState { Idle,Walkaround, Wander ,Talk }
    public NPCState currentState = NPCState.Wander;
    public NPC_Talk Talk;
    public NPC_Walkaround Walkaround;
    public NPC_Wander Wander;
    // Start is called before the first frame update
    void Start()
    {
        SwitchState(currentState);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SwitchState(NPCState state)
    {
        currentState = state;

        Talk.enabled = state == NPCState.Talk;
        Wander.enabled = state == NPCState.Wander;
        Walkaround.enabled = state == NPCState.Walkaround;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchState(NPCState.Talk);
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchState(NPCState.Wander);
        }
    }
}
