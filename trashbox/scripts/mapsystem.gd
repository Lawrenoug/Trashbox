extends Control
signal level_selected(level_index)

# --- 1. 素材配置 (请替换为你实际的头像/图标路径) ---
const ICON_NORMAL = preload("res://trashbox/assets/sprites/boss.png")
const ICON_ELITE = preload("res://trashbox/assets/sprites/boss.png")
const ICON_BOSS = preload("res://trashbox/assets/sprites/boss.png")

# --- 2. 布局配置 ---
const NODE_SIZE = Vector2(64, 64) # 【关键】强制按钮大小，不要太大
const X_SPACING = 150             # 横向间距
const LAYER_COUNT = 10            # 关卡总数

@onready var map_canvas = $MapScroller/MapCanvas
@onready var player_icon = $MapScroller/MapCanvas/PlayerIcon
@onready var scroller = $MapScroller

# 存储所有按钮的数组
var map_nodes = []
# 当前进度 (-1: 未开始, 0: 第一关...)
var current_level_index = -1

func _ready():
	# 确保玩家图标别太大 (根据你的截图，它太大了)
	player_icon.scale = Vector2(0.3, 0.3) 
	player_icon.z_index = 2 # 保证在最上面
	
	_generate_single_line_map()
	_update_node_states()

# 生成单行地图
func _generate_single_line_map():
	# 1. 清理旧数据
	for child in map_canvas.get_children():
		if child != player_icon: # 保留玩家图标
			child.queue_free()
	map_nodes.clear()
	
	# 2. 计算垂直居中的 Y 坐标
	# 画布高度的一半 - 按钮高度的一半
	var center_y = (map_canvas.custom_minimum_size.y / 2) - (NODE_SIZE.y / 2)
	
	# 3. 循环生成节点
	for i in range(LAYER_COUNT):
		var btn = TextureButton.new()
		
		# 决定类型 (最后是Boss)
		var type = 0 # 默认普通
		if i == LAYER_COUNT - 1: type = 2
		elif i > 0 and i % 4 == 0: type = 1 # 每4关一个精英
		
		# 设置图片
		match type:
			0: btn.texture_normal = ICON_NORMAL
			1: btn.texture_normal = ICON_ELITE
			2: btn.texture_normal = ICON_BOSS
		
		# --- 【核心修正：强制缩放图片】---
		btn.ignore_texture_size = true  # 忽略原图尺寸
		btn.stretch_mode = TextureButton.STRETCH_KEEP_ASPECT_CENTERED # 保持比例居中缩放
		btn.custom_minimum_size = NODE_SIZE # 强制大小
		btn.size = NODE_SIZE
		
		# 设置位置 (横向排开)
		# 50是左边距
		btn.position = Vector2(50 + i * X_SPACING, center_y)
		
		# 连接点击信号
		btn.pressed.connect(_on_node_clicked.bind(i))
		
		map_canvas.add_child(btn)
		map_nodes.append(btn)
		
		# 画线 (连向下一个)
		if i < LAYER_COUNT - 1:
			_draw_line_to_next(btn.position, Vector2(50 + (i+1) * X_SPACING, center_y))

	# 初始化玩家位置 (在第一个按钮左边)
	player_icon.position = Vector2(10, center_y + NODE_SIZE.y/2)

# 画线辅助函数
func _draw_line_to_next(start_pos, end_pos):
	var line = Line2D.new()
	line.width = 4
	line.default_color = Color(1, 1, 1, 0.5) # 半透明白线
	# 也就是按钮中心点
	line.add_point(start_pos + NODE_SIZE / 2) 
	line.add_point(end_pos + NODE_SIZE / 2)
	line.z_index = -1 # 在按钮下面
	map_canvas.add_child(line)

# 更新状态 (呼吸效果)
func _update_node_states():
	for i in range(map_nodes.size()):
		var btn = map_nodes[i]
		
		if i < current_level_index:
			# 已通过：变暗
			btn.modulate = Color(0.4, 0.4, 0.4, 1)
			btn.disabled = true
			_stop_breathing(btn)
			
		elif i == current_level_index:
			# 当前所在：高亮
			btn.modulate = Color(1, 1, 1, 1)
			btn.disabled = true
			_stop_breathing(btn)
			
		elif i == current_level_index + 1:
			# 下一关：【呼吸效果】+ 可点击
			btn.modulate = Color(1, 1, 1, 1)
			btn.disabled = false
			_start_breathing(btn)
			
		else:
			# 未来的关卡：变黑不可点
			btn.modulate = Color(0.2, 0.2, 0.2, 1)
			btn.disabled = true
			_stop_breathing(btn)

# 点击事件
func _on_node_clicked(index):
	current_level_index = index
	
	# 1. 玩家移动动画
	var btn = map_nodes[index]
	var tween = create_tween()
	var target_pos = btn.position + Vector2(NODE_SIZE.x/2, -30) 
	tween.tween_property(player_icon, "position", target_pos, 0.5).set_trans(Tween.TRANS_CUBIC)
	
	# 2. 自动滚动地图
	var scroll_center = btn.position.x - scroller.size.x / 2 + NODE_SIZE.x / 2
	tween.parallel().tween_property(scroller, "scroll_horizontal", scroll_center, 0.5)
	
	# 3. 更新状态
	_update_node_states()
	
	print("地图通知：请求进入第 %d 关" % index)
	level_selected.emit(index)

# --- 动画工具 ---
func _start_breathing(node):
	if not node.has_meta("tween"):
		node.pivot_offset = node.size / 2 # 设置缩放中心点
		var t = create_tween().set_loops()
		node.set_meta("tween", t)
		t.tween_property(node, "scale", Vector2(1.2, 1.2), 0.6)
		t.tween_property(node, "scale", Vector2(1.0, 1.0), 0.6)

func _stop_breathing(node):
	if node.has_meta("tween"):
		node.get_meta("tween").kill()
		node.remove_meta("tween")
		node.scale = Vector2(1, 1)
