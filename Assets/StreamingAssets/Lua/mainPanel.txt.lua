local mainPanel={}
function mainPanel.Awake( env )
	print("第一个lua UI脚本成功被唤醒")
	env.goldText=env.self.tmps[0]
	env.buildBtn=env.self.buttons[0]
	env.goldCount=100
end
function mainPanel.Start( env )
	print("开始绑定点击监听。。。")
	env.buildBtn.OnClick:AddListener(
		function ( )
		mainPanel.OnBuildClick(env)	
	end)
end
function mainPanel.OnBuildClick(  )
	print("建造了一个家具")
end
function mainPanel.OnDestory(  )
	print("界面正被销毁，清理跨语言指针")
	if env.buildBtn~=nil then 
		evn.OnBuildClick.OnClick:RemoveAllListeners()
	end
end

return mainPanel


