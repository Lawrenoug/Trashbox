extends "res://trashbox/scripts/window_base.gd"

# --- 节点引用 ---
@onready var current_contact_label = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/Header/CurrentContactLabel
@onready var chat_log = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/ChatLog
@onready var message_input = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/InputArea/MessageInput
@onready var send_button = $BgColor/MainLayout/ContentSlot/SplitView/RightChatArea/ChatLayout/InputArea/SendButton

# 左侧联系人按钮
@onready var btn_boss = $BgColor/MainLayout/ContentSlot/SplitView/LeftSidebar/ContactList/BtnBoss
@onready var btn_colleague = $BgColor/MainLayout/ContentSlot/SplitView/LeftSidebar/ContactList/BtnColleague

# --- 数据存储 ---
# 本地缓存，初始化时会从桌面同步
var chat_history = {} 

# 对桌面的引用，用于回写数据
var desktop_ref = null

# 当前正在和谁聊天
var current_chat_target = ""

func _ready():
	super._ready() # 必须调用父类
	
	# 将自己加入组，这样桌面收到剧情消息时能找到我
	add_to_group("ChatApps")
	
	# 连接信号
	btn_boss.pressed.connect(_switch_chat_to.bind("老板"))
	# 注意：这里名字要和 chat_history 里的 Key 一致
	btn_colleague.pressed.connect(_switch_chat_to.bind("华姐"))
	
	send_button.pressed.connect(_on_send_pressed)
	message_input.text_submitted.connect(_on_send_pressed)

#初始化数据
func init_data(desktop_node):
	desktop_ref = desktop_node
	# 从桌面复制最新的聊天记录
	chat_history = desktop_node.global_chat_data.duplicate()
	_switch_chat_to("老板")

func _switch_chat_to(target_name: String):
	current_chat_target = target_name
	current_contact_label.text = target_name
	
	if chat_history.has(target_name):
		chat_log.text = chat_history[target_name]
	else:
		chat_log.text = ""
	
	await get_tree().process_frame
	chat_log.scroll_to_line(chat_log.get_line_count())

#发送消息 (玩家发送)
func _on_send_pressed(_text_from_enter = ""):
	var text = message_input.text
	if text.strip_edges() == "":
		return 
	
	if current_chat_target == "":
		return
		
	# 1. 格式化消息
	var new_line = "[color=#4a90e2][b]我:[/b][/color] " + text + "\n"
	
	# 2. 更新本地和全局数据
	_append_message(current_chat_target, new_line)
	
	# 3. 清空输入框
	message_input.clear()

# --- 接收消息 (实时接收，由 DesktopScreen 通过组调用) ---
func receive_message(sender_name: String, message: String):
	# 1. 格式化消息
	var color = "#e74c3c" if sender_name == "老板" else "#2ecc71"
	var new_line = "[color=" + color + "][b]" + sender_name + ":[/b][/color] " + message + "\n"
	
	# 2. 更新显示
	_append_message(sender_name, new_line)

# --- 内部工具：追加消息并刷新 ---
func _append_message(target: String, formatted_text: String):
	# 1. 更新本地数据
	if not chat_history.has(target):
		chat_history[target] = ""
	chat_history[target] += formatted_text
	
	# 2. 同步回桌面全局数据 (防止窗口关闭后数据丢失)
	if desktop_ref and desktop_ref.global_chat_data.has(target):
		desktop_ref.global_chat_data[target] += formatted_text
	
	# 3. 如果当前正在看这个人，直接更新UI
	if current_chat_target == target:
		chat_log.append_text(formatted_text)
