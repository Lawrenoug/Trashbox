extends Control
signal level_selected(level_index, level_type)

# --- 1. 素材配置 (请替换为你实际的头像/图标路径) ---
const ICON_NORMAL = preload("res://trashbox/assets/sprites/boss.png")
const ICON_ELITE = preload("res://trashbox/assets/sprites/boss.png")
const ICON_BOSS = preload("res://trashbox/assets/sprites/boss.png")
const ICON_EVENT = preload("res://trashbox/assets/sprites/folder.png") # 【新增】事件图标(暂用文件夹代替)
# --- 2. 布局配置 ---
const NODE_SIZE = Vector2(128,128) # 【关键】强制按钮大小，不要太大
const X_SPACING = 250             # 横向间距
const LAYER_COUNT = 10            # 关卡总数

@onready var map_canvas = $MapScroller/MapCanvas
@onready var player_icon = $MapScroller/MapCanvas/PlayerIcon
@onready var scroller = $MapScroller

var map_nodes = []
# 当前进度 (这个变量现在由全局变量控制)
var current_level_index = -1

func _ready():
	# 确保玩家图标别太大
	player_icon.scale = Vector2(0.6, 0.6) 
	player_icon.z_index = 2 
	
	# --- 【新增】从全局单例读取进度 ---
	# 如果是第一次运行，GlobalGameState.current_level_progress 默认为 -1
	# 我们把它同步过来
	current_level_index = GlobalGameState.current_level_progress
	
	_generate_single_line_map()
	_update_node_states()
	
	# --- 【新增】如果是刚打完回来，自动把玩家图标移动到最新关卡位置 ---
	if current_level_index >= 0 and current_level_index < LAYER_COUNT:
		# 等待一帧让节点生成完毕
		await get_tree().process_frame 
		if map_nodes.size() > current_level_index:
			var btn = map_nodes[current_level_index]
			# 简单的让玩家图标瞬移过去，表示"我在这里"
			# 如果你想更精细，可以让它停在 current_level_index + 1 (下一关) 的位置
			# 下面这个逻辑是停在"刚打完的这一关"
			player_icon.position = btn.position + Vector2(NODE_SIZE.x/2, -30)

# 生成单行地图
func _generate_single_line_map():
	# 1. 强行获取父容器的高度作为参考
	var container_height = 250.0 
	if get_parent() is Control:
		container_height = get_parent().size.y
	# 2. 重新计算 map_canvas 的高度，确保它够高
	map_canvas.custom_minimum_size.y = container_height
	# 3. 计算居中 (使用 container_height)
	var center_y = (container_height / 2) - (NODE_SIZE.y / 2)
	
	# 3. 循环生成节点
	for i in range(LAYER_COUNT):
		var btn = TextureButton.new()
		
		# 决定类型: 0=普通, 1=精英, 2=Boss, 3=事件
		var type = 0 
		if i == LAYER_COUNT - 1: type = 2
		elif i > 0 and i % 4 == 0: type = 1
		elif i > 0 and i % 3 == 0: type = 3 # 【新增】每3层可能出现一个事件
		
		# 设置图片
		match type:
			0: 
				btn.texture_normal = ICON_NORMAL
				btn.tooltip_text = "运行时错误 (战斗)"
			1: 
				btn.texture_normal = ICON_ELITE
				btn.tooltip_text = "内存溢出 (精英)"
			2: 
				btn.texture_normal = ICON_BOSS
				btn.tooltip_text = "版本发布 (Boss)"
			3: 
				btn.texture_normal = ICON_EVENT # 【新增】
				btn.tooltip_text = "随机需求 (事件)"
		
		# --- 【核心修正：强制缩放图片】---
		btn.ignore_texture_size = true  # 忽略原图尺寸
		btn.stretch_mode = TextureButton.STRETCH_KEEP_ASPECT_CENTERED # 保持比例居中缩放
		btn.custom_minimum_size = NODE_SIZE # 强制大小
		btn.size = NODE_SIZE
		
		# 设置位置 (横向排开)
		# 50是左边距
		btn.position = Vector2(50 + i * X_SPACING, center_y)
		
		# 连接点击信号
		btn.pressed.connect(_on_node_clicked.bind(i, type))
		
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
func _on_node_clicked(index, type):
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
	
	print("地图通知：请求进入第 %d 关, 类型 %d" % [index, type])
	await tween.finished
	level_selected.emit(index, type) # 【修改】发出信号带类型

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
