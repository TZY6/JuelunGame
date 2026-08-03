using UnityEngine;

public class QuitManager : MonoBehaviour
{
    public CanvasGroup quitConfirm;
    /// <summary>
    /// 退出游戏（编辑器下停止运行，打包后关闭应用程序）
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 仅在编辑器模式下生效的退出播放模式逻辑
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        // 打包后的退出应用
        Application.Quit();
#endif
    }
    public void OpenQuitConfirm(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    public void CloseQuitConfirm(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    } 
}