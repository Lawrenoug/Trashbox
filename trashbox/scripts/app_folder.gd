extends "res://trashbox/scripts/window_base.gd"

# --- 引用 ---
@onready var grid = $BgColor/MainLayout/ContentSlot/ScrollContainer/GridContainer
@onready var title_label = $BgColor/MainLayout/TitleBar/TitleLabel

# --- 素材 (请替换为你真实的图标路径) ---
const ICON_FOLDER = preload("res://trashbox/assets/sprites/folder.png")
const ICON_TXT = preload("res://trashbox/assets/sprites/txt.png")
const ICON_IMG = preload("res://trashbox/assets/sprites/jpg.png")
const ICON_LOCK = preload("res://trashbox/assets/sprites/lock.png") # 找个锁的图标
# 文件查看器场景
const FileViewerScene = preload("res://trashbox/scenes/main/file_viewer.tscn")

# --- 核心：文件系统数据结构 ---
# 这是一个嵌套字典，模拟文件夹结构
var file_system = {
	"root": {
		"D_Drive": {
			"name": "D:/Personal (私人)", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"GameDev_Dream": {
					"name": "GameDev_Dream", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"草图.png": {"type": "image", "icon": ICON_IMG, "data": "res://trashbox/assets/sprites/boss.png"},
						"Demo_v0.1.exe": {"type": "file", "icon": ICON_TXT, "data": "系统错误：文件已损坏。\n(创建时间: 5年前)"}
					}
				}
			}
		},
		"C_Drive": {
			"name": "C:/Work (工作)", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"Project_X": {
					"name": "Project_X_Final", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"1.doc": {"type": "file", "icon": ICON_TXT, "data": "需求文档_v1"},
						"2.doc": {"type": "file", "icon": ICON_TXT, "data": "需求文档_v2_最终版"},
						"3.doc": {"type": "file", "icon": ICON_TXT, "data": "需求文档_打死不改版"},
						"4.doc": {"type": "file", "icon": ICON_TXT, "data": "需求文档_老板脑子有坑版"}
					}
				},
				"Confidential": {
					"name": "绝密资料(加密)", "type": "locked_folder", "icon": ICON_LOCK,
					"password": "admin", # 密码
					"content": {
						"裁员名单.xlsx": {"type": "file", "icon": ICON_TXT, "data": "...张三, 李四, (主角的名字)..."},
						"AI计划.pdf": {"type": "file", "icon": ICON_TXT, "data": "计划目标：替代所有碳基程序员。"}
					}
				}
			}
		}
	}
}

# 当前所在的目录节点
var current_dir_data = {}
# 路径栈，用于返回上一级
var path_stack = []

func _ready():
	super._ready()
	# 初始化进入根目录
	current_dir_data = file_system["root"]
	_refresh_view()

func _refresh_view():
	# 清空现有图标
	for child in grid.get_children():
		child.queue_free()
	
	# 1. 如果不是根目录，添加“返回上一级”按钮
	if path_stack.size() > 0:
		var btn_back = _create_icon_btn(".. (返回)", ICON_FOLDER)
		btn_back.pressed.connect(_on_back_pressed)
		grid.add_child(btn_back)
	
	# 2. 遍历当前目录数据生成图标
	for key in current_dir_data:
		var item = current_dir_data[key]
		var btn = _create_icon_btn(item.get("name", key), item["icon"])
		
		# 连接点击信号
		if item["type"] == "folder":
			btn.pressed.connect(_on_folder_clicked.bind(item["content"]))
		elif item["type"] == "locked_folder":
			btn.pressed.connect(_on_locked_folder_clicked.bind(item))
		elif item["type"] == "file":
			btn.pressed.connect(_on_file_clicked.bind(key, item["data"]))
		elif item["type"] == "image":
			btn.pressed.connect(_on_image_clicked.bind(key, item["data"]))
			
		grid.add_child(btn)

# --- 交互逻辑 ---

func _on_folder_clicked(next_dir_content):
	# 记录当前目录以便返回
	path_stack.append(current_dir_data)
	# 进入下一级
	current_dir_data = next_dir_content
	_refresh_view()

func _on_back_pressed():
	if path_stack.size() > 0:
		current_dir_data = path_stack.pop_back()
		_refresh_view()

func _on_locked_folder_clicked(item_data):
	# 这里简单起见，用 OS.alert 或者 print 模拟，理想情况是弹出一个 LineEdit 小窗口
	# 为了不增加额外的UI工作量，我们做一个简化：
	# 点击后控制台输入，或者默认解锁
	print("请输入密码: ")
	
	# === 简易密码弹窗逻辑 (你可以后续完善) ===
	# 这里我们做一个极其简化的处理：如果玩家按住 Shift 点击，就算破解成功
	if Input.is_key_pressed(KEY_SHIFT):
		_on_folder_clicked(item_data["content"])
	else:
		# 实际上你应该弹出一个带 LineEdit 的小窗口 WindowBase
		# 这里我们暂时用标题栏提示
		title_label.text = "需要密码！(提示: 按住Shift点击模拟黑客破解)"
		await get_tree().create_timer(2.0).timeout
		title_label.text = "我的电脑"

func _on_file_clicked(filename, content):
	var viewer = FileViewerScene.instantiate()
	get_parent().add_child(viewer) # 添加到桌面
	viewer.setup_text(filename, content)
	# 稍微偏移一点位置
	viewer.position += Vector2(20, 20)

func _on_image_clicked(filename, path):
	var viewer = FileViewerScene.instantiate()
	get_parent().add_child(viewer)
	viewer.setup_image(filename, path)

# --- 工具：生成按钮 ---
func _create_icon_btn(text, icon):
	var btn = Button.new()
	btn.text = text
	btn.icon = icon
	btn.icon_alignment = HORIZONTAL_ALIGNMENT_CENTER
	btn.vertical_icon_alignment = VERTICAL_ALIGNMENT_TOP
	btn.custom_minimum_size = Vector2(80, 100)
	btn.flat = true
	btn.clip_text = true
	return btn
