using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI引用")]
    public Image actorHead;
    public TMP_Text actorName;
    public TMP_Text DialogueText;
    public DialogueSO currentDialogue;
    public CanvasGroup DialogurCanvas;
    public Button[] optionButtons;
    public int dialogueIndex;
    public ActorSO Player;
    [Header("状态")]
    public bool isDialogueOn = false;
    public static DialogueManager instance {  get; private set; }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
    }
    public void StartANewDia(DialogueSO curdia)
    {
        dialogueIndex = 0;
        currentDialogue = curdia;
        ShowDialogue();
        HideDiaButtons();
    }
    public void ShowDialogue()
    {
        isDialogueOn = true;
        DialogurCanvas.alpha = 1;
        DialogurCanvas.interactable = true;
        DialogurCanvas.blocksRaycasts = true;
        DialogueLine line = currentDialogue.Lines[dialogueIndex];
        actorHead.sprite = line.Speaker.actorHead;
        actorName.text = line.Speaker.actorName;
        
        DialogueText.text = ReplacePlaceholders(line.Text, Player);
        if (line.OfferQuest != null)
            GetNewQuest.instance.AddCurrentQuest(line.OfferQuest);
        if (line.CompleteQuest != null)
            QuestManager.instance.MakeQuestComplete(line.CompleteQuest);
    }

    public void ContinueCurrentConversation()
    {
        // 如果当前已经是最后一行
        if (dialogueIndex == currentDialogue.Lines.Length - 1)
        {
            // 有选项：选项应该已经显示，忽略继续点击（或可在此禁用继续按钮）
            if (currentDialogue.options.Length > 0)
            {
                // 可考虑禁用继续交互，避免干扰
                return;
            }
            else
            {
                // 无选项：结束对话
                StopDialogue();
                return;
            }
        }

        // 未到最后一行 → 推进
        dialogueIndex++;
        ShowDialogue();

        // 推进后检查是否到达最后一行
        if (dialogueIndex == currentDialogue.Lines.Length - 1)
        {
            if (currentDialogue.options.Length > 0)
            {
                ShowDiaButtons();      // 自动显示选项
                                       // 可同时禁用“继续”按钮，防止再次点击
            }
            // 若无选项，不自动结束，等待下一次点击（由上面的分支处理）
        }
    }
    public void StopDialogue()
    {
        isDialogueOn = false;
        DialogurCanvas.alpha = 0;
        DialogurCanvas.interactable = false;
        DialogurCanvas.blocksRaycasts = false;
    }

    private string ReplacePlaceholders(string originalText, ActorSO playerActor)
    {
        if (string.IsNullOrEmpty(originalText) || playerActor == null)
            return originalText;

        // 使用 Replace 替换所有出现的 "#玩家名#"
        return originalText.Replace("#玩家名#", playerActor.actorName);
    }
    public void ShowDiaButtons()
    {
        for (int i = 0; i < currentDialogue.options.Length; i++)
        {
            // 清除旧监听器，防止重复绑定
            optionButtons[i].onClick.RemoveAllListeners();

            int index = i; // 闭包捕获
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = currentDialogue.options[i].optionText;
            optionButtons[i].gameObject.SetActive(true);

            if (currentDialogue.options[i].nextDialogue != null)
            {
                optionButtons[i].onClick.AddListener(() => ChooseOption(currentDialogue.options[index].nextDialogue));
            }
            else
            {
                optionButtons[i].onClick.AddListener(StopDialogue);
            }
        }
    }
    public void ChooseOption(DialogueSO nextDialogue)
    {
        dialogueIndex = 0;
        currentDialogue = nextDialogue;
        StartANewDia(nextDialogue);
    }
    public void HideDiaButtons()
    {
        foreach (Button but in optionButtons)
        {
            but.gameObject.SetActive(false);
        }
    }
}
