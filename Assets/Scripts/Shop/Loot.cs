using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public ItemSO ItemSO;
    public SpriteRenderer LootRenderer;
    public Animator anim;
    public bool canbePickedUp = true;
    public int amount;
    public static event Action<ItemSO, int> OnItemLooted;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Initialize(ItemSO itemso, int amount)
    {
        ItemSO = itemso;
        this.amount = amount;
        canbePickedUp = false;
        OnValidate();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnValidate()
    {
        if(ItemSO == null) return;
        LootRenderer.sprite = ItemSO.icon;
        this.name = ItemSO.name;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag( "Player") && canbePickedUp)
        {
            anim.Play("LootGet");
            OnItemLooted?.Invoke(ItemSO, amount);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canbePickedUp)
        {
            canbePickedUp = true;
        }
        
    }
    public void DesroyAfterGet()
    {
        Destroy(gameObject);
    }
}
