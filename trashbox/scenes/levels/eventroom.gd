extends Node2D

# --- 配置区域 ---
@export_group("事件设置")
@export var event_title: String = "系统通知"
@export_multiline var event_description: String = "发生了一个未知的运行时错误..."
@export var background_texture: Texture2D

@export_group("奖励配置")
@export var reward_pool: Array[PackedScene] 

# --- UI 引用 ---
@onready var title_label = $CanvasLayer/Window/VBoxContainer/Title
@onready var desc_label = $CanvasLayer/Window/VBoxContainer/Description
@onready var btn_1 = $CanvasLayer/Window/VBoxContainer/HBox/Option1
@onready var btn_2 = $CanvasLayer/Window/VBoxContainer/HBox/Option2
@onready var btn_3 = $CanvasLayer/Window/VBoxContainer/HBox/Option3
@onready var hp_bar = $CanvasLayer/HUD/HPBar 

# --- 【关键】这里定义了你报错缺少的变量 ---
# 请在编辑器里拖拽赋值
@export var battle_ui_node: Control 
@export var backpack_ui_node: Control

var player_ref = null
var current_global_player = null

func _ready():
	# 1. 初始化文字
	if title_label: title_label.text = event_title
	if desc_label: desc_label.text = event_description
	
	# 2. 连接按钮
	if btn_1: btn_1.pressed.connect(_on_option_1_clicked)
	if btn_2: btn_2.pressed.connect(_on_option_2_clicked)
	if btn_3: btn_3.pressed.connect(_on_option_3_clicked)
	
	# 3. 搬运玩家
	_setup_global_player()
	
	# 4. 读档
	_load_all_data()
	
	# 5. 【UI 显示控制：事件房全部显示】
	if battle_ui_node:
		battle_ui_node.visible = true
	if backpack_ui_node:
		backpack_ui_node.visible = true

# --- 玩家搬运逻辑 ---
func _setup_global_player():
	var temp_player = get_node_or_null("Player")
	if temp_player: temp_player.queue_free()
		
	if has_node("/root/GlobalPlayer"):
		current_global_player = get_node("/root/GlobalPlayer")
		if current_global_player.get_parent():
			current_global_player.get_parent().remove_child(current_global_player)
		add_child(current_global_player)
		current_global_player.visible = false 
		_setup_player_ref(current_global_player)

func _setup_player_ref(player):
	player_ref = player
	if player.has_signal("HealthChanged"):
		player.connect("HealthChanged", _update_hp_bar)
		if player.get("CurrentBlood"):
			_update_hp_bar(player.get("CurrentBlood"), player.get("MaxBlood"))

func _update_hp_bar(current, max_hp):
	if hp_bar:
		hp_bar.value = current
		hp_bar.max_value = max_hp

# --- 读档逻辑 ---
func _load_all_data():
	if battle_ui_node:
		_load_grid_data(battle_ui_node.get_node("GridContainer"), GlobalGameState.saved_skill_paths)
	if backpack_ui_node:
		_load_grid_data(backpack_ui_node.get_node("GridContainer"), GlobalGameState.saved_backpack_paths)

func _load_grid_data(grid_node: Control, paths: Array[String]):
	if not grid_node or paths.is_empty(): return
	
	for slot in grid_node.get_children():
		for child in slot.get_children(): child.queue_free()
	
	var slots = grid_node.get_children()
	for i in range(paths.size()):
		if i >= slots.size(): break
		var path = paths[i]
		if path and path != "":
			var scene = load(path)
			if scene:
				slots[i].add_child(scene.instantiate())

# --- 存档逻辑 ---
func _save_all_data():
	if battle_ui_node:
		GlobalGameState.saved_skill_paths = _save_grid_data(battle_ui_node.get_node("GridContainer"))
	if backpack_ui_node:
		GlobalGameState.saved_backpack_paths = _save_grid_data(backpack_ui_node.get_node("GridContainer"))
	print("EventRoom: 数据已保存")

func _save_grid_data(grid_node: Control) -> Array[String]:
	var result: Array[String] = []
	if not grid_node: return result
	for slot in grid_node.get_children():
		if slot.get_child_count() > 0:
			var skill = slot.get_child(0)
			if skill.scene_file_path != "":
				result.append(skill.scene_file_path)
	return result

# --- 按钮回调 ---
func _on_option_1_clicked(): pass
func _on_option_2_clicked(): pass
func _on_option_3_clicked(): _leave_event()

# --- 离开逻辑 ---
func _leave_event():
	print("事件结束，正在存档并离开...")
	
	if btn_1: btn_1.disabled = true
	if btn_2: btn_2.disabled = true
	if btn_3: btn_3.disabled = true
	if desc_label: desc_label.text += "\n\n[color=yellow]>> 正在保存状态并退出...[/color]"
	
	_save_all_data()
	
	await get_tree().create_timer(1.0).timeout
	
	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr: room_mgr.ExitRoom()
	
	if current_global_player:
		remove_child(current_global_player)
		get_tree().root.add_child(current_global_player)
		current_global_player.visible = false
	
	GlobalGameState.current_level_progress += 1
	GlobalGameState.should_open_engine_automatically = true
	get_tree().change_scene_to_file(GlobalGameState.desktop_scene_path)
