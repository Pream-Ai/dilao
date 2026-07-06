using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLaunch : MonoBehaviour
{
    private void Awake()
    {
        //初始化游戏资源，网络模块，ui模块

        //
    }
    void Start()
    {
        //检查服务器资源版本，下载更新包

        //进入游戏
        XLuaManager.instance.EnterGame();
    }

    void Update()
    {
        
    }
}
