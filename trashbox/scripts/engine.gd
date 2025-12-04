extends "res://trashbox/scripts/window_base.gd"

# --- 1. 节点引用 ---
# 【重要】如果报错 Node not found，请去场景树右键复制正确路径替换下面
# 中间：预览视口
@onready var preview_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
# 右侧：描述文本
@onready var description_text: RichTextLabel = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
# 左下：技能库容器 (GridContainer)
@onready var library_grid: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillDeckPanel/DeckContent/DeckScroll/DeckGrid
# 左上：携带技能容器 (VBoxContainer)
@onready var equipped_list: VBoxContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillLogPanel/LogContent/LogScroll/LogList

# --- 2. 接口：技能数据 ---
# 请在编辑器的检查器中，把你做好的 .tres 文件拖到这就两个数组里
@export var library_skills: Array[Resource]  # 技能库里的技能
@export var equipped_skills: Array[Resource] # 当前携带的技能

# --- 3. 资源预加载 ---
# 专门用于演示技能的小场景 (练功房)
const PreviewStageScene = preload("res://trashbox/scenes/main/skill_preview_stage.tscn")

# 当前加载的预览舞台实例
var current_preview_instance = null

func _ready():
	super._ready() # 必须调用父类，保证窗口能拖动
	
	# 1. 初始化中间的预览舞台
	_init_preview_stage()
	
	# 2. 刷新左侧两栏的 UI
	_refresh_library_ui()
	_refresh_equipped_ui()
	
	# 3. 设置右侧默认文本
	description_text.text = "[center]系统就绪。\n请在左侧选择一个 [color=yellow]模块 (技能)[/color] 查看详细文档。[/center]"

# --- 初始化预览舞台 (中间视口) ---
func _init_preview_stage():
	# 清理视口里残留的东西
	for child in preview_viewport.get_children():
		child.queue_free()
	
	# 实例化练功房
	if PreviewStageScene:
		current_preview_instance = PreviewStageScene.instantiate()
		preview_viewport.add_child(current_preview_instance)
	else:
		print("错误：未找到 PreviewStageScene 场景文件！")

# --- 刷新左下角：技能库 ---
func _refresh_library_ui():
	# 清空现有按钮
	for child in library_grid.get_children():
		child.queue_free()
		
	for skill in library_skills:
		if skill == null: continue # 跳过空槽位
		
		var btn = Button.new()
		# 读取资源里的技能名
		btn.text = skill.skill_name
		# 设置按钮最小高度，方便点击
		btn.custom_minimum_size = Vector2(0, 40)
		
		# 连接点击信号，把当前这个技能的数据(Resource)传过去
		btn.pressed.connect(_on_skill_selected.bind(skill))
		
		library_grid.add_child(btn)
		

# --- 刷新左上角：携带技能 (日志样式) ---
func _refresh_equipped_ui():
	# 清空现有列表
	for child in equipped_list.get_children():
		child.queue_free()
		
	for skill in equipped_skills:
		if skill == null: continue
		
		# 这里用 Button 模拟日志条目，也可以用 Label
		var slot = Button.new()
		# 加上前缀模拟运行日志
		slot.text = "> [Running] " + skill.skill_name
		slot.alignment = HORIZONTAL_ALIGNMENT_LEFT
		slot.flat = true
		slot.add_theme_color_override("font_color", Color.GREEN) # 设为绿色字
		
		# 点击也可以查看详情
		slot.pressed.connect(_on_skill_selected.bind(skill))
		
		equipped_list.add_child(slot)
		
		slot.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS # 超出显示省略号...
		slot.clip_text = true # 开启裁剪

# --- 核心交互：选中技能 ---
func _on_skill_selected(skill_data):
	# 1. 更新右侧描述 (读取 .tres 里的数据)
	# 使用 BBCode 进行排版
	var title = "[font_size=32][b]%s[/b][/font_size]\n\n" % skill_data.skill_name
	var cost_info = "[color=orange]消耗: %d[/color]   [color=red]伤害: %d[/color]\n\n" % [skill_data.cost, skill_data.damage]
	var desc = skill_data.description
	
	description_text.text = title + cost_info + desc
	
	# 2. 更新中间的演示动画
	# 假设你的 skill_preview_stage.tscn 里有个函数叫 play_demo(anim_name)
	print("正在请求演示动画: ", skill_data.animation_name)
	
	if current_preview_instance and current_preview_instance.has_method("play_demo"):
		current_preview_instance.play_demo(skill_data.animation_name)
