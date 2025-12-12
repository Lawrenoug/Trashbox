extends "res://trashbox/scripts/window_base.gd"

# --- 节点引用 ---
@onready var current_contact_label = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/Header/CurrentContactLabel
@onready var chat_log = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/ChatLog
@onready var message_input = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/InputArea/MessageInput
@onready var send_button = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/InputArea/SendButton

# --- 左侧联系人按钮 ---
@onready var btn_boss = $BgColor/MainLayout/ContentSlot/SplitView/LeftSidebar/ContactList/BtnBoss
@onready var btn_colleague = $BgColor/MainLayout/ContentSlot/SplitView/LeftSidebar/ContactList/BtnColleague
@onready var btn_group = $BgColor/MainLayout/ContentSlot/SplitView/LeftSidebar/ContactList/BtnGroup
@onready var btn_hr = $BgColor/MainLayout/ContentSlot/SplitView/LeftSidebar/ContactList/BtnHR
@onready var btn_mom = $BgColor/MainLayout/ContentSlot/SplitView/LeftSidebar/ContactList/BtnMom

# --- 数据存储 ---
var chat_history = {} 
var desktop_ref = null
var current_chat_target = ""

# --- 【新增】防刷屏限制 ---
const MAX_TEXT_LENGTH = 3000 # 字符上限
var has_triggered_bug_egg = false # 是否触发过彩蛋

func _ready():
	super._ready()
	add_to_group("ChatApps")
	
	if btn_boss: btn_boss.pressed.connect(_switch_chat_to.bind("老板"))
	if btn_colleague: btn_colleague.pressed.connect(_switch_chat_to.bind("华姐"))
	if btn_group: btn_group.pressed.connect(_switch_chat_to.bind("公司全员群"))
	if btn_hr: btn_hr.pressed.connect(_switch_chat_to.bind("HR-Linda"))
	if btn_mom: btn_mom.pressed.connect(_switch_chat_to.bind("妈妈"))
	
	send_button.pressed.connect(_on_send_pressed)
	message_input.text_submitted.connect(_on_send_pressed)

func init_data(desktop_node):
	desktop_ref = desktop_node
	chat_history = desktop_node.global_chat_data.duplicate()
	_switch_chat_to("老板")

func _switch_chat_to(target_name: String):
	# 防止点击当前正在聊的人导致刷新
	if current_chat_target == target_name: return

	current_chat_target = target_name
	current_contact_label.text = target_name
	chat_log.clear()
	
	if chat_history.has(target_name):
		var content = chat_history[target_name]
		
		# === 【新增】检测文本是否溢出 (Bug 彩蛋) ===
		if content.length() > MAX_TEXT_LENGTH:
			# 如果太长了，触发彩蛋
			_trigger_lazy_bug_popup()
			# 截断显示，防止卡顿
			chat_log.text = content.substr(0, 1000) + "\n\n[color=red][SYSTEM ERROR: 内存溢出][/color]"
		else:
			chat_log.text = content
	else:
		chat_log.text = "[color=#888888]（暂无更多消息）[/color]"
	
	await get_tree().process_frame
	chat_log.scroll_to_line(chat_log.get_line_count())

# --- 【新增】Bug 彩蛋逻辑 ---
func _trigger_lazy_bug_popup():
	# 避免重复弹窗烦死玩家，只弹一次或者少弹几次
	if has_triggered_bug_egg: return
	has_triggered_bug_egg = true
	
	var dialog = AcceptDialog.new()
	dialog.title = "系统崩溃 (Fatal Error)"
	dialog.dialog_text = "别点了！这个bug懒得修了！！！！\n(文本长度已达上限)"
	
	# 设置窗口大小
	dialog.min_size = Vector2i(300, 150)
	
	add_child(dialog)
	dialog.popup_centered()
	
	# 播放一个警告音效 (如果有 sfx_player)
	if desktop_ref and desktop_ref.shake_sound and desktop_ref.sfx_player:
		desktop_ref.sfx_player.stream = desktop_ref.shake_sound
		desktop_ref.sfx_player.play()

# --- 发送消息逻辑 ---
func _on_send_pressed(_text_from_enter = ""):
	var text = message_input.text
	if text.strip_edges() == "": return 
	if current_chat_target == "": return
	
	# 1. 拦截逻辑：老板说话时禁止插嘴
	if current_chat_target == "老板":
		if desktop_ref and desktop_ref.get("is_boss_script_playing"):
			message_input.clear()
			chat_log.append_text("[color=#888888][i](系统拦截：对方正在输入中，无法插话...)[/i][/color]\n")
			return 
	
	message_input.clear()
	var new_line = "[color=#4a90e2][b]我:[/b][/color] " + text + "\n"
	_append_message(current_chat_target, new_line)
	_trigger_auto_reply(current_chat_target)

# --- 自动回复 ---
func _trigger_auto_reply(target_name: String):
	var delay = randf_range(1.0, 2.5)
	await get_tree().create_timer(delay).timeout
	
	var reply_text = ""
	var sender_display_name = target_name 
	var color = "#ffffff"
	
	match target_name:
		"老板":
			color = "#e74c3c"
			reply_text = ["收到就好，别废话，去做事。", "这个不在我的关注范围内。", "今晚下班前我要看到东西。", "你在教我做事？"].pick_random()
		"HR-Linda":
			color = "#9b59b6"
			reply_text = ["亲，这是公司的规定哦~", "请填写流转单。", "请注意你的言辞。"].pick_random()
		"妈妈":
			color = "#e67e22"
			reply_text = ["工作累不累呀？", "记得按时吃饭。", "妈给你转点钱？"].pick_random()
		"公司全员群":
			color = "#f1c40f"
			sender_display_name = "行政-小王"
			reply_text = ["@我 请勿在群内闲聊。", "[系统消息] 全员禁言中。"].pick_random()
		"华姐":
			color = "#888888"
			sender_display_name = "系统"
			reply_text = "消息发送失败：该用户已注销或不存在。"
	
	if reply_text != "":
		get_tree().call_group("ChatApps", "receive_message", sender_display_name, reply_text)

# --- 接收消息 ---
func receive_message(sender_name: String, message: String):
	var color = "#2ecc71"
	if sender_name == "老板": color = "#e74c3c"
	elif sender_name == "HR-Linda": color = "#9b59b6"
	elif sender_name == "妈妈": color = "#e67e22"
	elif sender_name == "行政-小王" or sender_name == "公司全员群": color = "#f1c40f"
	elif sender_name == "系统": color = "#888888"
	
	var new_line = "[color=" + color + "][b]" + sender_name + ":[/b][/color] " + message + "\n"
	
	var target_key = sender_name
	if sender_name == "行政-小王": target_key = "公司全员群"
	if sender_name == "系统": target_key = "华姐"
	
	_append_message(target_key, new_line)

func _append_message(target: String, formatted_text: String):
	if not chat_history.has(target): chat_history[target] = ""
	
	# === 【防刷屏检查】 ===
	# 如果已经溢出了，就不再往历史记录里加垃圾数据了
	if chat_history[target].length() > MAX_TEXT_LENGTH:
		return 
	# ====================
	
	chat_history[target] += formatted_text
	
	if desktop_ref and desktop_ref.global_chat_data.has(target):
		desktop_ref.global_chat_data[target] += formatted_text
	
	if current_chat_target == target:
		chat_log.append_text(formatted_text)
