extends "res://trashbox/scripts/window_base.gd"

# --- 1. 节点引用 ---
# (路径基于你提供的代码，请确保场景结构一致)
# 中间：预览视口
@onready var preview_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
# 右侧：描述文本
@onready var description_text: RichTextLabel = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
# 左下：技能库容器
@onready var deck_grid: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillDeckPanel/DeckContent/DeckScroll/DeckGrid

# --- 2. 资源预加载 ---
# 【重要】你需要创建一个专门用于演示技能的小场景(练功房)，里面放个木桩和发射器
# 如果还没做，请先新建一个 Node2D 场景保存到这个路径，否则会报错
const PreviewStageScene = preload("res://trashbox/scenes/main/skill_preview_stage.tscn")

# 当前加载的预览舞台实例
var current_preview_instance = null

func _ready():
	super._ready() # 必须调用父类
	
	# 1. 初始化中间的预览舞台
	_init_preview_stage()
	
	# 2. 生成左侧的技能列表
	_populate_skill_list()
	
	# 3. 设置右侧默认文本
	description_text.text = "[center]请在左侧选择一个 [color=yellow]模块 (技能)[/color] 查看详细文档。[/center]"

# --- 初始化预览舞台 (中间视口) ---
func _init_preview_stage():
	# 清理残留
	for child in preview_viewport.get_children():
		child.queue_free()
	
	# 实例化练功房
	if PreviewStageScene:
		current_preview_instance = PreviewStageScene.instantiate()
		preview_viewport.add_child(current_preview_instance)
	else:
		print("错误：未找到 PreviewStageScene 场景文件！")

# --- 生成技能列表 (左下侧栏) ---
func _populate_skill_list():
	# 模拟的技能数据 (未来可以从 JSON 或 Resource 读取)
	var skills = [
		{
			"name": "快速排序斩",
			"desc": "对单个目标造成 [color=red]30[/color] 点物理伤害。\n\n[i]时间复杂度 O(nlogn)，非常高效的单体清理手段。[/i]",
			"anim_name": "attack_quick_sort" 
		},
		{
			"name": "递归护盾",
			"desc": "获得 [color=blue]10[/color] 点护甲。\n\n[i]如果护盾被击破，自动调用自身重新生成 50% 的护盾量。小心栈溢出！[/i]",
			"anim_name": "skill_recursion"
		},
		{
			"name": "垃圾回收 (GC)",
			"desc": "回复 [color=green]15[/color] 点生命值，移除自身所有负面状态。\n\n[i]清理内存中的无用对象，让系统重获新生。[/i]",
			"anim_name": "skill_gc"
		},
		{
			"name": "死循环",
			"desc": "对所有敌人施加 [color=purple]眩晕[/color] 1 回合。\n\n[i]while(true) { 敌人无法行动; }[/i]",
			"anim_name": "skill_infinite_loop"
		}
	]
	
	# 清空现有列表
	for child in deck_grid.get_children():
		child.queue_free()
	
	# 动态生成按钮
	for skill_data in skills:
		var btn = Button.new()
		btn.text = skill_data["name"]
		# 设置按钮最小高度，方便点击
		btn.custom_minimum_size = Vector2(0, 40)
		# 连接点击信号，把当前这个技能的数据传过去
		btn.pressed.connect(_on_skill_selected.bind(skill_data))
		deck_grid.add_child(btn)

# --- 核心交互：选中技能 ---
func _on_skill_selected(data):
	# 1. 更新右侧描述
	# 使用 BBCode 进行富文本显示
	var title = "[font_size=24][b]%s[/b][/font_size]\n\n" % data["name"]
	var content = data["desc"]
	description_text.text = title + content
	
	# 2. (可选) 更新中间的演示动画
	# 这里只是打印，等你做好了 PreviewStage 里的动画逻辑，可以在这里调用
	print("正在演示技能动画: ", data["anim_name"])
	
	if current_preview_instance and current_preview_instance.has_method("play_demo"):
		current_preview_instance.play_demo(data["anim_name"])
