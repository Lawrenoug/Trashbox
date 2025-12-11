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
	# 1. 清理旧玩家
	if has_node("Player"): get_node("Player").queue_free()
	
	# 2. 生成新玩家
	current_player = PlayerScene.instantiate()
	add_child(current_player)
	
	# 设置位置
	if start_pos: current_player.global_position = start_pos.global_position
	else: current_player.global_position = Vector2(300, 300)
	
	# --- 读档 ---
	_load_player_data()
	
	# 连接血条
	_connect_player(current_player)
	
	# 3. 修复 C# 引用 (让玩家能攻击)
	if current_player.get("attackManager"):
		current_player.attackManager.set("BulletNode", self)
		current_player.attackManager.set("enableAttack", true)
	
	# 4. 生成测试按钮
	_create_debug_skip_button()

	# --- 【UI 显示控制：战斗只显示战斗列表】---
	if battle_ui_node:
		battle_ui_node.visible = true
		
	if backpack_ui_node:
		backpack_ui_node.visible = false # 战斗时隐藏背包

# --- 读档逻辑 ---
func _load_player_data():
	if not current_player: return
	
	# 恢复血量
	current_player.set("MaxBlood", GlobalGameState.player_max_hp)
	current_player.set("CurrentBlood", GlobalGameState.player_current_hp)
	
	# 恢复战斗列表技能 (如果UI节点存在)
	if battle_ui_node and GlobalGameState.saved_skill_paths.size() > 0:
		var grid = battle_ui_node.get_node("GridContainer")
		if grid:
			# 清空旧图标
			for slot in grid.get_children():
				for child in slot.get_children(): child.queue_free()
			
			# 填入新图标
			var slots = grid.get_children()
			var paths = GlobalGameState.saved_skill_paths
			for i in range(paths.size()):
				if i >= slots.size(): break
				var path = paths[i]
				if path != "":
					var skill_scene = load(path)
					if skill_scene:
						slots[i].add_child(skill_scene.instantiate())
			
			# 通知 C# 更新
			if grid.has_method("GetSkillList"):
				var list = grid.GetSkillList()
				current_player.attackManager.InsertSkill(list)
				print("LevelBase: 已恢复技能 -> ", list.size())

# --- 存档逻辑 ---
func _save_player_data():
	if not current_player: return
	
	# 保存血量
	GlobalGameState.player_current_hp = current_player.get("CurrentBlood")
	GlobalGameState.player_max_hp = current_player.get("MaxBlood")
	
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
	print("DEBUG: 结算...")
	
	var room_mgr = get_node_or_null("/root/RoomManager")
	var layer_index = GlobalGameState.current_level_progress + 1 
	if room_mgr:
		var rewards = room_mgr.GetSkills(layer_index)
		if rewards:
			room_mgr.SendSkillToBackpack(rewards)
		room_mgr.ExitRoom()
	
	# 保存数据
	_save_player_data()
	
	# 销毁玩家
	current_player.queue_free()
	
	# 跳转
	GlobalGameState.current_level_progress += 1
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
func _connect_player(p):
	if p.has_signal("HealthChanged"):
		p.connect("HealthChanged", _on_health_changed)
		_on_health_changed(p.get("CurrentBlood"), p.get("MaxBlood"))
func _on_health_changed(c, m):
	if hp_bar:
		hp_bar.value = c
		hp_bar.max_value = m
