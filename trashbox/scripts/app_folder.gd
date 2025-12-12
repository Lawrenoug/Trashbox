extends "res://trashbox/scripts/window_base.gd"

# --- 引用 ---
@onready var grid = $BgColor/MainLayout/ContentSlot/ScrollContainer/GridContainer
@onready var title_label = $BgColor/MainLayout/TitleBar/TitleLabel

# --- 素材 (请确保这些图片在你的 assets 文件夹里) ---
const ICON_FOLDER = preload("res://trashbox/assets/sprites/folder.png")
const ICON_TXT = preload("res://trashbox/assets/sprites/txt.png")
const ICON_IMG = preload("res://trashbox/assets/sprites/jpg.png")
const ICON_LOCK = preload("res://trashbox/assets/sprites/lock.png") 

# 文件查看器场景
const FileViewerScene = preload("res://trashbox/scenes/main/file_viewer.tscn")

# --- 核心：文件系统数据结构 (完整版) ---
var file_system = {
	"root": {
		# === C盘：工作与系统 ===
		"C_Drive": {
			"name": "C:/System", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"Project_Trashbox": {
					"name": "工作项目_Trashbox", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"00_需求文档": {
							"name": "需求变更记录", "type": "folder", "icon": ICON_FOLDER,
							"content": {
								"v1.doc": {"type": "file", "icon": ICON_TXT, "data": "老板：做一个类似以撒的肉鸽游戏，要爽！"},
								"v2.doc": {"type": "file", "icon": ICON_TXT, "data": "老板：最近流行吸血鬼幸存者，把玩法全改了。"},
								"v3.doc": {"type": "file", "icon": ICON_TXT, "data": "老板：美术太阴暗了，要加点二次元元素。"},
								"v1024_最终版.doc": {"type": "file", "icon": ICON_TXT, "data": "老板：还是改回第一版吧，今晚必须上线。"}
							}
						},
						"01_Bug_Reports": {
							"name": "Bug追踪", "type": "folder", "icon": ICON_FOLDER,
							"content": {
								"Critical_01.txt": {"type": "file", "icon": ICON_TXT, "data": "[严重] 玩家角色偶尔会转过头盯着屏幕外的我看。\n状态：无法复现"},
								"Todo.txt": {"type": "file", "icon": ICON_TXT, "data": "1. 修复穿墙\n2. 增加咖啡机交互\n3. 只要我不睡，Bug就追不上我。"}
							}
						},
						"辞职信_草稿.txt": {"type": "file", "icon": ICON_TXT, "data": "尊敬的领导：\n\n身体实在扛不住了，我..."}
					}
				},
				"System32": {
					"name": "System_Logs", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"boot_log.txt": {"type": "file", "icon": ICON_TXT, "data": "System Start... OK\nLoading Consciousness... 84%\nWarning: Memory Leak in 'Hope' module."},
						"kernel_panic.log": {"type": "file", "icon": ICON_TXT, "data": "Fatal Exception: Player refuses to work."}
					}
				}
			}
		},

		# === D盘：个人数据 ===
		"D_Drive": {
			"name": "D:/Personal", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"Downloads": {
					"name": "下载", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"简历_2025优化版.pdf": {"type": "file", "icon": ICON_TXT, "data": "求职意向：任何只要不写代码的工作（保安也行）"},
						"Steam_Setup.exe": {"type": "file", "icon": ICON_TXT, "data": "（这是一个安装包，但由于公司网络限制无法运行。）"}
					}
				},
				"Photos": {
					"name": "相册", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"Family.jpg": {"type": "image", "icon": ICON_IMG, "data": "res://trashbox/assets/sprites/huajie.png"},
						"Last_Travel.jpg": {"type": "file", "icon": ICON_IMG, "data": "（图片无法显示）\n文件已损坏。\n你好像很久没有出去旅游了。"}
					}
				},
				"Diary": {
					"name": "加密日记", "type": "locked_folder", "icon": ICON_LOCK,
					"password": "123", 
					"content": {
						"2025-12-01.txt": {"type": "file", "icon": ICON_TXT, "data": "华姐被裁了，整个组就剩我一个。"},
						"2025-12-10.txt": {"type": "file", "icon": ICON_TXT, "data": "我听见机箱里有声音。不是风扇声，像是有人在里面呼吸。"}
					}
				}
			}
		},
		
		# === E盘：被封禁的娱乐盘 ===
		"E_Drive": {
			"name": "E:/Games", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"Steam_Library": {
					"name": "Steam", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"error.log": {"type": "file", "icon": ICON_TXT, "data": "[系统拦截] 检测到非工作软件。\n[警告] 您的年终奖已因此操作扣除 10%。"}
					}
				},
				"Music": {
					"name": "我的歌单", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"大悲咒.mp3": {"type": "file", "icon": ICON_TXT, "data": "（播放失败：声卡驱动已卸载，以便您专心工作。）"},
						"好运来.mp3": {"type": "file", "icon": ICON_TXT, "data": "（播放失败：声卡驱动已卸载。）"}
					}
				}
			}
		},

		# === G盘：公司内网共享 ===
		"G_Drive": {
			"name": "G:/Company_Share", "type": "folder", "icon": ICON_FOLDER,
			"content": {
				"00_行政通知": {
					"name": "行政通知", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"考勤制度.pdf": {"type": "file", "icon": ICON_TXT, "data": "迟到一次扣 500。\n加班没有加班费，但在公司过夜提供免费热水。"},
						"厕所使用规范.doc": {"type": "file", "icon": ICON_TXT, "data": "单次如厕时间不得超过 5 分钟，否则计入旷工。"}
					}
				},
				"01_食堂菜单": {
					"name": "本周菜单", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"周一.txt": {"type": "file", "icon": ICON_TXT, "data": "合成淀粉肠 + 剩饭"},
						"周二.txt": {"type": "file", "icon": ICON_TXT, "data": "预制料理包(过期) + 剩饭"}
					}
				},
				"Public_Upload": {
					"name": "公共上传区(匿名)", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"run.txt": {"type": "file", "icon": ICON_TXT, "data": "快跑！！！这家公司会吃人！"},
						"help_me.jpg": {"type": "image", "icon": ICON_IMG, "data": "res://trashbox/assets/sprites/boss.png"}
					}
				}
			}
		},
		
		# === Z盘：深层网络 (需要管理员密码) ===
		"Z_Drive": {
			"name": "Z:/Server_Root", "type": "locked_folder", "icon": ICON_LOCK,
			"password": "admin", 
			"content": {
				"Project_Humanity": {
					"name": "人类补完计划(废案)", "type": "folder", "icon": ICON_FOLDER,
					"content": {
						"Subject_01.txt": {"type": "file", "icon": ICON_TXT, "data": "实验体：程序员\n状态：精神濒临崩溃\n用途：作为算力电池供养 AI。"},
						"Escape_Plan.doc": {"type": "file", "icon": ICON_TXT, "data": "唯一的逃离方式是让系统崩溃（BSOD）。去寻找红色的“Bug”。"}
					}
				},
				"Boss_Config.ini": {
					"type": "file", "icon": ICON_TXT, 
					"data": "[Boss]\nName=资本家\nHP=Infinite\nWeakness=None\n\n// 备注：只要他还在加班，我就能永生。"
				}
			}
		},
		
		# === A盘：神秘软盘 (需要特殊密码) ===
		"A_Drive": {
			"name": "A:/Legacy_Floppy", "type": "locked_folder", "icon": ICON_LOCK,
			"password": "1999", 
			"content": {
				"Dream.txt": {"type": "file", "icon": ICON_TXT, "data": "小时候的梦想：\n我想做一款带给人们快乐的游戏。\n\n现在的状态：\n正在制造电子垃圾。"},
				"Secret.txt": {"type": "file", "icon": ICON_TXT, "data": "如果你看到了这里，说明你还没有完全忘记过去。"}
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
		var icon = item.get("icon", ICON_FOLDER) 
		var btn = _create_icon_btn(item.get("name", key), icon)
		
		# 根据类型连接信号
		if item["type"] == "folder":
			btn.pressed.connect(_on_folder_clicked.bind(item["content"], item.get("name", key)))
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
	var dialog = AcceptDialog.new()
	dialog.title = "安全警告"
	dialog.dialog_text = "请输入文件夹访问密码："
	
	var input = LineEdit.new()
	input.placeholder_text = "请输入密码"
	input.secret = true 
	dialog.add_child(input)
	dialog.register_text_enter(input) 
	
	add_child(dialog)
	dialog.popup_centered()
	
	dialog.confirmed.connect(func():
		if input.text == item_data["password"]:
			dialog.queue_free()
			# 密码正确，进入文件夹
			_on_folder_clicked(item_data["content"], item_data["name"])
		else:
			# 密码错误特效
			var tween = create_tween()
			var original_pos = dialog.position 
			for i in range(5):
				var offset = Vector2i(int(randf_range(-5, 5)), 0)
				tween.tween_property(dialog, "position", original_pos + offset, 0.05)
			tween.tween_property(dialog, "position", original_pos, 0.05)
			input.text = ""
			input.placeholder_text = "密码错误！"
	)

func _on_file_clicked(filename, content):
	if FileViewerScene:
		var viewer = FileViewerScene.instantiate()
		get_parent().add_child(viewer) 
		viewer.setup_text(filename, content)
		viewer.move_to_front()
		viewer.position = position + Vector2(20, 20)

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
	btn.expand_icon = true 
	btn.icon_alignment = HORIZONTAL_ALIGNMENT_CENTER
	btn.vertical_icon_alignment = VERTICAL_ALIGNMENT_TOP
	btn.custom_minimum_size = Vector2(100, 130)
	btn.flat = true
	btn.clip_text = true
	btn.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
	btn.tooltip_text = text 
	btn.add_theme_font_size_override("font_size", 16) 
	return btn
