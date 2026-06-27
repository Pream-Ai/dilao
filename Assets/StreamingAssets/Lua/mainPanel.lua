local mainPanel = {}
local targetFurni=-1
function mainPanel.Awake(env)
    print("lua is awake")
    env.goldText = env.Self.tmps[0]
    env.buildBtn = env.Self.buttons[targetFurni]
    env.goldCount = 100
end

function mainPanel.Start(env)
    print("start listen")
    env.buildBtn.onClick:AddListener(function()
        mainPanel.OnBuildClick(0)
    end)
end

function mainPanel.OnBuildClick()
    print("you select a furniture")
    CS.UIManager.instance:selectFurni(targetFurni)
end

function mainPanel.OnDestroy(env)
    print("destroy...")
    if env.buildBtn then
        env.buildBtn.onClick:RemoveAllListeners()
    end
end

return mainPanel