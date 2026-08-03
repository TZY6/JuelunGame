using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationHistoryTracker : MonoBehaviour
{
    
    private readonly HashSet<LocationSO> ArrivedLocations = new HashSet<LocationSO>();
    public static LocationHistoryTracker instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void RecordLocation(LocationSO npc)
    {
        ArrivedLocations.Add(npc);

    }
    public bool HasArrived(LocationSO actorSO)
    {
        return ArrivedLocations.Contains(actorSO);
    }
}
