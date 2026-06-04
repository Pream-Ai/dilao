using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;

public class SaveLoadManager : MonoBehaviour
{
    private string saveFilePath;
    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
    }
    public void OnClickSave()
    {
        Debug.Log("存档");
        SaveGameAsync().Forget();
    }
    public void OnClickLoad()
    {
        Debug.Log("读档");
        LoadGameAsync().Forget();
    }
    public async UniTask SaveGameAsync()
    {
        Debug.Log("开始构建存档数据...");
        GameSavePackage savePackage = new GameSavePackage();
        savePackage.totalWorth = GameManager.instance.TotalWorth;
        savePackage.totalFurni = GameManager.instance.TotalFurni;
        savePackage.totalNpc = GameManager.instance.TotalNpc;
        foreach (Transform furni in GameManager.instance.furniParent)
        {
            var ctrl = furni.GetComponent<furniController>();
            if (ctrl != null)
            {
                savePackage.saveFurnitures.Add(new FurniSaveData
                {
                    furniID=ctrl.ID,
                    posX = ctrl.setPos.x,
                    posY = ctrl.setPos.y
                });
            }
        }
        foreach (Transform npc in GameManager.instance.npcParent)
        {
            var ctrl = npc.GetComponent<NpcController>();
            if (ctrl != null)
            {
                savePackage.saveNpc.Add(new NpcSaveData
                {
                    npcID = ctrl.ID,
                    posX = (int)ctrl.transform.position.x,
                    posY = (int)ctrl.transform.position.y,
                    remainMoney = ctrl.money,
                    currentState = ctrl.fsm._currentState
                });
            }
        }

        string jsonString = JsonUtility.ToJson(savePackage, true);
        byte[] encodedText = Encoding.UTF8.GetBytes(jsonString);
        using (FileStream sourceStream = new FileStream(
            saveFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true)
            )
        {
            await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
        }
        Debug.Log($"存档异步写入成功！物理路径: {saveFilePath}");
    }
    public async UniTask LoadGameAsync() 
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("存档文件不存在，无法加载游戏！");
            return;
        }
        Debug.Log("开始读取磁盘存档...");
        byte[] bytes = await File.ReadAllBytesAsync(saveFilePath).AsUniTask();
        string jsonString = Encoding.UTF8.GetString(bytes);
        Debug.Log($"存档异步读取成功！物理路径: {saveFilePath}");
        CleanGame();
        GameSavePackage loadedPackage = JsonUtility.FromJson<GameSavePackage>(jsonString);
        int spawnFurni = 0;
        foreach (var furniData in loadedPackage.saveFurnitures)
        {
            var furni=furniManager.instance.furniDataList[furniData.furniID];
            buildSystem.instance.furniBeSelect = furni;
            buildSystem.instance.buildFurni(new Vector2Int(furniData.posX,furniData.posY));
            spawnFurni++;
            if (spawnFurni >= 5)
            {
                spawnFurni = 0;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        //foreach (var npcSaveData in loadedPackage.saveNpc)
        //{
        //    GameObject npcInstance = Instantiate(
        //        NpcManager.instance.npcDataList[npcSaveData.npcID].prefab,
        //        new Vector3(npcSaveData.posX, npcSaveData.posY, 0),
        //        Quaternion.identity,
        //        GameManager.instance.npcParent
        //    );
        //    NpcController controller = npcInstance.GetComponent<NpcController>();
        //    controller.money = npcSaveData.remainMoney;
        //    controller.fsm._currentState = npcSaveData.currentState;
        //    Debug.Log($"生成NPC，剩余金钱: {npcSaveData.remainMoney}");
        //}
    }
    private void CleanGame()
    {
        for (int i=0;i<GameManager.instance.furniParent.childCount;i++)
        {
            GameManager.instance.furniParent.GetChild(i).GetComponent<furniController>().remove();
        }
    }
}
