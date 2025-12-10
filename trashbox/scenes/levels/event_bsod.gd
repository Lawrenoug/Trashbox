extends "res://trashbox/scenes/levels/eventroom.gd"

func _ready():
	event_title = "系统警告：致命异常"
	event_description = "检测到严重的内存溢出 (Stack Overflow)。\n面前的蓝色方尖碑正在吞噬周围的数据。\n\n你要怎么做？"
	super._ready()
	
	btn_1.text = "强制重启 (恢复 30% 血量)"
	btn_2.text = "尝试超频 (扣 20 血，+50 最大生命)"
	btn_3.text = "忽略错误 (离开)"

func _on_option_1_clicked():
	if player_ref:
		var max_hp = player_ref.get("MaxBlood")
		var current = player_ref.get("CurrentBlood")
		# 这是一个简单的加法，但在 C# 里要注意不要超过上限
		# 可以在 C# 加个 Heal 方法，或者直接在这里改
		var heal_amount = max_hp * 0.3
		player_ref.set("CurrentBlood", min(current + heal_amount, max_hp))
		
		# 触发一下受伤逻辑来更新UI (有点hack，但有效)
		player_ref.TakeDamage(0) 
		
		desc_label.text = "你按下了重启键。\n系统缓存已清除，感觉清爽多了。"
	_leave_event()

func _on_option_2_clicked():
	if player_ref:
		var current = player_ref.get("CurrentBlood")
		var max_hp = player_ref.get("MaxBlood")
		
		if current > 20:
			# 扣血
			player_ref.TakeDamage(20)
			# 加上限
			player_ref.set("MaxBlood", max_hp + 50)
			
			desc_label.text = "你触碰了蓝屏...\n虽然丢失了部分数据，但你的内存容量扩大了！"
		else:
			desc_label.text = "系统提示：电量不足，无法超频！"
	_leave_event()
