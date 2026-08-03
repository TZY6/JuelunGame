using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class DialogueHistoryTracker : MonoBehaviour
{
    private readonly HashSet<ActorSO> spokenNPCs = new HashSet<ActorSO>();
    public static DialogueHistoryTracker instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    public void RecordNPC(ActorSO npc)
    {
        spokenNPCs.Add(npc);

    }
    public bool HasSpokenWith(ActorSO actorSO)
    {
        return spokenNPCs.Contains(actorSO);
    }
    public DialogueHistoryData GetSaveData()
    {
        DialogueHistoryData data = new DialogueHistoryData();

        data.spokenNPCIDs = new List<string>();

        foreach (ActorSO npc in spokenNPCs)
        {
            data.spokenNPCIDs.Add(npc.actorID);
        }
        return data;
    }
    public void LoadData(DialogueHistoryData data)
    {
        if (data == null)
            return;


        spokenNPCs.Clear();


        foreach (string id in data.spokenNPCIDs)
        {

            ActorSO npc =
            NPCDatabase.Instance.GetActorByID(id);


            if (npc != null)
            {
                spokenNPCs.Add(npc);
            }

        }
    }
}
public class NPCDatabase : MonoBehaviour
{
    public static NPCDatabase Instance;
    public List<ActorSO> actors;
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

    public ActorSO GetActorByID(string id)
    {

        foreach (var actor in actors)
        {
            if (actor.actorID == id)
                return actor;
        }
        return null;
    }
}
[Serializable]
public class DialogueHistoryData
{
    public List<string> spokenNPCIDs;
}