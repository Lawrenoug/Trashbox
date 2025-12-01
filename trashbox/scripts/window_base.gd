extends Control

var dragging = false
var drag_start_position = Vector2()

func _ready():
	# 连接关闭信号
	$BgColor/MainLayout/TitleBar/CloseButton.pressed.connect(_on_close_pressed)
	# 连接拖拽信号
	var title_bar = $BgColor/MainLayout/TitleBar
	title_bar.gui_input.connect(_on_title_bar_input)

func _on_close_pressed():
	queue_free() # 自我销毁

func _on_title_bar_input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			dragging = event.pressed
			drag_start_position = get_local_mouse_position()
	
	if event is InputEventMouseMotion and dragging:
		global_position = get_global_mouse_position() - drag_start_position
