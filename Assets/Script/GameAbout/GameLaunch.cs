using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLaunch : MonoBehaviour
{
    private void Awake()
    {
        //��ʼ����Ϸ��Դ������ģ�飬uiģ��

        //
    }
    void Start()
    {
        //����������Դ�汾�����ظ��°�

        //������Ϸ
        AssetService.Instance.InitializeAsync();

        XLuaManager.instance.EnterGame();
    }

    void Update()
    {
        
    }
}
