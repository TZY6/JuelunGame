using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New actor", menuName = "Dialogue/NPC")]
public class ActorSO : ScriptableObject
{
    public string actorID;
    public string actorName;
    public Sprite actorHead;
    
}
