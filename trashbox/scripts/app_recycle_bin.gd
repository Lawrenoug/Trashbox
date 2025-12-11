extends "res://trashbox/scripts/window_base.gd"

# --- 引用 ---
# 注意：根据你的截图，路径包含 MainLayout
@onready var file_grid = $BgColor/MainLayout/ContentSlot/BodyLayout/ScrollContainer/GridContainer
@onready var btn_empty = $BgColor/MainLayout/ContentSlot/BodyLayout/Button

# --- 素材 ---
const ICON_TXT = preload("res://trashbox/assets/sprites/txt.png")
const ICON_IMG = preload("res://trashbox/assets/sprites/jpg.png")
const ICON_FILE = preload("res://trashbox/assets/sprites/txt.png") # 通用文件

const FileViewerScene = preload("res://trashbox/scenes/main/file_viewer.tscn")

func _ready():
	super._ready()
	
	if btn_empty:
		btn_empty.pressed.connect(_on_empty_clicked)
	
	# 【核心修改】检查桌面记录的状态
	var desktop = get_parent() # 获取桌面节点
	# 如果桌面说“没清空过”，或者是第一次打开(desktop可能为空的保护逻辑)
	if desktop and "is_recycle_bin_cleared" in desktop:
		if desktop.is_recycle_bin_cleared == false:
			_generate_trash_items()
		else:
			print("回收站已是清空状态，不再生成文件")
	else:
		# 预览场景时的后备逻辑
		_generate_trash_items()

func _generate_trash_items():
	# 1. 剧情关键物品：辞职信
	_create_file_btn("辞职信_最终版.txt", ICON_TXT, "resign")
	
	# 2. 氛围物品：恐怖日志
	_create_file_btn("error_log_2077.log", ICON_TXT, "scary")
	
	# 3. 情感物品：家人照片
	_create_file_btn("女儿的画.png", ICON_IMG, "family")
	
	# 4. 填充一些普通垃圾，让回收站看起来很满
	for i in range(5):
		_create_file_btn("临时文件_%d.tmp" % i, ICON_FILE, "useless")

func _create_file_btn(file_name, icon, type):
	var btn = Button.new()
	btn.text = file_name
	btn.icon = icon
	
	# 【关键修改】
	btn.expand_icon = true 
	btn.custom_minimum_size = Vector2(160, 200) # 放大两倍
	
	btn.icon_alignment = HORIZONTAL_ALIGNMENT_CENTER
	btn.vertical_icon_alignment = VERTICAL_ALIGNMENT_TOP
	btn.flat = true
	btn.clip_text = true
	btn.tooltip_text = file_name
	
	# 绑定点击事件
	btn.pressed.connect(_on_file_clicked.bind(type, btn))
	
	file_grid.add_child(btn)

func _on_file_clicked(type, btn_ref):
	var desktop = get_parent() # 获取桌面引用，用于弹窗通知
	
	if type == "resign":
		# === 辞职信特殊逻辑：无法删除/无法还原 ===
		# 1. 模拟试图操作
		btn_ref.modulate.a = 0.5 # 变半透明
		btn_ref.disabled = true
		
		# 2. 桌面弹窗警告
		if desktop.has_method("show_notification"):
			desktop.show_notification("系统警告", "正在尝试还原文件...")
		
		# 3. 等待几秒
		await get_tree().create_timer(1.5).timeout
		
		# 4. 失败反馈
		if desktop.has_method("show_notification"):
			desktop.show_notification("权限拒绝", "错误：员工合同未到期，禁止辞职。")
		
		# 5. 恢复按钮
		btn_ref.modulate.a = 1.0
		btn_ref.disabled = false
		
		# 6. 打开文件查看内容
		_open_viewer("辞职信.txt", "亲爱的老板：\n\n世界那么大，我想去看看。\n\n(系统批注：不，你不想。)")
		
	elif type == "scary":
		_open_viewer("error_log.log", "Fatal Error: Soul not found.\nTrying to reconnect...\nTrying to reconnect...\n[b]不要回头看。[/b]")
		
	elif type == "family":
		# 如果你有对应的图片，可以用 setup_image
		_open_viewer("全家福", "（照片上是一家人在海边的合影，但你的脸被像素模糊处理了。）\n\n备注：那是哪一年来着？")
		
	elif type == "useless":
		# 普通文件直接打开
		_open_viewer(btn_ref.text, "一堆乱码：\n0x0023 0x5F1A ...")

func _open_viewer(title, content):
	if FileViewerScene:
		var viewer = FileViewerScene.instantiate()
		get_parent().add_child(viewer)
		viewer.setup_text(title, content)
		viewer.move_to_front()
		viewer.position = position + Vector2(30, 30)

func _on_empty_clicked():
	var children = file_grid.get_children()
	if children.is_empty(): return

	for child in children:
		child.queue_free()
	
	# 【核心修改】告诉桌面：回收站空了！
	var desktop = get_parent()
	if desktop and "is_recycle_bin_cleared" in desktop:
		desktop.is_recycle_bin_cleared = true
	
	if desktop.has_method("show_notification"):
		desktop.show_notification("系统", "回收站已永久清空。")
