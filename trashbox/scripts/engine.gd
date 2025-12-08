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
	super._ready() # 调用父类，保证窗口拖拽
	
	# 【关键修改 1】将自己加入 "EngineUI" 组
	# 这样队友的 C# 脚本就能找到这个节点并调用函数
	add_to_group("EngineUI")
	
	# 初始化：加载中间的练功房
	_load_preview_stage()
	
	# 设置默认文本
	description_text.text = "[center]系统就绪。\n请点击左侧背包中的 [color=yellow]技能图标[/color] 查看详情。[/center]"
	
	# 连接地图信号
	if map_system and map_system.has_signal("level_selected"):
		map_system.level_selected.connect(_on_level_selected)

func _process(delta):
	# 闲置文本逻辑
	status_timer += delta
	if status_timer > 3.0:
		status_timer = 0
		if status_label and not status_label.text.begins_with(">"): 
			_show_random_idle_msg()

# --- 加载练功房 ---
func _load_preview_stage():
	for child in preview_viewport.get_children():
		child.queue_free()
	
	if PreviewStageScene:
		current_preview_instance = PreviewStageScene.instantiate()
		current_preview_instance.name = "SkillPreviewStage"
		preview_viewport.add_child(current_preview_instance)
	else:
		print("错误：未找到练功房场景 skill_preview_stage.tscn")

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
func _on_level_selected(level_index):
	var safe_index = 0
	if level_scenes.size() > 0:
		safe_index = level_index % level_scenes.size()
	
	load_level_full_screen(safe_index)

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
