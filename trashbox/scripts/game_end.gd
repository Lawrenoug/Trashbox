extends Control

@onready var bg_rect = $CanvasLayer/ColorRect
@onready var label = $CanvasLayer/ColorRect/Label

func _ready():
	# 1. 初始化状态
	# 背景透明度设为 0 (一开始能看到上一帧的画面，或者设为黑色看你喜好)
	# 这里建议设为 0，这样就像是画面突然闪白或者是慢慢变白覆盖了游戏
	bg_rect.modulate.a = 0.0 
	
	# 文字透明度设为 0 (不可见)
	label.modulate.a = 0.0
	
	# 2. 开始动画序列
	_start_ending_sequence()

func _start_ending_sequence():
	var tween = create_tween()
	
	# 步骤 A: 屏幕慢慢变白 (持续 2 秒)
	# SetEase(Tween.EASE_IN) 会让变白的过程有一种加速感
	tween.tween_property(bg_rect, "modulate:a", 1.0, 2.0).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	
	# 步骤 B: 等待 1 秒 (此时屏幕全白)
	tween.tween_interval(1.0)
	
	# 步骤 C: 文字浮现 (持续 2 秒)
	tween.tween_property(label, "modulate:a", 1.0, 2.0)
	
	# 步骤 D: 停留展示 (持续 4 秒)
	tween.tween_interval(4.0)
	
	# 步骤 E: 结束回调 (退出游戏 或 返回主菜单)
	tween.tween_callback(_on_sequence_finished)

func _on_sequence_finished():
	print("Game Over. Version Shipped.")
	# 选项 1: 直接退出游戏
	#get_tree().quit()
	
	# 选项 2: 返回登录界面 (如果你想让它循环)
	get_tree().change_scene_to_file("res://trashbox/scenes/main/desktop_screen.tscn")
