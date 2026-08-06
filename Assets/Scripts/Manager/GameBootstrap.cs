using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

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

    private void Start()
    {
        //LoadGame();
    }

    private void LoadGame()
    {
        if (SaveManager.instance.HasSaveData())
        {
            SaveManager.instance.LoadGame();
            Debug.Log("读取已有存档");
        }
        else
        {
            Debug.Log("新游戏");
            CreateNewGame();
        }
    }

    private void CreateNewGame()
    {
        //初始化默认数据
    }
}