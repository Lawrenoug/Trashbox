extends "res://trashbox/scripts/window_base.gd"

@onready var image_node = $BgColor/MainLayout/ContentSlot/ImageViewer
@onready var text_node = $BgColor/MainLayout/ContentSlot/TextViewer
@onready var title_label = $BgColor/MainLayout/TitleBar/TitleLabel

func setup_text(title: String, content: String):
	title_label.text = title
	text_node.text = content
	text_node.visible = true
	image_node.visible = false

func setup_image(title: String, texture_path: String):
	title_label.text = title
	var tex = load(texture_path)
	if tex:
		image_node.texture = tex
	text_node.visible = false
	image_node.visible = true
