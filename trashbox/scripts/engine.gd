extends "res://trashbox/scripts/window_base.gd"

# --- 1. 节点引用 ---
@onready var preview_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
@onready var description_text: RichTextLabel = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
@onready var library_grid: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillDeckPanel/技能组背包/GridContainer
@onready var equipped_list: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillLogPanel/技能战斗列表/GridContainer
@onready var map_system = $BgColor/MainLayout/ContentSlot/EditorRoot/TimelinePanel/MapSystem
@onready var status_label: Label = $BgColor/MainLayout/ContentSlot/EditorRoot/MenuBar/Status

# --- 2. 接口 ---
@export var library_skills: Array[PackedScene] 
@export var equipped_skills: Array[PackedScene]
@export var level_scenes: Array[PackedScene]

# 练功房场景
const PreviewStageScene = preload("res://trashbox/scenes/main/skill_preview_stage.tscn")

# 当前练功房实例
var current_preview_instance = null

# --- 3. 闲置状态文本 (中文程序员梗) ---
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

# 计时器，用于切换闲置文本
var status_timer = 0.0

func _ready():
	super._ready()
	
	# 初始化：加载练功房
	_load_preview_stage()
	
	# 刷新 UI
	_refresh_library_ui()
	_refresh_equipped_ui()
	
	description_text.text = "[center]系统就绪。\n读取 .tscn 技能模块中...[/center]"
	
	# 连接地图信号
	if map_system:
		map_system.level_selected.connect(_on_level_selected)

func _process(delta):
	# 每 3 秒随机切换一次闲置状态
	status_timer += delta
	if status_timer > 3.0:
		status_timer = 0
		# 只有当当前显示的不是重要信息(以>开头)时，才切换闲置文本
		if not status_label.text.begins_with(">"): 
			_show_random_idle_msg()

# --- 核心改动 A: 加载并锁定练功房 ---
func _load_preview_stage():
	for child in preview_viewport.get_children():
		child.queue_free()
	
	if PreviewStageScene:
		current_preview_instance = PreviewStageScene.instantiate()
		current_preview_instance.name = "SkillPreviewStage"
		preview_viewport.add_child(current_preview_instance)
	else:
		print("错误：未找到练功房场景 skill_preview_stage.tscn")

# --- 核心改动 B: 点击技能 ---
func _on_skill_selected(skill_scene: PackedScene):
	# 1. 实例化获取数据
	var skill_instance = skill_scene.instantiate()
	
	if not "skill_name" in skill_instance:
		skill_instance.queue_free()
		return
	
	var s_name = skill_instance.skill_name
	var s_cost = skill_instance.cost
	var s_dmg = skill_instance.damage
	var s_desc = skill_instance.description
	var s_anim = skill_instance.animation_name
	
	# 2. 更新右侧文本
	var title = "[font_size=32][b]%s[/b][/font_size]\n\n" % s_name
	var info = "[color=orange]消耗: %d[/color]   [color=red]伤害: %d[/color]\n\n" % [s_cost, s_dmg]
	description_text.text = title + info + s_desc
	
	# 更新状态栏
	set_status_log("正在检查模块 [" + s_name + "]...")
	
	# 3. 播放动画
	if current_preview_instance and current_preview_instance.has_method("play_demo"):
		current_preview_instance.play_demo(s_anim)
	
	skill_instance.queue_free()

# --- 核心改动 C: 地图选择 -> 全屏跳转 ---
func _on_level_selected(level_index):
	var safe_index = 0
	if level_scenes.size() > 0:
		safe_index = level_index % level_scenes.size()
	
	load_level_full_screen(safe_index)

# --- 跳转函数 (Load Function) ---
func load_level_full_screen(index: int):
	if level_scenes.is_empty(): 
		print("错误：没有配置 Level Scenes！")
		set_status_log("Error: Level list is empty!")
		return
		
	var scene_res = level_scenes[index]
	
	if scene_res:
		print("正在全屏跳转至关卡: ", index)
		
		# 更新状态栏提示
		set_status_log("正在部署运行环境 (Level " + str(index) + ")...")
		
		# 切换场景 (销毁当前 UI，进入战斗)
		get_tree().change_scene_to_packed(scene_res)
	else:
		print("错误：关卡场景资源为空")

# --- UI 刷新辅助函数 ---
func _refresh_library_ui():
	#for child in library_grid.get_children(): child.queue_free()
	for skill_scene in library_skills:
		if skill_scene == null: continue
		var temp_instance = skill_scene.instantiate()
		if not "skill_name" in temp_instance:
			temp_instance.queue_free()
			continue
		var btn = Button.new()
		btn.text = temp_instance.skill_name 
		btn.custom_minimum_size = Vector2(0, 40)
		btn.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
		btn.pressed.connect(_on_skill_selected.bind(skill_scene))
		library_grid.add_child(btn)
		temp_instance.queue_free()

func _refresh_equipped_ui():
	#for child in equipped_list.get_children(): child.queue_free()
	for skill_scene in equipped_skills:
		if skill_scene == null: continue
		var temp_instance = skill_scene.instantiate()
		if not "skill_name" in temp_instance:
			temp_instance.queue_free()
			continue
		var slot = Button.new()
		slot.text = "> [Running] " + temp_instance.skill_name
		slot.alignment = HORIZONTAL_ALIGNMENT_LEFT
		slot.flat = true
		slot.add_theme_color_override("font_color", Color.GREEN)
		slot.pressed.connect(_on_skill_selected.bind(skill_scene))
		equipped_list.add_child(slot)
		temp_instance.queue_free()

# --- 状态栏辅助函数 ---
func _show_random_idle_msg():
	var msg = idle_messages.pick_random()
	status_label.text = msg
	status_label.modulate = Color(0.6, 0.6, 0.6)

func set_status_log(action_name: String):
	status_timer = -1.0 # 暂时暂停闲置切换
	status_label.text = "> " + action_name
	status_label.modulate = Color(0, 1, 0) # 亮绿色高亮
