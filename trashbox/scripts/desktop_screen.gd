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

# --- 音效资源 ---
@export var notification_sound: AudioStream # 消息提示音
@export var shake_sound: AudioStream        # 抖动提示音

# --- 音效播放器 ---
var sfx_player: AudioStreamPlayer
var is_boss_script_playing = false
# --- 4. 全局聊天数据存储 (丰富版) ---
# 注意：Key 的名字必须和 app_chat.gd 里绑定的一致
var global_chat_data = {
	"老板": "[color=#888888]--- 历史记录 ---[/color]\n[color=#e74c3c][b]老板:[/b][/color] 昨天发的日报太敷衍了，重写。\n[color=#4a90e2][b]我:[/b][/color] 好的收到。\n",
	
	"华姐": """[color=#888888]--- 历史记录 ---[/color]
[color=#2ecc71][b]华姐:[/b][/color] 听说隔壁组全被裁了，吓死我了。
[color=#4a90e2][b]我:[/b][/color] 唉，希望能挺过这一波。
[color=#2ecc71][b]华姐:[/b][/color] 对了，听说服务器的后门密码还没改，还是默认的 [b]admin[/b]，这群运维真是太草率了。
""",

"公司全员群": """[color=#888888]--- 历史记录 ---[/color]
[color=#f1c40f][b]行政-小王:[/b][/color] [全员通知] 严禁在工位上吃早饭，违者罚款 200。
[color=#95a5a6][b]技术部-张三:[/b][/color] 收到。
[color=#95a5a6][b]市场部-李四:[/b][/color] 收到。
[color=#95a5a6][b]美术-王五:[/b][/color] 收到。
[color=#e74c3c][b]老板:[/b][/color] @全体人员 最近电费超标了，最后走的人记得关空调！别让我再看到空调开一整晚！
[color=#95a5a6][b]实习生-小赵:[/b][/color] 好的老板，收到。
[color=#95a5a6][b]财务-刘姐:[/b][/color] 收到。
[color=#f1c40f][b]行政-小王:[/b][/color] 另外，本周六全体“自愿”团建，地点在公司会议室，内容是业务复盘会，请大家准时参加。
[color=#95a5a6][b]某同事:[/b][/color] （撤回了一条消息）
[color=#f1c40f][b]行政-小王:[/b][/color] [温馨提示] 茶水间冰箱里的过期食品将在今天下班后统一清理。
[color=#95a5a6][b]技术部-老陈:[/b][/color] 谁把臭豆腐放冰箱了？？？整个那一层全是味儿。
""",

	"HR-Linda": """[color=#888888]--- 历史记录 ---[/color]
[color=#9b59b6][b]HR-Linda:[/b][/color] 亲，在吗？
[color=#9b59b6][b]HR-Linda:[/b][/color] 关于你的绩效改进计划(PIP)，我们需要聊聊。
[color=#9b59b6][b]HR-Linda:[/b][/color] 还有，你的加班时长不够饱和，记得多努努力哦~
""",

	"妈妈": """[color=#888888]--- 历史记录 ---[/color]
[color=#e67e22][b]妈妈:[/b][/color] 儿子，最近工作忙吗？
[color=#e67e22][b]妈妈:[/b][/color] 天冷了记得加衣裳。
[color=#e67e22][b]妈妈:[/b][/color] 别老熬夜，身体是自己的，实在太累就回家吧。
[color=#4a90e2][b]我:[/b][/color] 妈，我挺好的，不累。
"""
}
var is_recycle_bin_cleared = false 

func _ready():
	if notification_popup: notification_popup.visible = false
	if start_menu: start_menu.visible = false
	
	if 文件夹: 文件夹.pressed.connect(open_window.bind(AppFolderScene))
	if 回收站: 回收站.pressed.connect(open_window.bind(AppRecycleScene))
	if chatbro: chatbro.pressed.connect(open_window.bind(AppChatScene))
	if 引擎: 引擎.pressed.connect(open_window.bind(AppGodotScene))
	
	if start_button: start_button.pressed.connect(_on_start_button_clicked)
	if btn_shutdown: btn_shutdown.pressed.connect(_on_shutdown_clicked)
	
	var wallpaper = get_node_or_null("Wallpaper")
	if wallpaper: wallpaper.gui_input.connect(_on_wallpaper_input)

	sfx_player = AudioStreamPlayer.new()
	add_child(sfx_player)
	
	if not GlobalGameState.should_open_engine_automatically:
		_play_intro_story()
	else:
		GlobalGameState.should_open_engine_automatically = false
		call_deferred("open_window", AppGodotScene)

# --- 剧情脚本 (8条消息，匀速 1.5s) ---
func _play_intro_story():
	# === 【关键】剧情开始，锁定状态 ===
	is_boss_script_playing = true
	# ==============================

	# 1. 初始等待
	await get_tree().create_timer(1.0).timeout
	
	# 2. 图标抖动特效 (播放 shake_sound)
	if chatbro:
		if shake_sound and sfx_player:
			sfx_player.stream = shake_sound
			sfx_player.play()
		elif notification_sound and sfx_player:
			sfx_player.stream = notification_sound
			sfx_player.play()
			
		var tween = create_tween()
		var original_pos = chatbro.position
		for i in range(5):
			tween.tween_property(chatbro, "position", original_pos + Vector2(5, 0), 0.05)
			tween.tween_property(chatbro, "position", original_pos - Vector2(5, 0), 0.05)
		tween.tween_property(chatbro, "position", original_pos, 0.05)
		await tween.finished
	
	# 3. 自动打开聊天窗口
	open_window(AppChatScene)
	
	# --- 消息流程 ---
	await get_tree().create_timer(1.0).timeout
	incoming_message("老板", "那个垃圾清理系统做好了吗？")
	
	await get_tree().create_timer(1.5).timeout
	incoming_message("老板", "客户已经在催了，今天必须上线！")
	
	await get_tree().create_timer(1.5).timeout
	incoming_message("老板", "群里怎么不说话？")
	
	await get_tree().create_timer(1.5).timeout
	incoming_message("老板", "看到消息回话！")
	
	await get_tree().create_timer(1.5).timeout
	incoming_message("老板", "别磨蹭，赶紧打开引擎开始工作。")
	
	# === 【关键】剧情结束，解锁状态 ===
	is_boss_script_playing = false

# --- 以下保持不变 ---
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
	
	if scene_to_open == AppGodotScene:
		var screen_size = get_viewport_rect().size
		window_instance.position = Vector2.ZERO
		window_instance.size = screen_size
		window_instance.custom_minimum_size = screen_size
	else:
		if window_instance.has_method("init_data"):
			window_instance.init_data(self)
		
		var win_size = window_instance.custom_minimum_size
		if win_size.x < 100: 
			win_size = Vector2(1100, 750) 
		window_instance.size = win_size
		
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
	
	if notification_sound and sfx_player:
		sfx_player.stream = notification_sound
		sfx_player.play()

func show_notification(title_text: String, msg_text: String):
	if notification_popup and notification_anim:
		notification_popup.visible = true
		notif_title.text = title_text
		notif_msg.text = msg_text
		if notification_anim.has_animation("popup"):
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
