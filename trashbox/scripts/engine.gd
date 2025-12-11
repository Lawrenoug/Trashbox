extends "res://trashbox/scripts/window_base.gd"

# --- 1. 节点引用 ---
# (路径保持你之前的设置)
@onready var preview_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
@onready var description_text: RichTextLabel = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
@onready var map_system = $BgColor/MainLayout/ContentSlot/EditorRoot/TimelinePanel/MapSystem
@onready var status_label: Label = $BgColor/MainLayout/ContentSlot/EditorRoot/MenuBar/Status

# --- 2. 接口 ---
# 关卡场景列表 (地图跳转用)
@export var level_scenes: Array[PackedScene]
@export var event_scenes: Array[PackedScene]

# 练功房场景
const PreviewStageScene = preload("res://trashbox/scenes/main/skill_preview_stage.tscn")

# 当前练功房实例
var current_preview_instance = null

# --- 3. 闲置状态文本 ---
var idle_messages = [
	"系统就绪 (System Ready).",
	"正在执行垃圾回收 (GC)...",
	"等待指令输入...",
	"警告：CPU 占用率 99%",
	"检测到内存泄漏 (Memory Leak Detected)...",
	"正在编译着色器 (2048/4096)...",
	"0 错误, 99 警告 (能跑就行).",
	"Git: HEAD 指针游离 (Detached HEAD state)",
	"警告：咖啡因水平极低",
	"正在解析依赖项...",
	"正在尝试退出 Vim...",
	"在我的机器上是正常的 (It works on my machine)."
]

var status_timer = 0.0

func _ready():
	super._ready() 
	add_to_group("EngineUI")
	
	_load_preview_stage()
	
	description_text.text = "[center]系统就绪。\n请点击左侧背包中的技能图标查看详情。[/center]"
	
	if map_system and map_system.has_signal("level_selected"):
		map_system.level_selected.connect(_on_level_selected)

func _process(delta):
	status_timer += delta
	if status_timer > 3.0:
		status_timer = 0
		if status_label and not status_label.text.begins_with(">"): 
			status_label.text = idle_messages.pick_random()

# --- 加载练功房 ---
func _load_preview_stage():
	for child in preview_viewport.get_children():
		child.queue_free()
	if PreviewStageScene:
		current_preview_instance = PreviewStageScene.instantiate()
		preview_viewport.add_child(current_preview_instance)

# --- 【关键修改 2】供 C# 脚本调用的新接口 ---
# 当你在背包里点击图标时，Drapskill.cs 会调用这个函数
# 参数 skill_node: 是背包格子里那个实际存在的节点 (挂载了 Skill C# 脚本)
func preview_skill_instance(skill_node: Node):
	if skill_node == null: return
	
	# 1. 读取 C# 脚本中的变量
	# 使用 get() 是安全的，如果 C# 里没写这个变量，会返回 null 而不是报错
	# 注意：这里的属性名必须和你队友 C# 代码里写的一模一样 (区分大小写)
	var s_name = skill_node.get("skillName") if skill_node.get("skillName") else "未知模块"
	var s_desc = skill_node.get("skillDescription") if skill_node.get("skillDescription") else "暂无描述"
	var s_quote = skill_node.get("skillQuote") if skill_node.get("skillQuote") else ""
	var s_atk = skill_node.get("ATK") if skill_node.get("ATK") != null else 0
	var s_ats = skill_node.get("ATS") if skill_node.get("ATS") != null else 0
	
	# 2. 更新右侧文本描述
	var title = "[font_size=32][b]%s[/b][/font_size]\n\n" % s_name
	var stats = "[color=orange]攻速: %.1f[/color]   [color=red]攻击力: %d[/color]\n\n" % [s_ats, s_atk]
	var content = s_desc + "\n\n"
	
	# 添加引用台词 (灰色斜体)
	var quote_text = ""
	if s_quote != "":
		quote_text = "[color=#888888][i]“%s”[/i][/color]" % s_quote
	
	description_text.text = title + stats + content + quote_text
	
	# 更新顶部状态栏
	set_status_log("选中模块: " + s_name)
	
	# 3. 尝试在中间演示技能
	# 因为传进来的是一个实例，我们需要获取它的源文件路径(scene_file_path)
	# 重新加载一个 PackedScene 给练功房去实例化演示
	if skill_node.scene_file_path != "":
		var skill_packed = load(skill_node.scene_file_path)
		if current_preview_instance and current_preview_instance.has_method("play_demo_with_scene"):
			current_preview_instance.play_demo_with_scene(skill_packed)
	else:
		print("警告：该技能节点没有对应的场景文件路径，无法在练功房演示。")

# --- 地图选择 -> 全屏跳转 ---
func _on_level_selected(level_index, level_type):
	print("Engine: 收到地图信号 -> 层数: ", level_index, ", 类型: ", level_type)
	
	# 根据 mapsystem.gd 的定义：
	# 0=普通战斗, 1=精英战斗, 2=Boss, 3=事件房
	
	if level_type == 3:
		# === 进入事件房逻辑 ===
		if event_scenes.size() > 0:
			# 这里可以选择随机一个事件，或者按顺序
			# 比如：随机抽一个事件房
			var random_event = event_scenes.pick_random()
			_perform_scene_change(random_event, "正在加载事件模块...")
		else:
			print("错误：你还没在 Engine 的 Inspector 里配置 Event Scenes 数组！")
			set_status_log("Error: No Event Scenes configured!")
			
	else:
		# === 进入战斗逻辑 (0, 1, 2) ===
		if level_scenes.size() > 0:
			# 简单的按索引取模，防止越界
			var safe_index = level_index % level_scenes.size()
			var combat_scene = level_scenes[safe_index]
			_perform_scene_change(combat_scene, "正在部署战斗环境 (Level %d)..." % level_index)
		else:
			print("错误：你还没在 Engine 的 Inspector 里配置 Level Scenes 数组！")
			set_status_log("Error: No Level Scenes configured!")
			
func _perform_scene_change(scene_res: PackedScene, log_msg: String):
	if scene_res:
		set_status_log(log_msg)
		# 延迟一点点跳转，让玩家看到 Log 变化
		await get_tree().create_timer(0.5).timeout
		get_tree().change_scene_to_packed(scene_res)
	else:
		print("错误：目标场景资源为空")

# --- 跳转函数 ---
func load_level_full_screen(index: int):
	if level_scenes.is_empty(): 
		set_status_log("Error: Level list is empty!")
		return
		
	var scene_res = level_scenes[index]
	
	if scene_res:
		print("正在全屏跳转至关卡: ", index)
		set_status_log("正在部署运行环境 (Level " + str(index) + ")...")
		get_tree().change_scene_to_packed(scene_res)
	else:
		print("错误：关卡场景资源为空")

# --- 状态栏辅助函数 ---
func _show_random_idle_msg():
	var msg = idle_messages.pick_random()
	status_label.text = msg
	status_label.modulate = Color(0.6, 0.6, 0.6)

func set_status_log(action_name: String):
	status_timer = -1.0 
	status_label.text = "> " + action_name
	status_label.modulate = Color(0, 1, 0)

# --- 供 C# 背包脚本调用 ---
func scan_and_update_sequence():
	set_status_log("正在同步战斗序列...")
	
	# 1. 寻找背包里的 GridContainer (存放格子的容器)
	# 根据你的场景结构，我们需要在 engine 场景里找到它
	# 你的 engine.tscn 结构有点深，使用 find_child 递归查找是最稳妥的
	var grid_container = find_child("GridContainer", true, false)
	
	if not grid_container:
		print("错误：在 Engine 中找不到 GridContainer，无法同步技能！")
		return
	
	# 2. 收集所有技能的 PackedScene
	var sequence_array: Array[PackedScene] = []
	var debug_names = ""
	
	# 遍历所有格子 (TextureRect)
	for slot in grid_container.get_children():
		# 检查格子里有没有技能 (技能是格子的子节点)
		if slot.get_child_count() > 0:
			var skill_node = slot.get_child(0)
			
			# 只要这个节点有文件路径，就说明它是我们要的技能
			if skill_node.scene_file_path != "":
				var packed = load(skill_node.scene_file_path)
				sequence_array.append(packed)
				
				# (可选) 获取名字用来打印日志
				var s_name = skill_node.get("skillName")
				if s_name: debug_names += "[" + s_name + "] "
	
	# 3. 发送给练功房
	if sequence_array.is_empty():
		set_status_log("序列为空")
		return
		
	if current_preview_instance:
		print("同步序列: ", debug_names)
		current_preview_instance.update_sequence(sequence_array)
		set_status_log("序列更新: " + str(sequence_array.size()) + " 个模块")
