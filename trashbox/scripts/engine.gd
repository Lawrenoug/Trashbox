extends "res://trashbox/scripts/window_base.gd"

# --- 1. 节点引用 (保持不变) ---
@onready var preview_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
@onready var description_text: RichTextLabel = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
@onready var library_grid: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillDeckPanel/技能组背包/GridContainer
@onready var equipped_list: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillLogPanel/技能战斗列表/GridContainer
@onready var map_system = $BgColor/MainLayout/ContentSlot/EditorRoot/TimelinePanel/MapSystem

# --- 2. 接口：改为 PackedScene (场景) ---
# 【变化】现在这里拖入的是 .tscn 文件，而不是 .tres 文件
@export var library_skills: Array[PackedScene] 
@export var equipped_skills: Array[PackedScene]

# --- 3. 关卡接口 (保持不变) ---
@export var level_scenes: Array[PackedScene]

const PreviewStageScene = preload("res://trashbox/scenes/main/skill_preview_stage.tscn")
var current_content_instance = null

func _ready():
	super._ready()
	
	_load_preview_stage()
	
	# 刷新 UI
	_refresh_library_ui()
	_refresh_equipped_ui()
	
	description_text.text = "[center]系统就绪。\n读取 .tscn 技能模块中...[/center]"
	
	if map_system:
		map_system.level_selected.connect(_on_level_selected)

# --- 刷新左下角：技能库 ---
func _refresh_library_ui():
	#for child in library_grid.get_children(): child.queue_free()
		
	for skill_scene in library_skills:
		if skill_scene == null: continue
		
		# 【关键变化】为了读取 .tscn 里的名字，必须先实例化
		var temp_instance = skill_scene.instantiate()
		
		# 检查它是不是一个合法的技能 (有没有继承 Skill 类)
		# 这里的 'Skill' 是你队友定义的 class_name
		if not "skill_name" in temp_instance:
			print("警告：该场景缺少 skill_name 属性，可能不是技能！")
			temp_instance.queue_free()
			continue
			
		var btn = Button.new()
		# 读取实例化后的属性
		btn.text = temp_instance.skill_name 
		btn.custom_minimum_size = Vector2(0, 40)
		btn.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
		
		# 连接信号：把这个 PackedScene 传给点击函数
		btn.pressed.connect(_on_skill_selected.bind(skill_scene))
		
		library_grid.add_child(btn)
		
		# 读完数据就销毁这个临时对象，不占内存
		temp_instance.queue_free()

# --- 刷新左上角：携带技能 ---
func _refresh_equipped_ui():
	#for child in equipped_list.get_children(): child.queue_free()
		
	for skill_scene in equipped_skills:
		if skill_scene == null: continue
		
		# 同样需要实例化来读取名字
		var temp_instance = skill_scene.instantiate()
		if not "skill_name" in temp_instance:
			print("错误：发现一个无效的技能场景！它没有 skill_name 属性。场景名: ", temp_instance.name)
			# 销毁它，并跳过这次循环，不要让游戏崩溃
			temp_instance.queue_free()
			continue
		# 这里省略类型检查，假设拖进来的都是对的
		var slot = Button.new()
		slot.text = "> [Running] " + temp_instance.skill_name
		slot.alignment = HORIZONTAL_ALIGNMENT_LEFT
		slot.flat = true
		slot.add_theme_color_override("font_color", Color.GREEN)
		
		slot.pressed.connect(_on_skill_selected.bind(skill_scene))
		
		equipped_list.add_child(slot)
		temp_instance.queue_free()

# --- 核心交互：选中技能 ---
func _on_skill_selected(skill_scene: PackedScene):
	# 1. 恢复练功房视口
	if not (current_content_instance is Node2D and current_content_instance.name.begins_with("SkillPreview")):
		_load_preview_stage()

	# 2. 【关键】再次实例化来获取详细信息
	# 此时我们需要这个实例存在，甚至可能把它放到视口里演示
	skill_instance = skill_scene.instantiate()
	
	# 读取属性
	var s_name = skill_instance.skill_name
	var s_cost = skill_instance.cost
	var s_dmg = skill_instance.damage
	var s_desc = skill_instance.description
	var s_anim = skill_instance.animation_name
	
	# 更新文本
	var title = "[font_size=32][b]%s[/b][/font_size]\n\n" % s_name
	var info = "[color=orange]消耗: %d[/color]   [color=red]伤害: %d[/color]\n\n" % [s_cost, s_dmg]
	description_text.text = title + info + s_desc
	
	# 3. 演示动画
	if current_content_instance and current_content_instance.has_method("play_demo"):
		current_content_instance.play_demo(s_anim)
	
	# 4. 这个用于读取信息的实例用完了，如果不需要把它放到场景里，就销毁
	# 如果你想直接把技能特效显示出来，也可以把它 add_child 到视口里
	var skill_instance = skill_scene.instantiate()

	# 2. 只实例化技能的预览部分（不加载完整战斗场景）
	preview_viewport.add_child(skill_instance)
	current_skill_instance = skill_instance


# ... (后面的 load_level, _load_preview_stage 等代码保持不变) ...
# 请把之前完整代码里的那些辅助函数补在后面
# ----------------------------------------------------
# 以下是补全的辅助代码，直接粘贴在下面即可
# ----------------------------------------------------

func _load_preview_stage():
	_clear_viewport()
	if PreviewStageScene:
		current_content_instance = PreviewStageScene.instantiate()
		current_content_instance.name = "SkillPreviewStage" # 方便判断
		preview_viewport.add_child(current_content_instance)

func _on_level_selected(level_index):
	var safe_index = 0
	if level_scenes.size() > 0:
		safe_index = level_index % level_scenes.size()
	load_level(safe_index)

# 新增：切换到战斗场景的方法
func start_battle():
	# 请将路径替换为你的战斗场景实际路径
	get_tree().change_scene_to_file("res://trashbox/scenes/main/battle_scene.tscn")

func load_level(index: int):
	if level_scenes.is_empty(): return
	_clear_viewport()
	var scene_res = level_scenes[index]
	if scene_res:
		current_content_instance = scene_res.instantiate()
		preview_viewport.add_child(current_content_instance)
		description_text.text = "[center]战斗模块加载中...\nLevel %d[/center]" % index

func _clear_viewport():
	if current_content_instance != null:
		current_content_instance.queue_free()
		current_content_instance = null
	for child in preview_viewport.get_children():
		child.queue_free()
