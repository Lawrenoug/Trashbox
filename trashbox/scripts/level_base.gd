
extends Node2D

# --- 【关键】这里定义了你报错缺少的变量 ---
# 请在编辑器里把 "技能战斗列表" 节点拖给 Battle Ui Node
@export var battle_ui_node: Control 
# 请在编辑器里把 "技能组背包" 节点拖给 Backpack Ui Node
@export var backpack_ui_node: Control 

func _ready():
	# 创建调试按钮
	_create_debug_skip_button()
	
	# 场景加载完毕，通知 C# 生成敌人
	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr:
		# 调用 EnterRoom 开始战斗，使用目标关卡索引作为参数
		print("LevelBase: 呼叫 RoomManager EnterRoom 生成第 %d 层敌人" % GlobalGameState.target_level_index)
		room_mgr.EnterRoom(GlobalGameState.target_level_index)
	else:
		print("错误：找不到 RoomManager 单例！可能未在进入关卡前创建")

# --- 离开逻辑 ---
func _on_skip_level_pressed():
	print("--- [流程] 请求离开关卡 ---")
	
	# === 调用 C# ExitRoom 清理敌人 ===
	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr:
		print("--- [流程] 调用 RoomManager.ExitRoom() 清理敌人 ---")
		room_mgr.ExitRoom()
	
	# === 强制等待 ===
	# 等待 2 帧。
	# 第1帧：让 C# 的逻辑跑完。
	# 第2帧：让 Godot 的 QueueFree 生效，确保节点真的从内存中断开了。
	await get_tree().process_frame
	await get_tree().process_frame
	
	
	print("--- [流程] C# 清理完毕，开始切换 ---")
	
	# === 计算进度并跳转 ===
	GlobalGameState.current_level_progress = GlobalGameState.target_level_index
	
	# 【新增】设置返回标志
	GlobalGameState.has_returned_from_level = true
	
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



# --- 辅助 UI ---
func _create_debug_skip_button():
	# 检查是否已经存在调试按钮，避免重复创建
	if has_node("DebugCanvasLayer"):
		return
		
	var layer = CanvasLayer.new()
	layer.name = "DebugCanvasLayer"
	layer.layer = 100  # 设置高图层确保在最前面
	add_child(layer)
	
	var btn = Button.new()
	btn.text = "DEBUG: 强制通关"
	# 调整到屏幕右下角更合适的位置
	btn.position = Vector2(get_viewport_rect().size.x - 320, 20)
	btn.custom_minimum_size = Vector2(300, 50)
	btn.modulate = Color(1, 0, 1, 1)  # 确保完全不透明
	btn.pressed.connect(_on_skip_level_pressed)
	layer.add_child(btn)
	
	print("调试按钮已创建，位置:", btn.position)
