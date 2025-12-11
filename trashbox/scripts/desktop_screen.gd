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
@onready var clock_label = $Taskbar/TaskbarItems/ClockLabel

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
	# 连接图标点击信号 (保持不变)
	if 文件夹: 文件夹.pressed.connect(open_window.bind(AppFolderScene))
	if 回收站: 回收站.pressed.connect(open_window.bind(AppRecycleScene))
	if chatbro: chatbro.pressed.connect(open_window.bind(AppChatScene))
	if 引擎: 引擎.pressed.connect(open_window.bind(AppGodotScene))

	# --- 【新增】检查全局标记，决定是否自动打开引擎 ---
	if GlobalGameState.should_open_engine_automatically:
		# 重置标记，防止下次正常登录时也打开
		GlobalGameState.should_open_engine_automatically = false
		
		# 稍微延迟一帧，确保桌面 UI 初始化完毕后再打开窗口
		# open_window 是你在 desktop_screen.gd 里定义的函数
		call_deferred("open_window", AppGodotScene)

# --- [修改后] 通用的打开窗口逻辑 ---
func open_window(scene_to_open: PackedScene):
	if scene_to_open == null:
		print("错误：场景为空！")
		return

	var window_instance = scene_to_open.instantiate()
	add_child(window_instance)
	
	# === 情况1：如果是引擎，强制全屏 ===
	if scene_to_open == AppGodotScene:
		var screen_size = get_viewport_rect().size
		window_instance.position = Vector2.ZERO
		window_instance.size = screen_size
		window_instance.custom_minimum_size = screen_size
		
	else:
		# === 情况2：其他应用（文件夹、聊天等） ===
		
		# 1. 传递数据 (如果是聊天窗口)
		if window_instance.has_method("init_data"):
			window_instance.init_data(self)
		
		# 2. 【核心修复】强制放大窗口尺寸
		# 获取窗口原本设定的最小尺寸
		var win_size = window_instance.custom_minimum_size
		
		# 如果尺寸太小(比如是0或者是旧的400)，给一个高清屏适合的默认值
		if win_size.x < 100 or win_size == Vector2.ZERO: 
			win_size = Vector2(800, 600) 
		else:
			pass
			
		# 应用尺寸
		window_instance.size = win_size
		# (可选) 更新最小尺寸防止被拖太小
		window_instance.custom_minimum_size = win_size 
		
		# 3. 计算居中位置
		var center_pos = get_viewport_rect().size / 2
		var offset = Vector2(randf_range(-30, 30), randf_range(-30, 30))
		
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

func _process(delta):
	_update_system_time()

func _update_system_time():
	if clock_label:
		# 1. 获取系统时间字典 (包含年、月、日、时、分、秒)
		var time = Time.get_datetime_dict_from_system()
		
		# 2. 格式化字符串
		# %02d 的意思是：如果是1位数，前面自动补0 (例如 9:5 -> 09:05)
		# 格式：小时:分钟
		#var time_str = "%02d:%02d" % [time.hour, time.minute]
		
		# 如果你想显示日期，可以用下面这行：
		var time_str = "%d/%02d\n%02d:%02d" % [time.month, time.day, time.hour, time.minute]
		
		# 3. 更新 UI
		clock_label.text = time_str
