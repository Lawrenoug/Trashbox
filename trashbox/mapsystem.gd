extends Control

# 关卡数据：0=普通, 1=精英, 2=Boss
# 以后这个可以从全局管理器读取
var map_data = [0, 0, 1, 0, 2] 

@onready var nodes_container = $Nodes
@onready var line = $ConnectionLine

func _ready():
	_generate_map()

func _generate_map():
	# 1. 生成图标
	for i in range(map_data.size()):
		var type = map_data[i]
		var btn = Button.new()
		
		# 根据类型设置样式
		match type:
			0: btn.text = "Bug"
			1: btn.text = "需求"
			2: btn.text = "版本"
		
		btn.custom_minimum_size = Vector2(60, 60)
		
		# 点击事件：进入战斗 (这里只是打印，你需要连接到你的转场逻辑)
		btn.pressed.connect(func(): print("点击了关卡: ", i))
		
		nodes_container.add_child(btn)
	
	# 2. 等一帧，让布局计算好位置，然后再画线
	await get_tree().process_frame
	_draw_line()

func _draw_line():
	line.clear_points()
	var children = nodes_container.get_children()
	if children.is_empty(): return
	
	# 获取第一个和最后一个按钮的中心点
	# 注意：需要转换坐标系
	var start_node = children[0]
	var end_node = children[-1]
	
	# 计算 HBoxContainer 内部所有按钮的中心连线太麻烦
	# 简单做法：直接画一条贯穿长线
	
	var start_pos = start_node.global_position + start_node.size / 2 - global_position
	var end_pos = end_node.global_position + end_node.size / 2 - global_position
	
	line.add_point(start_pos)
	line.add_point(end_pos)
