extends Node2D

@onready var hp_bar = $HUD/HPBar

func _ready():
	# --- 原有逻辑 ---
	var player = get_node_or_null("Player") 
	if player:
		_connect_player(player)
	
	# --- 【新增】生成测试用的跳过按钮 ---
	_create_debug_skip_button()

# --- 【新增】动态创建UI ---
func _create_debug_skip_button():
	# 创建一个独立的 CanvasLayer 确保按钮在最上层
	var debug_layer = CanvasLayer.new()
	debug_layer.name = "DebugLayer"
	add_child(debug_layer)
	
	var btn = Button.new()
	btn.text = "DEBUG: 强制通关 (返回地图)"
	btn.global_position = Vector2(1600, 20) # 放在右上角
	btn.custom_minimum_size = Vector2(300, 50)
	btn.modulate = Color(1, 0, 1) # 紫色显眼一点
	
	# 连接点击信号
	btn.pressed.connect(_on_skip_level_pressed)
	debug_layer.add_child(btn)

# --- 【新增】跳过逻辑 ---
func _on_skip_level_pressed():
	print("DEBUG: 强制通关！")
	
	# 1. 更新全局进度 (进度 +1)
	GlobalGameState.current_level_progress += 1
	
	# 2. 【核心修改】设置标记：告诉桌面“回去后立刻打开引擎”
	GlobalGameState.should_open_engine_automatically = true
	
	# 3. 切换回桌面
	get_tree().change_scene_to_file(GlobalGameState.desktop_scene_path)

# --- 原有逻辑保持不变 ---
func setup_player(player_node):
	_connect_player(player_node)

func _connect_player(player):
	if player.has_signal("HealthChanged"):
		player.connect("HealthChanged", _on_health_changed)
		hp_bar.value = player.get("blood") if player.get("blood") else 100
		hp_bar.max_value = player.get("maxBlood") if player.get("maxBlood") else 100

func _on_health_changed(current, max_hp):
	hp_bar.value = current
	hp_bar.max_value = max_hp
