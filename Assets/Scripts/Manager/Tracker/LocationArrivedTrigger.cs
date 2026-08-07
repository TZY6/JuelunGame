using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationArrivedTrigger : MonoBehaviour
{
    public LocationSO locationSO;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            LocationHistoryTracker.instance.RecordLocation(locationSO);
            Destroy(gameObject);//防止每次进入都触发
        }
    }
    private void OnValidate()
    {
        gameObject.name = locationSO.name;
    }
}
