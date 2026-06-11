using System.Collections.Generic;
using DG.Tweening;
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
    public string currentState;
    public IdleState idleState;
    public WanderState wanderState;
    public MoveState moveState;
    public WaitState waitState;
    public BuyState buyState;
    public EnterPool enterPool;
    public ExitPool exitPool;

    [Header("决策系统")]
    [SerializeField] private float decisionInterval = 5; // 决策时间间隔
    private float decisionTimer = 1f; // 决策时间计时器
    public furniController targetFurni;//目标家具
    [Header("表现层")]
    public Animator anim;
    private SpriteRenderer spriteRenderer;
    public SpriteRenderer emo;
    private void OnEnable()
    {
        initNpcData();
        transform.GetComponent<ClickMarker>().Init(1, this.data);
    }
    private void Update()
    {
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
        wanderState = new WanderState(this);
        moveState = new MoveState(this);
        waitState = new WaitState(this);
        buyState = new BuyState(this);
        enterPool = new EnterPool(this);
        exitPool = new ExitPool(this);
        fsm.stateChange(idleState);//默认状态
        //参数初始化
        decisionTimer = UnityEngine.Random.Range(0, decisionInterval);
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1,1,1,0);
    }
    public void cleanNpcData()
    {
        ID = -1;
        name = null;
        prefab = null;
        level = -1;
        money = -1;
    }
    private void OnDisable()
    {
        DOTween.Kill(gameObject);
        CancelInvoke();
        fsm._currentState = null;
        cleanNpcData();
    }
    #region 决策系统
    /// <summary>
    /// 决策逻辑找未注册家具且不和上次使用的家具一样
    /// </summary>
    /// <returns></returns>
    public bool Decision()
    {
        List<furniController> CanBeUseFurniList = new List<furniController>();
        foreach (var kv in NpcManager.instance.furniList)
        {
            if (!kv.Value.beUsing && kv.Value != targetFurni)
            {
                CanBeUseFurniList.Add(kv.Value);
            }
        } 
        if (CanBeUseFurniList.Count > 0)
        {
            var targetIndex = UnityEngine.Random.Range(0, CanBeUseFurniList.Count);
            targetFurni = CanBeUseFurniList[targetIndex];
            NpcManager.instance.TryReseveSlot(targetFurni);
            return true;
        }
        return false;
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
        if (targetFurni == null) return null;
        bool[,] walls = buildSystem.instance.getWall(targetFurni);
        var end = targetFurni.setPos + targetFurni.offset;
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
    /// <summary>
    /// 动静切换
    /// </summary>
    /// <param name="ismove">是否切换至移动状态</param>
    public void changeAnim(bool ismove)
    {
        if (ismove) anim.SetBool("ismove", true);
        else anim.SetBool("ismove", false);
    }
    /// <summary>
    /// 翻转切换，当flip为真时面向右边
    /// </summary>
    public void changeFlip(Vector2Int targetPos)
    {
        var deltaX = targetPos.x - transform.position.x;
        if (deltaX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (deltaX < 0)
        {
            spriteRenderer.flipX = true;
        }
    }
    /// <summary>
    /// /排序层与深度效果
    /// </summary>
    public void sortLayer()
    {
        int targetOrder = 101 - Mathf.RoundToInt(transform.position.y);
        float zOffset = targetOrder * -0.01f + (ID * -0.0001f);//防止y轴闪烁
        transform.GetComponent<SpriteRenderer>().sortingOrder = targetOrder;
        transform.position = new Vector3(transform.position.x, transform.position.y, zOffset);
    }
    /// <summary>
    /// 购买过程中进度条显示
    /// </summary>
    public void buy()
    {
        //Debug.Log("进度条推动");
    }
    /// <summary>
    /// 切换表情
    /// </summary>
    /// <param name="targetEmo"></param>
    public void showEmo(int emoIndex=0)
    {

    }
    #endregion
}
