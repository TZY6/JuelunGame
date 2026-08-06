using TMPro;          // 如果使用 TextMeshPro，否则用 UnityEngine.UI.Text
using UnityEngine;
using System;
public class LoadSlot : MonoBehaviour
{
    public int LoadSlotIndex;                // 存档槽编号（对应 save1, save2, ...）

    [Header("UI References")]
    public TextMeshProUGUI infoText;         // 显示存档信息的 TMP 文本（单行或多行）

    /// <summary>
    /// 由 SaveManager 调用，更新此槽位的显示
    /// </summary>
    /// <param name="hasSave">是否存在存档</param>
    /// <param name="dateTimeString">存档中的保存时间字符串（与 SaveSlot 格式一致）</param>
    public void UpdateSlotDisplay(bool hasSave, string dateTimeString = "")
    {
        if (!hasSave)
        {
            // 空存档显示
            infoText.text = $"存档：{LoadSlotIndex}\n（空）";
        }
        else
        {
            // 尝试解析时间字符串
            if (DateTime.TryParse(dateTimeString, out DateTime saveTime))
            {
                string date = saveTime.ToString("yyyy年M月d日");
                string time = saveTime.ToString("H点mm分");
                infoText.text = $"存档：{LoadSlotIndex}\n最后更新时间：\n{date}\n{time}";
            }
            else
            {
                // 如果解析失败，直接显示原始字符串（或显示“未知时间”）
                infoText.text = $"存档：{LoadSlotIndex}\n最后更新时间：\n{dateTimeString}";
            }
        }
    }

    /// <summary>
    /// 点击加载按钮时调用，尝试加载对应存档并切换场景
    /// </summary>
    public void TryLoadGame()
    {
        // 可选：先检查存档是否存在，若不存在可给出提示
        if (!SaveManager.instance.HasSaveData("save" + LoadSlotIndex))
        {
            Debug.LogWarning($"存档 save{LoadSlotIndex} 不存在，无法加载");
            return;
        }

        SaveManager.instance.LoadGame("save" + LoadSlotIndex);
        SceneControl.Instance.EnterRPG();   // 进入游戏场景
    }
}