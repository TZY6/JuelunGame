using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DialogueSO", menuName = "Dialogue/DialogueNODE")]
public class DialogueSO : ScriptableObject
{
    //public string DialogueName;
    public DialogueLine[] Lines;
    public DialogueOption[] options;

}
[System.Serializable]
public class DialogueLine
{    
    public ActorSO Speaker;
    [TextArea(3,5)] public string Text;
    public QuestSO[] OfferQuest;
    public QuestSO[] CompleteQuest;
}
[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public DialogueSO nextDialogue;
}

