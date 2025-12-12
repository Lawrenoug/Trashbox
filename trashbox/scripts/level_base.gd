extends Node2D

@onready var hp_bar = $HUD/HPBar
@onready var start_pos = get_node_or_null("StartPos")

# 删除了 PlayerScene 预加载
# const PlayerScene = preload("res://trashbox/scenes/main/GlobalPlayer.tscn")

# --- 【关键】这里定义了你报错缺少的变量 ---
# 请在编辑器里把 "技能战斗列表" 节点拖给 Battle Ui Node
@export var battle_ui_node: Control 
# 请在编辑器里把 "技能组背包" 节点拖给 Backpack Ui Node
@export var backpack_ui_node: Control 

var current_player = null

func _ready():
	# --- 删除了玩家生成逻辑 ---
	
	# --- 1. 查找现有玩家节点 ---
	# 假设场景中已经有一个名为 "Player" 的节点
	current_player = get_node_or_null("Player")
	
	if current_player:
		# === 【核心修复】确保玩家显示 ===
		current_player.visible = true 
		current_player.modulate.a = 1.0 # 防止透明度是0
		# ================================
	else:
		print("警告：未找到名为 'Player' 的节点")
		# 提前返回，避免后续代码出错
		return
	
	# --- 2. 设置位置（如果场景中有 StartPos 节点）---
	if start_pos: 
		current_player.global_position = start_pos.global_position
	
	# --- 3. 读档和UI连接 ---
	_load_player_data()
	_connect_player(current_player)
	
	# 3. 修复 C# 引用 (让玩家能攻击) - 添加安全检查
	if current_player and current_player.get("attackManager"):
		current_player.attackManager.set("BulletNode", self)
		current_player.attackManager.set("enableAttack", true)
		print("LevelBase: 已连接玩家的攻击管理器")
	
	_create_debug_skip_button()
	if battle_ui_node: battle_ui_node.visible = false
	if backpack_ui_node: backpack_ui_node.visible = false
	
	# === 【新增】场景加载完毕，通知 C# 生成敌人 ===
	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr:
		# 调用 EnterRoom 开始战斗
		print("LevelBase: 呼叫 RoomManager EnterRoom 生成第 %d 层敌人" % GlobalGameState.target_level_index)
		room_mgr.EnterRoom(GlobalGameState.target_level_index)
	else:
		print("错误：找不到 RoomManager 单例！可能未在进入关卡前创建")
	
	# 连接玩家的攻击管理器 - 添加安全检查
	var real_player_node = current_player.get_node("Player") if current_player else null
	if real_player_node and real_player_node.get("attackManager"):
		# 将 LevelBase (self) 设为子弹的父节点，这样子弹才会出现在关卡里
		real_player_node.attackManager.set("BulletNode", self)
		real_player_node.attackManager.set("enableAttack", true)
		print("LevelBase: 已成功连接 C# 攻击管理器")
	else:
		print("LevelBase: 无法在 Player 子节点上找到 attackManager")

# --- 离开逻辑 ---
func _on_skip_level_pressed():
	print("--- [流程] 1. 玩家请求离开关卡 ---")
	
	# === 第一步：立刻停火 ===
	# 防止在清理过程中还有新子弹生成，导致报错
	if current_player:
		var real_player = current_player.get_node_or_null("Player")
		if real_player and real_player.get("attackManager"):
			real_player.attackManager.set("enableAttack", false)
	
	# === 第二步：调用 C# ExitRoom 清理敌人 ===
	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr:
		print("--- [流程] 2. 调用 RoomManager.ExitRoom() 清理敌人 ---")
		room_mgr.ExitRoom()
	
	# === 第三步：【关键】强制等待 ===
	# 等待 2 帧。
	# 第1帧：让 C# 的逻辑跑完。
	# 第2帧：让 Godot 的 QueueFree 生效，确保节点真的从内存中断开了。
	await get_tree().process_frame
	await get_tree().process_frame
	
	print("--- [流程] 3. 调用 RoomManager.endAttack() 恢复节点关系 ---")
	# 调用 endAttack 将玩家和UI节点恢复原位
	if room_mgr:
		room_mgr.endAttack()
	
	print("--- [流程] 4. C# 清理完毕，开始存档和切换 ---")
	
	# === 第四步：存档 (GDScript 逻辑) ===
	_save_player_data()
	
	# === 第五步：清理玩家节点 ===
	if current_player:
		current_player.queue_free()
	
	# === 第六步：计算进度并跳转 ===
	GlobalGameState.current_level_progress += 1
	
	# 通关判断 (假设总共8关: 0~7)
	if GlobalGameState.current_level_progress >= 7: 
		print("--- [流程] 判定：通关 ---")
		var end_scene_path = "res://trashbox/scenes/main/GameEnd.tscn"
		if ResourceLoader.exists(end_scene_path):
			get_tree().change_scene_to_file(end_scene_path)
		else:
			print("错误：找不到结局文件")
	else:
		print("--- [流程] 判定：返回桌面 ---")
		GlobalGameState.should_open_engine_automatically = true
		# 最后一步才是切换场景，这时候 C# 早就完事了，不会报错
		get_tree().change_scene_to_file(GlobalGameState.desktop_scene_path)

# --- 读档逻辑 ---
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

# --- 辅助 UI ---
func _create_debug_skip_button():
	var layer = CanvasLayer.new()
	layer.name = "DebugCanvasLayer"
	layer.layer = 100  # 设置高图层确保在最前面
	add_child(layer)
	
	var btn = Button.new()
	btn.text = "DEBUG: 强制通关"
	# 调整到屏幕右下角更合适的位置
	btn.position = Vector2(get_viewport().size.x - 320, 20)
	btn.custom_minimum_size = Vector2(300, 50)
	btn.modulate = Color(1, 0, 1, 1)  # 确保完全不透明
	btn.pressed.connect(_on_skip_level_pressed)
	layer.add_child(btn)
	
	print("调试按钮已创建，位置:", btn.position)

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
