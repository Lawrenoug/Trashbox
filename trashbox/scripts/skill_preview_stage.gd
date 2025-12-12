extends Node2D

@onready var dummy_enemy = $DummyEnemy       # 敌人
@onready var anim_player = $AnimationPlayer

# 用这个变量存住借来的玩家引用
var current_player_ref = null

func _ready():
	# 等待两帧确保视口尺寸计算完成
	await get_tree().process_frame
	await get_tree().process_frame
	
	# --- 【核心修改】借用 GlobalPlayer ---
	if has_node("/root/GlobalPlayer"):
		current_player_ref = get_node("/root/GlobalPlayer")
		
		# 1. 从原父节点(可能是root)移除
		if current_player_ref.get_parent():
			current_player_ref.get_parent().remove_child(current_player_ref)
		
		# 2. 加到练功房
		add_child(current_player_ref)
		
		# 3. 【关键】强制显示 (因为我们在第一步里把它隐藏了)
		current_player_ref.visible = true
		
		# 4. 【填坑】修复 C# PlayerManager 的子弹容器引用
		# 在练功房里，我们希望子弹打在这个场景里
		if current_player_ref.get("attackManager"):
			current_player_ref.attackManager.set("BulletNode", self)
			# 确保攻击开启
			current_player_ref.attackManager.set("enableAttack", true)
	
	_setup_positions()

func _setup_positions():
	var size = get_viewport_rect().size
	if size.x < 100: size = Vector2(1200, 800)
	
	# 设置玩家位置
	if current_player_ref:
		var target_pos = Vector2(size.x * 0.15, size.y * 0.6)
		current_player_ref.global_position = target_pos
		# 重置物理状态
		current_player_ref.linear_velocity = Vector2.ZERO
		current_player_ref.angular_velocity = 0

	# 设置敌人位置
	if dummy_enemy:
		dummy_enemy.global_position = Vector2(size.x * 0.85, size.y * 0.6)

# --- 演示逻辑保持不变，但要把 target 换成 current_player_ref ---

func play_demo(anim_name: String):
	if anim_player.has_animation(anim_name):
		anim_player.play(anim_name)

func play_demo_with_scene(skill_scene: PackedScene):
	if current_player_ref and is_instance_valid(current_player_ref):
		if current_player_ref.has_method("TestSkill"):
			current_player_ref.TestSkill(skill_scene)
		else:
			print("错误：Player C# 脚本未编译 TestSkill 方法。")

func update_sequence(skill_packed_array: Array):
	if current_player_ref and is_instance_valid(current_player_ref):
		if current_player_ref.has_method("TestSkillSequence"):
			current_player_ref.TestSkillSequence(skill_packed_array)

# --- 【非常重要】当练功房被销毁时，必须归还玩家 ---
func _exit_tree():
	if current_player_ref and is_instance_valid(current_player_ref):
		# 如果玩家还是我的子节点，赶紧把它扔回 root，否则它会跟我一起死
		if current_player_ref.get_parent() == self:
			remove_child(current_player_ref)
			get_tree().root.add_child(current_player_ref)
			# 归还后记得隐藏，防止在桌面显示
			current_player_ref.visible = false
			print("PreviewStage: 已归还 GlobalPlayer 到 Root")
