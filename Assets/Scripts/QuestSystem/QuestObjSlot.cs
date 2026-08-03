using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestObjSlot : MonoBehaviour
{
    public TMP_Text ObjDescriptionText;
    public TMP_Text ObjProgressText;
    private bool Completed=false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool CheckIfQuestCompleted(QuestSO questSO)
    {
        return Completed;
    }
    public void RefreshObj(string ObjDescription, string progressText,bool isComplete)
    {
        ObjDescriptionText.text = ObjDescription;
        ObjProgressText.text = progressText;

        Color color = isComplete? Color.gray : Color.white;
        ObjDescriptionText.color = color;
        ObjProgressText.color = color;
    }
    
}
