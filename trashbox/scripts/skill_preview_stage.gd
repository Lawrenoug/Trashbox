extends Node2D

@onready var dummy_enemy = $Dummy        # 敌人
@onready var dummy_player = $Player      # 玩家 (挂载 C# 脚本的节点)
@onready var anim_player = $AnimationPlayer

func _ready():
	# 【关键修复】等待两帧
	# 第1帧：Godot 加载场景
	# 第2帧：父级 UI (Engine) 计算出 SubViewportContainer 的最终大小
	await get_tree().process_frame
	await get_tree().process_frame
	
	_setup_positions()

func _setup_positions():
	# 获取视口大小 (中间那个深灰色区域的实际像素大小)
	var size = get_viewport_rect().size
	
	# 如果 size 还是太小(比如还没展开)，给个保底值防止重叠
	if size.x < 100: size = Vector2(1200, 800)
	
	print("Preview 视口最终大小: ", size)
	
	# 1. 设置玩家位置：左侧 1/5 处，垂直居中
	if dummy_player:
		# Y轴稍微偏下(0.6)看起来更有透视感
		var target_pos = Vector2(size.x * 0.15, size.y * 0.6)
		dummy_player.global_position = target_pos
		
		# 强制重置 C# 刚体物理状态 (防止残留速度)
		dummy_player.linear_velocity = Vector2.ZERO
		dummy_player.angular_velocity = 0

	# 2. 设置敌人位置：右侧 4/5 处
	if dummy_enemy:
		dummy_enemy.global_position = Vector2(size.x * 0.85, size.y * 0.6)

# ... (Play demo 相关的代码保持不变) ...
func play_demo(anim_name: String):
	if anim_player.has_animation(anim_name):
		anim_player.play(anim_name)

func play_demo_with_scene(skill_scene: PackedScene):
	if dummy_player and dummy_player.has_method("TestSkill"):
		dummy_player.TestSkill(skill_scene)
