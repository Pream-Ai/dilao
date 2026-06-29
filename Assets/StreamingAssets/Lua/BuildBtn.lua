local buildBtn = {}
local targetFurni = -1

function buildBtn.Awake(env)
    print("lua is awake")
    env.goldText = env.Self.tmps[0]
    -- 假设有3个按钮，对应不同的家具
    env.buildBtns = {
        env.Self.buttons[0],
        env.Self.buttons[1],
        env.Self.buttons[2],
        env.Self.buttons[3],
        env.Self.buttons[4],
        env.Self.buttons[5],
    }
    env.goldCount = 100
end

function buildBtn.Start(env)
    print("start listen")
    for i, btn in ipairs(env.buildBtns) do
        btn.onClick:AddListener(function()
            buildBtn.OnBuildClick(i - 1) -- 假设targetFurni为0,1,2
        end)
    end
end

function buildBtn.OnBuildClick(furniIndex)
    targetFurni = furniIndex
    print("you select a furniture: " .. targetFurni)
    CS.UIManager.instance:selectFurni(targetFurni)
end

function buildBtn.OnDestroy(env)
    print("destroy...")
    if env.buildBtns then
        for _, btn in ipairs(env.buildBtns) do
            btn.onClick:RemoveAllListeners()
        end
    end
end

return buildBtn