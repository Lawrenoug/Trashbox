extends "F:/godotnet/trashbox/trashbox/scripts/window_base.gd" # 继承基类

@onready var file_grid = $BgColor/MainLayout/ContentSlot/ScrollContainer/GridContainer
@onready var clear_btn = $BgColor/MainLayout/ContentSlot/VBoxContainer/Button

func _ready():
	super._ready() # 必须调用！
	clear_btn.pressed.connect(_clear_files)

func _clear_files():
	for child in file_grid.get_children():
		child.queue_free()
