extends Node2D

# --- 配置区域 ---
@export_group("事件设置")
@export var event_title: String = "系统通知"
@export_multiline var event_description: String = "发生了一个未知的运行时错误..."
@export var background_texture: Texture2D

@export_group("奖励配置 (仅代码仓库事件用)")
# 把你的技能 .tscn (例如 像素点.tscn) 拖到这个数组里，用于随机奖励
@export var reward_pool: Array[PackedScene] 

# --- 内部引用 ---
@onready var title_label = $CanvasLayer/Window/Title
@onready var desc_label = $CanvasLayer/Window/Description
@onready var btn_1 = $CanvasLayer/Window/HBox/Option1
@onready var btn_2 = $CanvasLayer/Window/HBox/Option2
@onready var btn_3 = $CanvasLayer/Window/HBox/Option3
@onready var hp_bar = $CanvasLayer/HUD/HPBar # 确保你把 HUD 复制过来了

var player_ref = null

func _ready():
	# 初始化 UI 文字
	if title_label: title_label.text = event_title
	if desc_label: desc_label.text = event_description
	
	# 连接按钮信号
	btn_1.pressed.connect(_on_option_1_clicked)
	btn_2.pressed.connect(_on_option_2_clicked)
	btn_3.pressed.connect(_on_option_3_clicked)
	
	# 寻找玩家
	# 因为是切换场景进来的，我们要像 level_base 那样等待玩家
	var player = get_node_or_null("Player")
	if player:
		_setup_player(player)

func _setup_player(player):
	player_ref = player
	# 连接血条信号 (和 level_base 一样)
	if player.has_signal("HealthChanged"):
		player.connect("HealthChanged", _update_hp_bar)
		_update_hp_bar(player.get("CurrentBlood"), player.get("MaxBlood"))

func _update_hp_bar(current, max_hp):
	if hp_bar:
		hp_bar.value = current
		hp_bar.max_value = max_hp

# --- 需要在子类或者具体场景中通过信号连接修改的逻辑 ---
# 这里提供默认的“关闭”逻辑，具体逻辑我们在后面三个不同的脚本里覆写
func _on_option_1_clicked():
	pass

func _on_option_2_clicked():
	pass

func _on_option_3_clicked():
	# 选项3通常是“离开”
	_leave_event()

func _leave_event():
	print("事件结束，返回地图或下一关")
	# 这里通常调用 Engine 的逻辑返回地图，或者直接 finish
	# 暂时先打印，实际整合时可能需要发信号给 Engine
	# 比如: get_tree().call_group("EngineUI", "return_to_map")
	
	# 简单处理：直接禁用所有按钮，显示“处理完成”
	btn_1.disabled = true
	btn_2.disabled = true
	btn_3.disabled = true
	desc_label.text += "\n\n[color=green]> 操作执行完毕。正在等待系统回收...[/color]"
