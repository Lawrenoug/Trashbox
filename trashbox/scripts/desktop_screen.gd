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

# --- 3. 获取其他组件 ---
@onready var notification_popup = $NotificationPopup
@onready var notification_anim = $NotificationPopup/AnimationPlayer
@onready var notif_title = $NotificationPopup/HBoxContainer/VBoxContainer/Title
@onready var notif_msg = $NotificationPopup/HBoxContainer/VBoxContainer/Message
@onready var clock_label = $Taskbar/TaskbarItems/ClockLabel
@onready var start_button = $Taskbar/TaskbarItems/StartButton
@onready var start_menu = $StartMenu
@onready var btn_shutdown = $StartMenu/VBoxContainer/BtnShutdown

# --- 【新增】音效播放器 ---
# 请在 _ready 里动态创建，或者你在场景里加一个 AudioStreamPlayer 并引用
var sfx_player: AudioStreamPlayer

# --- 4. 全局聊天数据存储 (初始历史记录) ---
var global_chat_data = {
	"老板": "[color=#888888]--- 历史记录 ---[/color]\n[color=#e74c3c][b]老板:[/b][/color] 上周的进度太慢了。\n[color=#e74c3c][b]老板:[/b][/color] 周末记得加班赶一下。\n[color=#4a90e2][b]我:[/b][/color] 好的收到。\n",
	"华姐": """[color=#888888]--- 历史记录 ---[/color]
[color=#2ecc71][b]华姐:[/b][/color] 听说隔壁组全被裁了，吓死我了。
[color=#4a90e2][b]我:[/b][/color] 唉，希望能挺过这一波。
[color=#2ecc71][b]华姐:[/b][/color] 有空帮我看个 Bug 呗？
[color=#2ecc71][b]华姐:[/b][/color] 对了，听说服务器的后门密码还没改，还是默认的 [b]admin[/b]，这群运维真是太草率了。
"""
}
var is_recycle_bin_cleared = false 

func _ready():
	# 初始化：隐藏弹窗和菜单
	if notification_popup: notification_popup.visible = false
	if start_menu: start_menu.visible = false
	
	# 连接图标点击信号
	if 文件夹: 文件夹.pressed.connect(open_window.bind(AppFolderScene))
	if 回收站: 回收站.pressed.connect(open_window.bind(AppRecycleScene))
	if chatbro: chatbro.pressed.connect(open_window.bind(AppChatScene))
	if 引擎: 引擎.pressed.connect(open_window.bind(AppGodotScene))
	
	# 连接开始菜单信号
	if start_button: start_button.pressed.connect(_on_start_button_clicked)
	if btn_shutdown: btn_shutdown.pressed.connect(_on_shutdown_clicked)
	
	# 连接背景点击 (关闭菜单)
	var wallpaper = get_node_or_null("Wallpaper")
	if wallpaper: wallpaper.gui_input.connect(_on_wallpaper_input)

	# --- 【新增】初始化音效组件 ---
	sfx_player = AudioStreamPlayer.new()
	add_child(sfx_player)
	# 简单的提示音资源 (Godot 默认没有，你可以拖一个 wav 进来，或者用代码生成简单的波形)
	# 这里假设你之后会设置流，或者我们仅仅依靠视觉效果
	
	# --- 【核心】如果是从登录界面刚进来，触发剧情 ---
	# 我们通过判断是否自动打开引擎来区分（或者加个全局标记）
	# 这里简单处理：每次进入桌面都触发（方便测试），实际可加 GlobalGameState 标记
	if not GlobalGameState.should_open_engine_automatically:
		_play_intro_story()
	else:
		# 如果是打完关卡回来，直接打开引擎
		GlobalGameState.should_open_engine_automatically = false
		call_deferred("open_window", AppGodotScene)

# --- 剧情脚本 ---
func _play_intro_story():
	# 1. 等待 1 秒，让玩家看一眼桌面
	await get_tree().create_timer(1.0).timeout
	
	# 2. 播放提示音 (如果你有音频文件，请在这里 load)
	# var sound = load("res://trashbox/assets/audio/message.wav")
	# if sound: 
	# 	sfx_player.stream = sound
	# 	sfx_player.play()
	
	# 3. 图标抖动特效 (Tween)
	if chatbro:
		var tween = create_tween()
		var original_pos = chatbro.position
		# 左右快速抖动 5 次
		for i in range(5):
			tween.tween_property(chatbro, "position", original_pos + Vector2(5, 0), 0.05)
			tween.tween_property(chatbro, "position", original_pos - Vector2(5, 0), 0.05)
		tween.tween_property(chatbro, "position", original_pos, 0.05)
		
		# 等待抖动结束
		await tween.finished
	
	# 4. 自动打开聊天窗口
	open_window(AppChatScene)
	
	# 5. 等待窗口打开动画
	await get_tree().create_timer(0.5).timeout
	
	# 6. 生成老板的新消息 (推动剧情)
	incoming_message("老板", "那个垃圾清理系统做好了吗？")
	await get_tree().create_timer(1.5).timeout
	incoming_message("老板", "客户已经在催了，今天必须上线！")
	await get_tree().create_timer(1.0).timeout
	incoming_message("老板", "别磨蹭，赶紧打开引擎开始工作。")

# --- 原有函数保持不变 ---
func _on_start_button_clicked():
	if start_menu:
		start_menu.visible = not start_menu.visible
		if start_menu.visible: start_menu.move_to_front()

func _on_shutdown_clicked():
	get_tree().quit()

func _on_wallpaper_input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if start_menu and start_menu.visible: start_menu.visible = false

func open_window(scene_to_open: PackedScene):
	if scene_to_open == null:
		print("错误：场景为空！")
		return

	var window_instance = scene_to_open.instantiate()
	add_child(window_instance)
	
	# 如果是引擎，强制全屏 (保持不变)
	if scene_to_open == AppGodotScene:
		var screen_size = get_viewport_rect().size
		window_instance.position = Vector2.ZERO
		window_instance.size = screen_size
		window_instance.custom_minimum_size = screen_size
		
	else:
		# 其他应用（文件夹、聊天等）
		if window_instance.has_method("init_data"):
			window_instance.init_data(self)
		
		var win_size = window_instance.custom_minimum_size
		
		# --- 【修改】调大默认窗口尺寸 ---
		# 原来是 800, 600。你的屏幕是 1920x1080，这个尺寸有点显小。
		# 建议改为 1100, 750 左右，这样看起来更像一个正经的工作窗口。
		if win_size.x < 100: 
			win_size = Vector2(1100, 750) 
			
		window_instance.size = win_size
		
		# 计算居中位置 (保持不变)
		var center_pos = get_viewport_rect().size / 2
		var offset = Vector2(randf_range(-30, 30), randf_range(-30, 30))
		window_instance.position = center_pos - (win_size / 2) + offset
	
	window_instance.move_to_front()

func incoming_message(sender: String, msg: String):
	var color = "#e74c3c" if sender == "老板" else "#2ecc71"
	var formatted = "[color=" + color + "][b]" + sender + ":[/b][/color] " + msg + "\n"
	
	if global_chat_data.has(sender): global_chat_data[sender] += formatted
	else: global_chat_data[sender] = formatted
	
	show_notification("新消息: " + sender, msg)
	get_tree().call_group("ChatApps", "receive_message", sender, msg)

func show_notification(title_text: String, msg_text: String):
	if notification_popup and notification_anim:
		notification_popup.visible = true
		notif_title.text = title_text
		notif_msg.text = msg_text
		notification_anim.play("popup")
		var timer = get_tree().create_timer(3.0)
		await timer.timeout
		if notification_anim.has_animation("popup"):
			notification_anim.play_backwards("popup")
			await notification_anim.animation_finished
		notification_popup.visible = false

func _process(_delta):
	if clock_label:
		var time = Time.get_datetime_dict_from_system()
		clock_label.text = "%d/%02d\n%02d:%02d" % [time.month, time.day, time.hour, time.minute]
