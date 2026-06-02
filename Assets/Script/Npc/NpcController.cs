using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// NPC 总控脚本
/// ---NpcBrain 逻辑层
/// ---NpcAction 行为层
/// ---NpcView 表现层
/// </summary>
public class NpcController : MonoBehaviour
{
    [Header("基础数据")]
    public NpcData data;
    public int ID;
    public string name;
    public GameObject prefab;
    public int level;
    public int money;

    [Header("状态机")]
    public FSM fsm;
    public IdleState idleState;
    public WanderState wanderState;
    public MoveState moveState;
    public WaitState waitState;
    public BuyState buyState;
    public EnterPool enterPool;
    public ExitPool exitPool;

    [Header("决策系统")]
    [SerializeField] private float decisionInterval = 1f; // 决策时间间隔
    private float decisionTimer=5f; // 决策时间计时器
    public furniController targetFurni;//目标家具
    public Transform targetLogo;//测试用
    private void Start()
    {
        initNpcData();
        transform.GetComponent<ClickMarker>().Init(1, this.data);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("开始寻路");
            if (furniManager.instance.furniList.Count <= 0)
            {
                Debug.Log("无目标");
                return;
            }
            targetFurni = furniManager.instance.furniList[0];
            fsm.stateChange(moveState);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("开始闲逛");
            fsm.stateChange(wanderState);
        }
        //逐帧刷新
        fsm._currentState.TickPerFrame();
        decisionTimer += Time.deltaTime;
        if (decisionInterval <= decisionTimer)
        {
            decisionTimer = 0f;
            fsm._currentState?.TickDecision();
        }
    }
    public void initNpcData()
    {
        //数据初始化
        ID = data.ID;
        name = data.name;
        prefab = data.prefab;
        level = data.level;
        money = data.money;
        //状态机初始化
        fsm = new FSM();
        idleState = new IdleState(this);
        wanderState= new WanderState(this);
        moveState = new MoveState(this);
        waitState = new WaitState(this);
        buyState = new BuyState(this);
        enterPool = new EnterPool(this);
        exitPool = new ExitPool(this);
        fsm.stateChange(idleState);//默认状态
        decisionTimer = UnityEngine.Random.Range(0, decisionInterval);
    }
    public void cleanNpcData()
    {
        ID = -1;
        name = null;
        prefab = null;
        level = -1;
        money = -1;
    }
    #region 决策系统
    //生成/决策逻辑,测试用
    public int nextFurniIndex = 0;
    public furniController getNextFurni()
    {
        nextFurniIndex++;
        if (nextFurniIndex > furniManager.instance.furniList.Count-1)
        {
            nextFurniIndex = 0;
        }
        return furniManager.instance.furniList[nextFurniIndex];
    }
    //决策逻辑
    public void Decision()
    {
        foreach (var kv in NpcManager.instance.furniList)
        {
            if (!kv.Value.beUsing)
            {
                Debug.Log($"{kv.Value.name}没有被使用，可以预约");
                NpcManager.instance.TryReseveSlot(kv.Value);
            }
        }
    }
    #endregion

    #region 行为系统
    int manhattan(int x1, int y1, int x2, int y2)
    {
        int result = 0;
        result = math.abs(x2 - x1) + math.abs(y2 - y1);
        return result;
    }
    bool isValid(int x, int y, int width, int height, bool[,] walls)
    {
        return x >= 0 && x < width && y >= 0 && y < height && !walls[x, y];
    }
    public List<Vector2Int> AStar()
    {
        bool[,] walls = buildSystem.instance.getWall(targetFurni);
        var end = targetFurni.setPos + targetFurni.offset;
        targetLogo.position = new Vector3(end.x,end.y);
        Vector2Int start = npcPosToGrid();
        int width = walls.GetLength(0);
        int height = walls.GetLength(1);
        Vector2Int[,] cameFrom = new Vector2Int[width, height];
        int[,] gscore = new int[width, height];
        int[,] fscore = new int[width, height];
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                gscore[i, j] = int.MaxValue;
                fscore[i, j] = int.MaxValue;
                cameFrom[i, j] = new Vector2Int(-1, -1);
            }
        }
        gscore[start.x, start.y] = 0;
        fscore[start.x, start.y] = manhattan(start.x, end.x, start.y, end.y);

        List<Vector2Int> openList = new List<Vector2Int> { start };
        Vector2Int[] dirs = {
        new Vector2Int(0,1), new Vector2Int(0,-1),
        new Vector2Int(1,0), new Vector2Int(-1,0)
        };

        while (openList.Count > 0)
        {
            Vector2Int current = openList[0];
            foreach (var node in openList)
            {
                if (fscore[node.x, node.y] < fscore[current.x, current.y])
                {
                    current = node;
                }
            }
            if (current == end)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int node = end;
                while (node.x != -1)
                {
                    path.Add(node);
                    node = cameFrom[node.x, node.y];
                }
                path.Reverse();
                return path;
            }
            openList.Remove(current);
            foreach (Vector2Int dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (!isValid(neighbor.x, neighbor.y, width, height, walls))
                    continue;
                int tentativeG = gscore[current.x, current.y] + 1;
                if (tentativeG < gscore[neighbor.x, neighbor.y])
                {
                    cameFrom[neighbor.x, neighbor.y] = current;
                    gscore[neighbor.x, neighbor.y] = tentativeG;
                    fscore[neighbor.x, neighbor.y] = tentativeG + manhattan(neighbor.x, neighbor.y, end.x, end.y);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        Debug.Log("寻路失败");
        return null;
    }
    Vector2Int npcPosToGrid() => new Vector2Int((int)transform.position.x / 1, (int)transform.position.y / 1);
    #endregion

    #region 表现层
    //排序层与深度效果
    public void sortLayer()
    {
        int targetOrder = 101 - (int)transform.position.y;
        transform.GetComponent<SpriteRenderer>().sortingOrder = targetOrder;
        transform.position = new Vector3(transform.position.x, transform.position.y, targetOrder * -0.01f);
    }
    public void buy()
    {
        Debug.Log("进度条推动");
    }
    #endregion
}
