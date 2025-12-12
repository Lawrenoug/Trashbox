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

func _ready():
	super._ready()
	add_to_group("ChatApps")
	
	# 绑定按钮
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
	if current_chat_target == target_name: return # 防止重复刷新

	current_chat_target = target_name
	current_contact_label.text = target_name
	chat_log.clear()
	
	if chat_history.has(target_name):
		chat_log.text = chat_history[target_name]
	else:
		chat_log.text = "[color=#888888]（暂无更多消息）[/color]"
	
	await get_tree().process_frame
	chat_log.scroll_to_line(chat_log.get_line_count())

# --- 发送消息逻辑 (修复了输入框不清空的问题) ---
func _on_send_pressed(_text_from_enter = ""):
	var text = message_input.text
	if text.strip_edges() == "": return 
	if current_chat_target == "": return
	
	# === 【新增】拦截逻辑：如果老板正在训话，禁止发送 ===
	if current_chat_target == "老板":
		if desktop_ref and desktop_ref.get("is_boss_script_playing"):
			# 清空输入框
			message_input.clear()
			# 在聊天框显示一条灰色的系统提示，告诉玩家为什么发不出去
			var warning = "[color=#888888][i](系统拦截：对方正在输入中，无法插话...)[/i][/color]\n"
			chat_log.append_text(warning)
			return # 直接结束函数，不执行后面的发送和回复逻辑
	# =================================================
	
	# 1. 先把输入框清空
	message_input.clear()
		
	# 2. 显示玩家发送的消息
	var new_line = "[color=#4a90e2][b]我:[/b][/color] " + text + "\n"
	_append_message(current_chat_target, new_line)
	
	# 3. 触发自动回复
	_trigger_auto_reply(current_chat_target)

# --- 核心：根据人设自动回复 ---
func _trigger_auto_reply(target_name: String):
	# 模拟打字延迟 (随机 1.0 到 2.5 秒)
	var delay = randf_range(1.0, 2.5)
	await get_tree().create_timer(delay).timeout
	
	var reply_text = ""
	var sender_display_name = target_name # 默认显示名字
	var color = "#ffffff"
	
	# --- 定义回复内容池 ---
	match target_name:
		"老板":
			color = "#e74c3c"
			var responses = [
				"收到就好，别废话，去做事。",
				"这个不在我的关注范围内，我要结果。",
				"我不管过程，今晚下班前我要看到东西。",
				"你在教我做事？",
				"知道了。"
			]
			reply_text = responses.pick_random()
			
		"HR-Linda":
			color = "#9b59b6"
			var responses = [
				"亲，这是公司的规定哦~",
				"如果对考勤有异议，请填写流转单，审批流程大概需要15个工作日。",
				"亲，请注意你的言辞，这会被记录在综合评估里的。",
				"这也是为了帮你更好地成长呀。"
			]
			reply_text = responses.pick_random()
			
		"妈妈":
			color = "#e67e22"
			var responses = [
				"工作累不累呀？",
				"记得按时吃饭，别老吃外卖。",
				"没事，妈就是想听听你声音。",
				"钱够不够花？妈给你转点？",
				"早点睡，别把眼睛熬坏了。"
			]
			reply_text = responses.pick_random()
			
		"公司全员群":
			color = "#f1c40f"
			sender_display_name = "行政-小王" # 群里一般是行政在说话
			var responses = [
				"@我 请勿在群内闲聊，专心工作。",
				"@我 收到请回复，不要发无关内容。"
			]
			reply_text = responses.pick_random()
			
		"华姐":
			# 华姐被裁了，所以发消息会报错，增加恐怖感
			color = "#888888" # 灰色系统字
			sender_display_name = "系统"
			reply_text = "消息发送失败：该用户已注销或不存在。"
	
	# --- 生成回复 ---
	if reply_text != "":
		var formatted_reply = "[color=" + color + "][b]" + sender_display_name + ":[/b][/color] " + reply_text + "\n"
		
		# 接收消息 (这会自动触发 desktop_screen.gd 里的提示音和弹窗)
		# 注意：因为 app_chat 也是 ChatApps 组的一员，所以 receive_message 会被调用，从而更新 UI
		get_tree().call_group("ChatApps", "receive_message", sender_display_name, reply_text)

# --- 接收消息 ---
func receive_message(sender_name: String, message: String):
	# 这里是为了处理 DesktopScreen 广播过来的消息
	# 同时也处理上面 _trigger_auto_reply 发过来的消息
	
	# 简单的颜色映射，用于显示 (上面其实已经格式化过了，但为了保险起见)
	var color = "#2ecc71"
	if sender_name == "老板": color = "#e74c3c"
	elif sender_name == "HR-Linda": color = "#9b59b6"
	elif sender_name == "妈妈": color = "#e67e22"
	elif sender_name == "行政-小王" or sender_name == "公司全员群": color = "#f1c40f"
	elif sender_name == "系统": color = "#888888"
	
	var new_line = "[color=" + color + "][b]" + sender_name + ":[/b][/color] " + message + "\n"
	
	# 这里的 target 需要判断一下。
	# 如果是“行政-小王”发来的，应该归类到“公司全员群”的历史记录里
	# 如果是“系统”发来的（给华姐发的），应该归类到“华姐”里
	var target_key = sender_name
	if sender_name == "行政-小王": target_key = "公司全员群"
	if sender_name == "系统": target_key = "华姐"
	
	_append_message(target_key, new_line)

func _append_message(target: String, formatted_text: String):
	if not chat_history.has(target): chat_history[target] = ""
	chat_history[target] += formatted_text
	
	if desktop_ref and desktop_ref.global_chat_data.has(target):
		desktop_ref.global_chat_data[target] += formatted_text
	
	if current_chat_target == target:
		chat_log.append_text(formatted_text)
