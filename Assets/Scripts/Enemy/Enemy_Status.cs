using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Status : MonoBehaviour
{
    public EnemyStatus enemyState;
    public SpriteRenderer enemySprite;
    public Coroutine burnCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        enemySprite = GetComponent<SpriteRenderer>();
        //if(enemySprite==null)
        //{
        //    enemySprite = GetComponent<SpriteRenderer>();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
        if (enemyState== EnemyStatus.ICE)
        {
            EnemyOnICE();
            ColorUtility.TryParseHtmlString("#00B6C3", out Color ICEColor);
            enemySprite.color = ICEColor;            
        }
        else if(enemyState == EnemyStatus.FIRE)
        {
            EnemyOnFIRE();
            ColorUtility.TryParseHtmlString("#FF5353", out Color FIREcolor);
            enemySprite.color = FIREcolor;
        }
        else if(enemyState == EnemyStatus.THUNDER)
        {

        }
        else//正常状态
        {
            EnemyOnNORMAL();
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
    public void EnemyOnICE()
    {
        //减速
        GetComponent<Enemy_Movement>().currentSpeed = GetComponent<Enemy_Movement>().initialSpeed / 2f;
        
    }
    public void EnemyOnFIRE()
    {
        StartBurn();
    }
    public void StartBurn()
    {
        //if (burnCoroutine != null)
        //    StopCoroutine(burnCoroutine);
        //burnCoroutine = StartCoroutine(BurnRoutine());
        if (burnCoroutine == null)
             burnCoroutine = StartCoroutine(BurnRoutine());

    }
    public void StopBurn()
    {
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
            burnCoroutine = null;
        }
    }
    private IEnumerator BurnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            GetComponent<Enemy_Health>().HealthChange(DataManager.instance.FIREburnDamage, "FIRE");
        }
    }
    public void EnemyOnNORMAL()
    {
        ColorUtility.TryParseHtmlString("#FFFFFF", out Color NORMALColor);
        //enemySprite.color = NORMALColor;
        //减速
        GetComponent<Enemy_Movement>().currentSpeed = GetComponent<Enemy_Movement>().initialSpeed ;
    }
    public enum EnemyStatus
    {
        ICE,
        FIRE,
        THUNDER,
        NORMAL
    }

}
