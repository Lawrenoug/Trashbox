extends "res://trashbox/scenes/levels/eventroom.gd"

func _ready():
	event_title = "休息点：橡皮鸭大神"
	event_description = "你遇到了一只巨大的橡皮鸭。\n它什么也没做，但看着它的眼睛，你觉得思路突然清晰了。\n\n要向它倾诉你的 Bug 吗？"
	super._ready()
	
	btn_1.text = "解释代码逻辑 (大量回血)"
	btn_2.text = "捏捏鸭子 (获得临时Buff - 暂未实现)" # 预留给未来功能
	btn_3.text = "谢谢鸭鸭 (离开)"

func _on_option_1_clicked():
	if player_ref:
		var max_hp = player_ref.get("MaxBlood")
		var _current = player_ref.get("CurrentBlood")
		
		# 回满血或者回很多
		player_ref.set("CurrentBlood", max_hp)
		player_ref.TakeDamage(0) # 刷新UI
		
		desc_label.text = "你向鸭子解释了为什么你的 if-else 写错了。\n鸭子没有说话，但你悟了。\n(生命值已完全恢复)"
	_leave_event()

func _on_option_2_clicked():
	# 这里可以扩展：给 C# Player 加一个 status 或者 buff
	# 比如 player_ref.AddBuff("Inspired")
	desc_label.text = "嘎吱——\n这声音真解压。"
	_leave_event()
