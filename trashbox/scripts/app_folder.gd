extends "res://trashbox/scripts/window_base.gd"

# --- 引用 ---
@onready var grid = $BgColor/MainLayout/ContentSlot/ScrollContainer/GridContainer
@onready var title_label = $BgColor/MainLayout/TitleBar/TitleLabel

# --- 素材 (请确保这些图片在你的 assets 文件夹里) ---
# 如果没有专用图标，Godot 会显示默认白色块，不影响逻辑运行
const ICON_FOLDER = preload("res://trashbox/assets/sprites/folder.png")
const ICON_TXT = preload("res://trashbox/assets/sprites/txt.png")
const ICON_IMG = preload("res://trashbox/assets/sprites/jpg.png")
const ICON_LOCK = preload("res://trashbox/assets/sprites/lock.png") 

# 文件查看器场景
const FileViewerScene = preload("res://trashbox/scenes/main/file_viewer.tscn")

# --- 核心：文件系统数据结构 ---
var file_system = {
	"root": {
		"D_Drive": {
			"name": "D:/Personal", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"Photos": {
					"name": "相册", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"cat.png": {"type": "image", "icon": ICON_IMG, "data": "res://trashbox/assets/sprites/boss.png"}, # 暂时用现有素材
						"family.png": {"type": "image", "icon": ICON_IMG, "data": "res://trashbox/assets/sprites/huajie.png"}
					}
				},
				"Games": {
					"name": "Steam(已卸载)", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"readme.txt": {"type": "file", "icon": ICON_TXT, "data": "为了项目上线，我已经把游戏都删了。\n加油，做完这单就赎身！"}
					}
				}
			}
		},
		"C_Drive": {
			"name": "C:/Work", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"Project_X": {
					"name": "Project_Trashbox", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"需求_v1.doc": {"type": "file", "icon": ICON_TXT, "data": "老板：我要一个五彩斑斓的黑。"},
						"需求_v2.doc": {"type": "file", "icon": ICON_TXT, "data": "老板：要加入大逃杀模式。"},
						"需求_v1024.doc": {"type": "file", "icon": ICON_TXT, "data": "老板：还是改回第一版吧。"},
						"BugList.txt": {"type": "file", "icon": ICON_TXT, "data": "1. 角色会穿墙\n2. 存档丢失\n3. 咖啡机不工作\n4. 我不想干了"}
					}
				},
				"Confidential": {
					"name": "绝密资料(加密)", "type": "locked_folder", "icon": ICON_LOCK,
					"password": "admin", # 这是解锁密码
					"content": {
						"裁员计划.xlsx": {"type": "file", "icon": ICON_TXT, "data": "裁员名单：\n1. 所有不加班的人\n2. 发量太多的人\n3. 你"},
						"AI替换人类.pdf": {"type": "file", "icon": ICON_TXT, "data": "计划代号：Skynet。\n目标：接管公司服务器。"}
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
# 路径名栈，用于显示标题
var name_stack = ["我的电脑"]

func _ready():
	super._ready()
	current_dir_data = file_system["root"]
	_refresh_view()

func _refresh_view():
	# 清空现有图标
	for child in grid.get_children():
		child.queue_free()
	
	# 更新标题栏路径
	title_label.text = "/".join(name_stack)
	
	# 1. 如果不是根目录，添加“返回”按钮
	if path_stack.size() > 0:
		var btn_back = _create_icon_btn(".. (返回)", ICON_FOLDER)
		btn_back.pressed.connect(_on_back_pressed)
		grid.add_child(btn_back)
	
	# 2. 遍历生成图标
	for key in current_dir_data:
		var item = current_dir_data[key]
		var icon = item.get("icon", ICON_FOLDER) # 防止漏填icon报错
		var btn = _create_icon_btn(item.get("name", key), icon)
		
		# 根据类型连接信号
		if item["type"] == "folder":
			btn.pressed.connect(_on_folder_clicked.bind(item["content"], item["name"]))
		elif item["type"] == "locked_folder":
			btn.pressed.connect(_on_locked_folder_clicked.bind(item))
		elif item["type"] == "file":
			btn.pressed.connect(_on_file_clicked.bind(item.get("name", key), item["data"]))
		elif item["type"] == "image":
			btn.pressed.connect(_on_image_clicked.bind(item.get("name", key), item["data"]))
			
		grid.add_child(btn)

# --- 交互逻辑 ---

func _on_folder_clicked(next_dir_content, dir_name):
	path_stack.append(current_dir_data)
	name_stack.append(dir_name)
	current_dir_data = next_dir_content
	_refresh_view()

func _on_back_pressed():
	if path_stack.size() > 0:
		current_dir_data = path_stack.pop_back()
		name_stack.pop_back()
		_refresh_view()

# --- 核心：密码解锁逻辑 ---
func _on_locked_folder_clicked(item_data):
	# 动态创建一个简单的密码输入弹窗
	var dialog = AcceptDialog.new()
	dialog.title = "安全警告"
	dialog.dialog_text = "请输入文件夹访问密码："
	
	# 创建输入框
	var input = LineEdit.new()
	input.placeholder_text = "默认密码是 admin"
	input.secret = true # 隐藏字符
	dialog.add_child(input)
	dialog.register_text_enter(input) # 允许回车确认
	
	# 添加到窗口树中
	add_child(dialog)
	dialog.popup_centered()
	
	# 连接确认信号
	dialog.confirmed.connect(func():
		if input.text == item_data["password"]:
			# 密码正确
			dialog.queue_free()
			# 将其类型临时改为普通文件夹并打开
			_on_folder_clicked(item_data["content"], item_data["name"])
		else:
			# 密码错误，震动窗口
			var tween = create_tween()
			var original_pos = dialog.position # 这是 Vector2i
			
			for i in range(5):
				# 【修复点】将随机偏移量强转为 Vector2i
				var offset = Vector2i(int(randf_range(-5, 5)), 0)
				tween.tween_property(dialog, "position", original_pos + offset, 0.05)
			
			tween.tween_property(dialog, "position", original_pos, 0.05)
			
			input.text = ""
			input.placeholder_text = "密码错误！"
	)

func _on_file_clicked(filename, content):
	if FileViewerScene:
		var viewer = FileViewerScene.instantiate()
		# 将查看器添加到桌面 (WindowBase 的父节点是 DesktopScreen)
		get_parent().add_child(viewer) 
		viewer.setup_text(filename, content)
		viewer.move_to_front()
		viewer.position = position + Vector2(20, 20) # 错开一点显示

func _on_image_clicked(filename, path):
	if FileViewerScene:
		var viewer = FileViewerScene.instantiate()
		get_parent().add_child(viewer)
		viewer.setup_image(filename, path)
		viewer.move_to_front()
		viewer.position = position + Vector2(20, 20)

# --- 工具：生成按钮 ---
func _create_icon_btn(text, icon):
	var btn = Button.new()
	btn.text = text
	btn.icon = icon
	
	# 【关键修改 1】允许图标跟随按钮大小缩放
	btn.expand_icon = true 
	
	# 【关键修改 2】设置图标尺寸限制 (让图标别撑满整个按钮，稍微留点空隙)
	# 如果不加这个，图标可能会变形，但这取决于你的素材。
	# 通常 icon_alignment 配合 expand_icon 就够了，或者你可以设置 icon_max_width
	
	btn.icon_alignment = HORIZONTAL_ALIGNMENT_CENTER
	btn.vertical_icon_alignment = VERTICAL_ALIGNMENT_TOP
	
	# 【关键修改 3】强制变大尺寸 (原 80,100 -> 改 160,200)
	btn.custom_minimum_size = Vector2(160, 200)
	
	btn.flat = true
	btn.clip_text = true
	btn.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
	btn.tooltip_text = text 
	
	# 增加字体覆盖，防止主题没生效
	btn.add_theme_font_size_override("font_size", 24) 
	
	return btn
