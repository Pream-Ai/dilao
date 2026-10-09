using UnityEngine;

public class GameLaunch : MonoBehaviour
{
    private void Awake()
    {
        // 初始化游戏资源、数据模块与 UI 模块
    }

    async void Start()
    {
        // 初始化资源系统，检查 catalog 更新并下载远端内容
        bool ready = await ResourceBootstrap.PrepareAsync();
        if (!ready)
        {
            Debug.LogError("[GameLaunch] 资源准备失败，游戏可能无法正常加载");
        }

        // 进入游戏
        XLuaManager.instance.EnterGame();
    }

    void Update()
    {
        
    }
}
