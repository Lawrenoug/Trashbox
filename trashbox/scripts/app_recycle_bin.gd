extends "res://trashbox/scripts/window_base.gd"

@onready var file_grid = $BgColor/MainLayout/ContentSlot/BodyLayout/ScrollContainer/GridContainer
@onready var btn_empty = $BgColor/MainLayout/ContentSlot/BodyLayout/Button

const ICON_TXT = preload("res://trashbox/assets/sprites/txt.png")
const ICON_IMG = preload("res://trashbox/assets/sprites/jpg.png")
const FileViewerScene = preload("res://trashbox/scenes/main/file_viewer.tscn")

func _ready():
	super._ready()
	btn_empty.pressed.connect(_on_empty_clicked)
	_generate_trash_items()

func _generate_trash_items():
	# 1. 辞职信 (核心交互)
	_create_file_btn("辞职信.txt", ICON_TXT, "resign")
	
	# 2. 恐怖日志
	_create_file_btn("help_me.log", ICON_TXT, "scary")
	
	# 3. 家人照片
	_create_file_btn("family.png", ICON_IMG, "family")

func _create_file_btn(file_name, icon, type):
	var btn = Button.new()
	btn.text = file_name
	btn.icon = icon
	btn.icon_alignment = HORIZONTAL_ALIGNMENT_CENTER
	btn.vertical_icon_alignment = VERTICAL_ALIGNMENT_TOP
	btn.custom_minimum_size = Vector2(80, 100)
	btn.flat = true
	
	btn.pressed.connect(_on_file_clicked.bind(type, btn))
	file_grid.add_child(btn)

func _on_file_clicked(type, btn_ref):
	if type == "resign":
		# === 辞职信的特殊逻辑 ===
		# 模拟“还原”：按钮消失 -> 提示 -> 按钮又出现
		btn_ref.visible = false
		
		# 告诉桌面弹个窗 (需要你的 desktop_screen.gd 支持 show_notification)
		var desktop = get_parent() 
		if desktop.has_method("show_notification"):
			desktop.show_notification("系统", "文件已还原到桌面...")
		
		# 等待2秒
		await get_tree().create_timer(2.0).timeout
		
		if desktop.has_method("show_notification"):
			desktop.show_notification("系统警告", "自动撤回：辞职申请未通过审批逻辑。")
		
		# 按钮又回来了
		btn_ref.visible = true
		
	elif type == "scary":
		_open_viewer("help_me.log", "如果你看到了这个，快跑！\n不要在晚上12点后编译代码……\n它在看着你……")
		
	elif type == "family":
		# 这里假设你有一张 family.png，没有的话用文本代替
		_open_viewer("全家福", "（照片背面写着）：\n没什么时间陪你们，对不起。\n等做完这个项目，我们就去旅游。")

func _open_viewer(title, content):
	var viewer = FileViewerScene.instantiate()
	get_parent().add_child(viewer)
	viewer.setup_text(title, content) # 这里简化统用文本显示

func _on_empty_clicked():
	# 清空回收站
	for child in file_grid.get_children():
		child.queue_free()
	
	# 播放音效或弹窗
	var desktop = get_parent()
	if desktop.has_method("show_notification"):
		desktop.show_notification("系统", "内存已释放 0MB。\n(有些东西是删不掉的)")
