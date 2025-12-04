extends "res://trashbox/scripts/window_base.gd"

# --- 1. 引用那个“洞” (SubViewport) ---
# 【重要】请去场景树里右键 GameViewport -> 复制节点路径 -> 替换下面的路径
@onready var game_viewport = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport
@onready var description_text: RichTextLabel = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/DescriptionPanel/DescriptionText
@onready var deck_grid: GridContainer = $BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillDeckPanel/DeckContent/DeckScroll/DeckGrid

# --- 2. 关卡池 ---
# 在编辑器里把你的 level_01.tscn, level_02.tscn 拖进去
@export var level_scenes: Array[PackedScene]

# 当前正在运行的关卡实例
var current_level = null

func _ready():
	super._ready() # 别忘了调用父类
	
	# 引擎启动时，加载第一个关卡（或者随机关卡）
	load_level(0)

# --- 3. 加载关卡的核心功能 ---
func load_level(index: int):
	# A. 安全检查
	if level_scenes.is_empty():
		print("错误：没有设置关卡场景！请在 Inspector 里的 Level Scenes 添加关卡。")
		return
	
	# B. 清理旧关卡 (如果之前有加载过)
	if current_level != null:
		current_level.queue_free()
	
	# 这里的逻辑是清空 Viewport 下所有东西，防止残留
	for child in game_viewport.get_children():
		child.queue_free()
	
	# C. 实例化新关卡
	var scene_resource = level_scenes[index] # 这里你可以改成 pick_random()
	current_level = scene_resource.instantiate()
	
	# D. 【最关键的一步】把关卡放入视口
	game_viewport.add_child(current_level)
	
	print("关卡已加载到游戏引擎视口中")
