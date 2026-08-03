using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Talk : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animatorWalking;
    public Animator animatorTalkable;
    public DialogueSO dialogueSO;
    // Start is called before the first frame update
    private void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();
        //animator = GetComponentInChildren<Animator>();
    }
    private void OnEnable()
    {
        rb.velocity = Vector2.zero;
        animatorWalking.SetBool("isWalking",false);
        animatorTalkable.Play("TalkReady");
        rb.isKinematic = true;
    }
    private void OnDisable()
    {
        //transform.parent.GetComponentInChildren<NPC_Walkaround>().SetVelocity(true);
        //animator.SetBool("isWalking", true);
        animatorTalkable.Play("TalkUnready");
        rb.isKinematic = false;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Interact"))
        {
            if(!DialogueManager.instance.isDialogueOn)
                DialogueManager.instance.StartANewDia(dialogueSO);
            else
            {
                DialogueManager.instance.ContinueCurrentConversation();
            }
        }
    }
}
  