using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationHistoryTracker : MonoBehaviour
{
    private readonly HashSet<string> arrivedLocations = new HashSet<string>();
    public static LocationHistoryTracker instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RecordLocation(LocationSO location)
    {
        if (location != null)
            arrivedLocations.Add(location.locationID);
    }

    public bool HasArrived(LocationSO location)
    {
        if (location == null)
            return false;

        return arrivedLocations.Contains(location.locationID);
    }

    public LocationHistoryData GetSaveData()
    {
        LocationHistoryData data = new LocationHistoryData();
        data.arrivedLocationIDs = new List<string>(arrivedLocations);
        return data;
    }

    public void LoadData(LocationHistoryData data)
    {
        if (data == null || data.arrivedLocationIDs == null)
            return;

        arrivedLocations.Clear();

        foreach (string id in data.arrivedLocationIDs)
        {
            arrivedLocations.Add(id);
        }
    }
}

public class LOCATIONDatabase : MonoBehaviour
{
    public static LOCATIONDatabase Instance;
    public List<LocationSO> locations;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LocationSO GetLocationByID(string id)
    {
        foreach (LocationSO location in locations)
        {
            if (location.locationID == id)
                return location;
        }

        return null;
    }
}

[Serializable]
public class LocationHistoryData
{
    public List<string> arrivedLocationIDs;
}