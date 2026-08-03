using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuestEvent  
{
    public static Action<QuestSO> OnQuestOfferRequested;
    public static Func<QuestSO,bool> isQuestComplete;
}
