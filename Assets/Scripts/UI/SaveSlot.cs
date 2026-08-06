using System;
using TMPro;
using UnityEngine;

public class SaveSlot : MonoBehaviour
{
    public int SaveSlotIndex;
    public TextMeshProUGUI infoText;   // 单个 TMP 显示所有信息

    /// <summary>
    /// 由 SaveManager 调用，根据存档存在与否更新 UI
    /// </summary>
    /// <param name="hasSave">是否存在存档</param>
    /// <param name="dateTimeString">存档中的 saveTime 字符串（格式与保存时一致）</param>
    public void UpdateSlotDisplay(bool hasSave, string dateTimeString = "")
    {
        if (!hasSave)
        {
            // 空存档显示
            infoText.text = $"存档：{SaveSlotIndex}\n（空）";
        }
        else
        {
            // 尝试解析字符串为日期时间，并按需要格式化显示
            // 如果保存时使用 "yyyy-MM-dd HH:mm:ss" 等格式，可直接用 DateTime.Parse
            if (DateTime.TryParse(dateTimeString, out DateTime saveTime))
            {
                string date = saveTime.ToString("yyyy年M月d日");
                string time = saveTime.ToString("H点mm分");
                infoText.text = $"存档：{SaveSlotIndex}\n最后更新时间：\n{date}\n{time}";
            }
            else
            {
                // 如果解析失败，直接显示原始字符串
                infoText.text = $"存档：{SaveSlotIndex}\n最后更新时间：\n{dateTimeString}";
            }
        }
    }

    // 原有方法（用于保存时即时更新，使用当前时间）
    public void UpdateSaveSlotUI()
    {
        // 保留原有逻辑：用当前时间更新（用于保存后立即刷新）
        DateTime now = DateTime.Now;
        string date = now.ToString("yyyy年M月d日");
        string time = now.ToString("H点mm分");
        infoText.text = $"存档：{SaveSlotIndex}\n最后更新时间：\n{date}\n{time}";
    }
    public void TryToSave()
    {
        SaveManager.instance.SaveGame("save"+ SaveSlotIndex);
    }
}