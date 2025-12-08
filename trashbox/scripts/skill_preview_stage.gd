extends Node2D

# 场景里的演员
@onready var dummy_enemy = $Dummy        # 挨打的螃蟹
@onready var dummy_player = $Player      # 挂载了 C# PlayerManager 的节点
@onready var anim_player = $AnimationPlayer

func _ready():
	# 调整站位 (根据你的视口大小微调，确保摄像机能看到)
	if dummy_player: dummy_player.position = Vector2(200, 300)
	if dummy_enemy: dummy_enemy.position = Vector2(800, 300)

# --- 核心：接收技能并演示 ---
# 这个函数会被 engine.gd 的 preview_skill() 调用
func play_demo_with_scene(skill_scene: PackedScene):
	
	# 1. 调用 C# Player 的测试接口
	if dummy_player and dummy_player.has_method("TestSkill"):
		print("Preview: C# Player 发射技能...")
		
		# 调用你之前在 C# 里写好的 TestSkill(PackedScene)
		dummy_player.TestSkill(skill_scene)
		
		# 2. 让螃蟹配合闪红 (模拟受击反馈)
		_play_dummy_hit_anim()
		
	else:
		print("错误：Player 没找到或 C# 脚本没编译 TestSkill 方法")

# --- 辅助：螃蟹闪红特效 ---
func _play_dummy_hit_anim():
	if dummy_enemy:
		var tween = create_tween()
		# 延迟 0.2 秒 (模拟子弹飞行时间)
		tween.tween_interval(0.2) 
		# 变红 -> 变回白
		tween.tween_property(dummy_enemy, "modulate", Color.RED, 0.1)
		tween.tween_property(dummy_enemy, "modulate", Color.WHITE, 0.1)
