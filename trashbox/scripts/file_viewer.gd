extends "res://trashbox/scripts/window_base.gd"

# 内容节点引用
@onready var image_node = $BgColor/MainLayout/ContentSlot/ImageViewer
@onready var text_node = $BgColor/MainLayout/ContentSlot/TextViewer

func _ready():
	super._ready() # 必须调用，保证窗口能拖动和关闭
	
	# 设置文本内容的颜色为黑色（防止白底白字看不见）
	if text_node:
		text_node.add_theme_color_override("default_color", Color.BLACK)
		text_node.fit_content = false

# --- 核心修复：设置窗口标题 ---
func set_window_title(new_title: String):
	# 方法 1：尝试直接通过路径获取 (最快)
	var lbl = get_node_or_null("BgColor/MainLayout/TitleBar/TitleLabel")
	
	# 方法 2：如果路径不对，使用递归查找 (最稳)
	if lbl == null:
		lbl = find_child("TitleLabel", true, false)
	
	# 赋值
	if lbl:
		lbl.text = new_title
		print("文件查看器标题已设置为：", new_title)
	else:
		print("错误：在 FileViewer 中找不到 TitleLabel 节点，无法设置标题！")

# 打开文本文件时调用
func setup_text(title: String, content: String):
	# 1. 设置标题
	set_window_title(title)
	
	# 2. 设置内容
	if text_node:
		text_node.text = content
		text_node.visible = true
	if image_node:
		image_node.visible = false

# 打开图片文件时调用
func setup_image(title: String, texture_path: String):
	# 1. 设置标题
	set_window_title(title)
	
	# 2. 设置图片
	var tex = load(texture_path)
	if tex and image_node:
		image_node.texture = tex
		image_node.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		image_node.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		
	if text_node: text_node.visible = false
	if image_node: image_node.visible = true
