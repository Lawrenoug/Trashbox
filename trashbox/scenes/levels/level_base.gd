extends Node2D

@onready var hp_bar = $HUD/HPBar

func _ready():
	# 等待一帧，确保 Player 已经生成（如果是动态生成的话）
	# 如果 Player 是直接放在场景里的，可以直接获取
	var player = get_node_or_null("Player") 
	# 或者如果 Player 是由 Engine 生成并塞进来的，我们需要等 Engine 通知
	
	if player:
		_connect_player(player)

# 这个函数供外部（Engine）调用，当它把 Player 放入场景后
func setup_player(player_node):
	_connect_player(player_node)

func _connect_player(player):
	# 连接 C# 信号 "HealthChanged"
	# 注意：C# 的 [Signal] 在 GDScript 中连接时，名字通常保持原样
	if player.has_signal("HealthChanged"):
		player.connect("HealthChanged", _on_health_changed)
		
		# 初始化血条 (假设 player 有 CurrentHP 和 MaxHP 属性)
		# 如果 C# 变量是 private，你需要 C# 提供 Get 方法或改为 public
		# 这里假设你在 C# PlayerManager 里把 blood 改成了 public 或者有对应属性
		hp_bar.value = player.get("blood") 
		hp_bar.max_value = player.get("maxBlood") # 需要你在 C# 加上这个

func _on_health_changed(current, max_hp):
	hp_bar.value = current
	hp_bar.max_value = max_hp
