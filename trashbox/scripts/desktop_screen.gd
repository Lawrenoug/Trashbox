extends Control

# --- 1. 预加载应用场景 ---
const AppFolderScene = preload("res://trashbox/scenes/main/app_folder.tscn")
const AppRecycleScene = preload("res://trashbox/scenes/main/app_recycle_bin.tscn")
const AppChatScene = preload("res://trashbox/scenes/main/app_chat.tscn")
const AppGodotScene = preload("res://trashbox/scenes/main/engine.tscn")

# --- 2. 获取桌面图标 ---
@onready var 文件夹: Button = $DesktopIcons/文件夹
@onready var 回收站: Button = $DesktopIcons/回收站
@onready var chatbro: Button = $DesktopIcons/Chatbro
@onready var 引擎: Button = $DesktopIcons/引擎

# --- 3. 获取弹窗通知节点 ---
@onready var notification_popup = $NotificationPopup
@onready var notification_anim = $NotificationPopup/AnimationPlayer
@onready var notif_title = $NotificationPopup/HBoxContainer/VBoxContainer/Title
@onready var notif_msg = $NotificationPopup/HBoxContainer/VBoxContainer/Message

# --- 4. 全局聊天数据存储 ---
var global_chat_data = {
	"老板": "[color=#888888]系统: 已经是好友了，开始聊天吧。[/color]\n",
	"华姐": "[color=#888888]系统: 已经是好友了，开始聊天吧。[/color]\n"
}
var is_recycle_bin_cleared = false 
func _ready():
	# 初始化：隐藏弹窗
	if notification_popup:
		notification_popup.visible = false

	# 连接图标点击信号
	if 文件夹: 文件夹.pressed.connect(open_window.bind(AppFolderScene))
	if 回收站: 回收站.pressed.connect(open_window.bind(AppRecycleScene))
	if chatbro: chatbro.pressed.connect(open_window.bind(AppChatScene))
	if 引擎: 引擎.pressed.connect(open_window.bind(AppGodotScene))

# --- [修改后] 通用的打开窗口逻辑 ---
func open_window(scene_to_open: PackedScene):
	if scene_to_open == null:
		print("错误：场景为空！")
		return

	var window_instance = scene_to_open.instantiate()
	add_child(window_instance)
	
	# === 核心修改：如果是引擎，强制全屏 ===
	if scene_to_open == AppGodotScene:
		# 获取屏幕大小 (1920x1080)
		var screen_size = get_viewport_rect().size
		# 设置位置为左上角 (0,0)
		window_instance.position = Vector2.ZERO
		# 设置大小为全屏
		window_instance.size = screen_size
		# (可选) 如果你想更彻底一点，也可以设置最小尺寸
		window_instance.custom_minimum_size = screen_size
		
	else:
		# === 其他应用：保持原来的小窗口逻辑 ===
		
		# 如果是聊天窗口，传递数据
		if window_instance.has_method("init_data"):
			window_instance.init_data(self)
		
		# 随机偏移位置
		var center_pos = get_viewport_rect().size / 2
		var offset = Vector2(randf_range(-30, 30), randf_range(-30, 30))
		
		# 获取窗口默认大小
		var win_size = window_instance.custom_minimum_size
		if win_size == Vector2.ZERO: 
			win_size = Vector2(400, 300)
			
		# 设置居中位置
		window_instance.position = center_pos - (win_size / 2) + offset
	
	# 确保新窗口在最上层
	window_instance.move_to_front()

# --- 剧情系统调用接口 ---
func incoming_message(sender: String, msg: String):
	var color = "#e74c3c" if sender == "老板" else "#2ecc71"
	var formatted = "[color=" + color + "][b]" + sender + ":[/b][/color] " + msg + "\n"
	
	if global_chat_data.has(sender):
		global_chat_data[sender] += formatted
	else:
		global_chat_data[sender] = formatted
	
	show_notification("新消息: " + sender, msg)
	get_tree().call_group("ChatApps", "receive_message", sender, msg)

func show_notification(title_text: String, msg_text: String):
	if notification_popup and notification_anim:
		notification_popup.visible = true
		notif_title.text = title_text
		notif_msg.text = msg_text
		
		# 播放弹出
		notification_anim.play("popup")
		
		# 【修复】创建一个计时器，等待 3 秒
		# 使用 create_timer 是最稳妥的，不受其他帧逻辑影响
		var timer = get_tree().create_timer(3.0)
		await timer.timeout
		
		# 播放消失 (倒放 popup 动画，或者是直接隐藏)
		# 如果你没有做消失动画，可以直接 notification_popup.visible = false
		if notification_anim.has_animation("popup"):
			notification_anim.play_backwards("popup")
			await notification_anim.animation_finished
		
		notification_popup.visible = false
