using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    //[SerializeField]
    //private int current_Health = 0;
    //[SerializeField]
    //private int max_health = 0;
    public TextMeshProUGUI HealthText;
    public Animator HealthTextAni;
    // Start is called before the first frame update
    void Start()
    {
        UpdateHealth();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateHealth()
    {
        HealthText.text = "HP: " +  DataManager.instance.currentHealth.ToString() +'/'+ DataManager.instance.maxHealth;
    }
    public void HealthChange(int health)
    {
        DataManager.instance.currentHealth += health;
        HealthTextAni.Play("TextUI");
        if (DataManager.instance.currentHealth > DataManager.instance.maxHealth)
        {
            DataManager.instance.currentHealth = DataManager.instance.maxHealth;
        }
        if (DataManager.instance.currentHealth <= 0)
        {
            DataManager.instance.currentHealth = 0;
            PlayerIsDead();

        }
        UpdateHealth();
    }
    public void PlayerIsDead()
    {
        gameObject.SetActive(false);
    }
}
