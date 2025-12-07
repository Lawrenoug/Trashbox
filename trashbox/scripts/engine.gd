extends "res://trashbox/scripts/window_base.gd"

# --- 1. 节点引用 ---
@onready var preview_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
@onready var description_text: RichTextLabel = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
@onready var library_grid: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillDeckPanel/技能组背包/GridContainer
@onready var equipped_list: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillLogPanel/技能战斗列表/GridContainer
@onready var map_system = $BgColor/MainLayout/ContentSlot/EditorRoot/TimelinePanel/MapSystem

# --- 2. 技能接口 ---
@export var library_skills: Array[PackedScene] 
@export var equipped_skills: Array[PackedScene]

# --- 3. 关卡接口 ---
@export var level_scenes: Array[PackedScene]

# 练功房场景
const PreviewStageScene = preload("res://trashbox/scenes/main/skill_preview_stage.tscn")

# 当前练功房实例
var current_preview_instance = null

func _ready():
	super._ready()
	
	# 【改动 1】初始化时，直接加载练功房，不再动它了
	_load_preview_stage()
	
	_refresh_library_ui()
	_refresh_equipped_ui()
	
	description_text.text = "[center]系统就绪。\n读取 .tscn 技能模块中...[/center]"
	
	# 连接地图信号
	if map_system:
		map_system.level_selected.connect(_on_level_selected)

# --- 【核心改动 A】加载并锁定练功房 ---
# 既然中间不再放关卡，这个函数只需要在 _ready 调用一次即可
# 它负责把 skill_preview_stage 塞进中间的视口
func _load_preview_stage():
	# 清理旧内容
	for child in preview_viewport.get_children():
		child.queue_free()
	
	if PreviewStageScene:
		current_preview_instance = PreviewStageScene.instantiate()
		current_preview_instance.name = "SkillPreviewStage"
		preview_viewport.add_child(current_preview_instance)
	else:
		print("错误：未找到练功房场景 skill_preview_stage.tscn")

# --- 【核心改动 B】点击技能逻辑简化 ---
# 现在不需要判断中间是关卡还是练功房了，直接调用演示即可
func _on_skill_selected(skill_scene: PackedScene):
	# 1. 实例化获取数据
	var skill_instance = skill_scene.instantiate()
	
	# 安全检查
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
	
	# 3. 在中间的练功房播放动画
	if current_preview_instance and current_preview_instance.has_method("play_demo"):
		current_preview_instance.play_demo(s_anim)
	
	# 4. 销毁临时数据实例
	skill_instance.queue_free()

# --- 【核心改动 C】地图选择关卡 -> 全屏跳转 ---
func _on_level_selected(level_index):
	# 防止数组越界
	var safe_index = 0
	if level_scenes.size() > 0:
		safe_index = level_index % level_scenes.size()
	
	load_level_full_screen(safe_index)

# 这个函数负责真正的跳转
func load_level_full_screen(index: int):
	if level_scenes.is_empty(): 
		print("错误：没有配置 Level Scenes！")
		return
		
	var scene_res = level_scenes[index]
	
	if scene_res:
		print("正在全屏跳转至关卡: ", index)
		
		# 【关键】这里不再是 add_child，而是切换整个游戏场景
		# 这会销毁当前的 Desktop 和 Engine 界面，进入纯粹的战斗场景
		get_tree().change_scene_to_packed(scene_res)
		
		# 提示：你需要在战斗场景里写一个“返回”按钮，
		# 也就是 get_tree().change_scene_to_file("res://.../desktop_screen.tscn")
	else:
		print("错误：关卡场景资源为空")

# ----------------------------------------------------
# 以前的 _refresh_library_ui 和 _refresh_equipped_ui 保持不变
# ----------------------------------------------------
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
