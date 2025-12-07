extends "res://trashbox/scripts/window_base.gd"

# --- 1. 节点引用 ---
# (保持不变，如果你的路径改过请修正)
@onready var preview_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
@onready var description_text = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
@onready var library_grid = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillDeckPanel/技能组背包/GridContainer
@onready var equipped_grid = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillLogPanel/技能战斗列表/GridContainer

# --- 2. 预加载格子模版 ---
const SkillSlotScene = preload("res://trashbox/scenes/main/skill_slot.tscn")

# --- 3. 【核心修改】接口改为 PackedScene (场景文件) ---
# 请在检查器里，把你队友做的 skill_fireball.tscn, skill_slash.tscn 等拖进来
@export var library_skills: Array[PackedScene] 
@export var equipped_skills: Array[PackedScene]

# 当前正在演示的技能实例
var current_skill_instance = null

func _ready():
	super._ready()
	
	# 刷新界面
	_refresh_library_ui()
	_refresh_equipped_ui()
	
	description_text.text = "[center]系统就绪。\n请选择模块查看文档。[/center]"

# --- 刷新左下角：技能库 ---
func _refresh_library_ui():
	# A. 先清空容器里生成的格子 (保留队友做的背景板等其他东西)
	# 注意：如果队友在 GridContainer 里放了装饰图，会被删掉。
	# 建议让 GridContainer 只用来放生成的技能。
	#for child in library_grid.get_children():
		#child.queue_free()
		
	# B. 生成新格子
	for skill in library_skills:
		if skill == null: continue
		
		# 实例化漂亮格子
		var slot = SkillSlotScene.instantiate()
		
		# 设置图标 (假设 slot 本身就是 TextureButton)
		# 如果 slot 是个复杂的组合，用 slot.get_node("Icon") 获取
		var btn = slot 
		if "texture_normal" in btn:
			btn.texture_normal = skill.icon # 确保你的 SkillData 里有 icon 变量
		
		# 绑定点击事件
		if btn.has_signal("pressed"):
			btn.pressed.connect(_on_skill_selected.bind(skill))
			
		library_grid.add_child(slot)

# --- 刷新左上角：携带技能 ---
func _refresh_equipped_ui():
	#for child in equipped_grid.get_children():
		#child.queue_free()
		
	for skill_scene in equipped_skills:
		if skill_scene == null: continue
		
		var temp_skill = skill_scene.instantiate()
		var slot = SkillSlotScene.instantiate()
		var btn = _get_button_from_slot(slot)
		
		if "icon" in temp_skill and temp_skill.icon != null:
			btn.texture_normal = temp_skill.icon
			
		if "skill_name" in temp_skill:
			btn.tooltip_text = "[已装备] " + temp_skill.skill_name
			
		btn.pressed.connect(_on_skill_selected.bind(skill_scene))
		equipped_grid.add_child(slot)
		
		temp_skill.queue_free()

# --- 核心交互：选中技能 ---
func _on_skill_selected(skill_scene: PackedScene):
	# 1. 清理中间视口
	for child in preview_viewport.get_children():
		child.queue_free()
	
	# 2. 【关键】在中间视口实例化真正的技能场景
	var skill_instance = skill_scene.instantiate()
	preview_viewport.add_child(skill_instance)
	current_skill_instance = skill_instance
	
	# 3. 读取数据更新右侧描述
	# 假设队友的代码里有这些变量：skill_name, description, damage, cost
	var s_name = skill_instance.skill_name if "skill_name" in skill_instance else "未命名"
	var s_desc = skill_instance.description if "description" in skill_instance else "无描述"
	var s_dmg = skill_instance.damage if "damage" in skill_instance else 0
	var s_cost = skill_instance.cost if "cost" in skill_instance else 0
	
	var title = "[font_size=32][b]%s[/b][/font_size]\n\n" % s_name
	var info = "[color=orange]消耗: %d[/color]   [color=red]伤害: %d[/color]\n\n" % [s_cost, s_dmg]
	
	description_text.text = title + info + s_desc
	
	# 4. 如果技能有演示功能，可以调用
	# 比如队友在技能里写了个 func demo(): ...
	if skill_instance.has_method("demo"):
		skill_instance.demo()
	elif skill_instance.has_node("AnimationPlayer"):
		# 或者尝试播放默认动画
		skill_instance.get_node("AnimationPlayer").play("attack")

# --- 辅助工具：获取格子里的按钮 ---
func _get_button_from_slot(slot):
	if slot is BaseButton: return slot
	if slot.has_node("TextureButton"): return slot.get_node("TextureButton")
	if slot.has_node("Button"): return slot.get_node("Button")
	return null
