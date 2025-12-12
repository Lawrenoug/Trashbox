extends Node2D

@onready var hp_bar = $HUD/HPBar
@onready var start_pos = get_node_or_null("StartPos")

const PlayerScene = preload("res://trashbox/scenes/main/GlobalPlayer.tscn")

# --- 【关键】这里定义了你报错缺少的变量 ---
# 请在编辑器里把 "技能战斗列表" 节点拖给 Battle Ui Node
@export var battle_ui_node: Control 
# 请在编辑器里把 "技能组背包" 节点拖给 Backpack Ui Node
@export var backpack_ui_node: Control 

var current_player = null

func _ready():
	# --- 1. 生成玩家逻辑 (必须有这段！) ---
	# 如果场景里自带一个旧的 Player 节点，先清理掉
	if has_node("Player"): 
		get_node("Player").queue_free()
	
	# 实例化新玩家
	if PlayerScene:
		current_player = PlayerScene.instantiate()
		add_child(current_player)
		
		# === 【核心修复】强制让玩家显示 ===
		current_player.visible = true 
		current_player.modulate.a = 1.0 # 防止透明度是0
		# ================================
		
		# 强制把名字改成 Player
		current_player.name = "Player" 
	else:
		print("严重错误：PlayerScene 未加载！")
		return
	# --- 2. 设置位置 ---
	if start_pos: 
		current_player.global_position = start_pos.global_position
	else: 
		# 如果没找到 StartPos 节点，给一个默认坐标防止它飞到 (0,0) 墙里去
		current_player.global_position = Vector2(300, 300) 
	
	# --- 3. 读档和UI连接 (后续代码...) ---
	_load_player_data()
	_connect_player(current_player)
	
	# 3. 修复 C# 引用 (让玩家能攻击)
	if current_player.get("attackManager"):
		current_player.attackManager.set("BulletNode", self)
		current_player.attackManager.set("enableAttack", true)
	
	_create_debug_skip_button()
	if battle_ui_node: battle_ui_node.visible = false
	if backpack_ui_node: backpack_ui_node.visible = false
	
	# === 【新增】场景加载完毕，通知 C# 生成敌人 ===
	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr:
		# 告诉 C#：我已经准备好了，我是第 X 层，请刷怪
		# 这里的 target_level_index 是刚才在地图里存下的
		print("LevelBase: 呼叫 RoomManager 生成第 %d 层敌人" % GlobalGameState.target_level_index)
		room_mgr.EnterRoom(GlobalGameState.target_level_index)
	else:
		print("错误：找不到 RoomManager 单例！")
	var real_player_node = current_player.get_node("Player")
	
	if real_player_node and real_player_node.get("attackManager"):
		# 将 LevelBase (self) 设为子弹的父节点，这样子弹才会出现在关卡里
		real_player_node.attackManager.set("BulletNode", self)
		real_player_node.attackManager.set("enableAttack", true)
		print("LevelBase: 已成功连接 C# 攻击管理器")
	else:
		print("LevelBase 错误: 无法在 Player 子节点上找到 attackManager！")
# --- 读档逻辑 ---
# trashbox/scripts/level_base.gd

func _load_player_data():
	if not current_player: return
	
	# === 【修改】设置真正的 Player 子节点 ===
	var real_player = current_player.get_node_or_null("Player")
	
	if real_player:
		real_player.set("MaxBlood", GlobalGameState.player_max_hp)
		real_player.set("CurrentBlood", GlobalGameState.player_current_hp)
	
	# --- 核心：恢复战斗技能 ---
	# 只要 battle_ui_node 引用还在（哪怕 visible=false），这个逻辑就能跑
	if battle_ui_node and GlobalGameState.saved_skill_paths.size() > 0:
		
		# 1. 获取隐藏的 GridContainer
		var grid = battle_ui_node.get_node("GridContainer") 
		# 如果找不到，尝试用 find_child
		if not grid: grid = battle_ui_node.find_child("GridContainer", true, false)
			
		if grid:
			# 2. 清空旧图标 (如果有的话)
			for slot in grid.get_children():
				for child in slot.get_children(): child.queue_free()
			
			# 3. 填入新技能 (在后台实例化)
			var slots = grid.get_children()
			var paths = GlobalGameState.saved_skill_paths
			
			for i in range(paths.size()):
				if i >= slots.size(): break
				var path = paths[i]
				if path != "":
					var skill_scene = load(path)
					if skill_scene:
						var skill_instance = skill_scene.instantiate()
						slots[i].add_child(skill_instance)
			
			# 4. 【关键】通知 C# 脚本读取这些技能
			# grid 节点上必须挂载了 SkillGroupsUIManager.cs
			if grid.has_method("UpdateSkillList"):
				grid.UpdateSkillList() # 让 C# 更新内部列表
			
			if grid.has_method("GetSkillList"):
				var csharp_skill_list = grid.GetSkillList() # 获取 C# List<Skill>
				
				# 5. 注入给玩家的 AttackManager
				if current_player.get("attackManager"):
					current_player.attackManager.InsertSkill(csharp_skill_list)
					print("LevelBase: 成功将 %d 个技能注入给玩家 C#" % csharp_skill_list.size())
				else:
					print("LevelBase: 玩家缺少 AttackManager，无法注入技能")

# --- 存档逻辑 ---
func _save_player_data():
	if not current_player: return
	
	# === 【修改】获取真正的 Player 子节点 ===
	var real_player = current_player.get_node_or_null("Player")
	
	if real_player:
		# 从子节点读取血量
		var current_hp = real_player.get("CurrentBlood")
		var max_hp = real_player.get("MaxBlood")
		
		# 安全检查：防止 C# 尚未初始化导致获取为 null
		if current_hp == null: current_hp = 100.0
		if max_hp == null: max_hp = 100.0
			
		GlobalGameState.player_current_hp = current_hp
		GlobalGameState.player_max_hp = max_hp
		print("已保存血量: %s / %s" % [current_hp, max_hp])
	else:
		print("错误：无法找到 Player 子节点，存档失败！")
	
	# 保存战斗列表里的技能
	if battle_ui_node:
		var grid = battle_ui_node.get_node("GridContainer")
		if grid:
			var paths: Array[String] = []
			for slot in grid.get_children():
				if slot.get_child_count() > 0:
					var skill_node = slot.get_child(0)
					if skill_node.scene_file_path != "":
						paths.append(skill_node.scene_file_path)
			GlobalGameState.saved_skill_paths = paths

# --- 离开逻辑 ---
func _on_skip_level_pressed():
	print("--- [DEBUG] 按钮被点击 ---")
	
	# 1. 暂时屏蔽 C# 逻辑，先测跳转功能
	# 如果这里报错，说明是队友的 C# 代码卡住了 GDScript
	# var room_mgr = get_node_or_null("/root/RoomManager")
	# var layer_index = GlobalGameState.current_level_progress + 1 
	# if room_mgr:
	# 	var rewards = room_mgr.GetSkills(layer_index)
	# 	if rewards:
	# 		room_mgr.SendSkillToBackpack(rewards)
	# 	room_mgr.ExitRoom()
	print("--- [DEBUG] 已跳过 C# 交互 (排查用) ---")
	
	# 2. 存档 (这是纯 GDScript，应该没事)
	_save_player_data()
	print("--- [DEBUG] 存档完成 ---")
	
	# 3. 销毁玩家
	if current_player:
		current_player.queue_free()
	
	# 4. 更新进度
	print("--- [DEBUG] 当前进度: ", GlobalGameState.current_level_progress)
	GlobalGameState.current_level_progress += 1
	print("--- [DEBUG] 更新后进度: ", GlobalGameState.current_level_progress)
	
	# 5. 通关判断 (假设总共8关，打完第8关索引变成7)
	# 也就是: 0,1,2,3,4,5,6 (前7关) -> 7 (第8关通关)
	if GlobalGameState.current_level_progress >= 7: 
		print("--- [DEBUG] 判定：通关！尝试跳转结局 ---")
		var end_scene_path = "res://trashbox/scenes/main/GameEnd.tscn"
		
		# 检查文件是否存在
		if ResourceLoader.exists(end_scene_path):
			var err = get_tree().change_scene_to_file(end_scene_path)
			if err != OK:
				print("--- [ERROR] 跳转失败，错误码: ", err)
		else:
			print("--- [ERROR] 找不到结局文件！请检查路径: ", end_scene_path)
			
	else:
		print("--- [DEBUG] 判定：未通关，返回桌面 ---")
		GlobalGameState.should_open_engine_automatically = true
		get_tree().change_scene_to_file(GlobalGameState.desktop_scene_path)
		
# --- 辅助 UI ---
func _create_debug_skip_button():
	var layer = CanvasLayer.new()
	add_child(layer)
	var btn = Button.new()
	btn.text = "DEBUG: 强制通关"
	btn.global_position = Vector2(1600, 20)
	btn.custom_minimum_size = Vector2(300, 50)
	btn.modulate = Color(1, 0, 1)
	btn.pressed.connect(_on_skip_level_pressed)
	layer.add_child(btn)

# --- 辅助 Player ---
func setup_player(p): _connect_player(p)

func _connect_player(p): # 注意：这里的 p 传进来的是根节点
	# === 【修改】连接真正的 Player 子节点 ===
	var real_player = p.get_node_or_null("Player")
	
	if real_player and real_player.has_signal("HealthChanged"):
		# 先断开旧的（如果有），防止重复连接报错
		if real_player.is_connected("HealthChanged", _on_health_changed):
			real_player.disconnect("HealthChanged", _on_health_changed)
			
		real_player.connect("HealthChanged", _on_health_changed)
		
		# 立即更新一次 UI
		_on_health_changed(real_player.get("CurrentBlood"), real_player.get("MaxBlood"))
	else:
		print("警告：无法连接血条信号，找不到 Player 子节点或信号缺失")
		
func _on_health_changed(c, m):
	if hp_bar:
		hp_bar.value = c
		hp_bar.max_value = m
