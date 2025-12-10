extends "res://trashbox/scenes/levels/eventroom.gd"

func _ready():
	event_title = "发现：废弃代码仓库 (Legacy Repo)"
	event_description = "这里存放着前任程序员留下的“屎山”代码。\n虽然看着很乱，但也许能找到能用的模块。\n\n要做一次 Code Review 吗？"
	super._ready()
	
	btn_1.text = "重构代码 (小幅回血)"
	btn_2.text = "复制粘贴 (随机获取技能 / 受伤)"
	btn_3.text = "git push -f (离开)"

func _on_option_1_clicked():
	if player_ref:
		# 简单的回血
		player_ref.TakeDamage(-10) # 负伤害 = 回血 (取决于你C#怎么写的，或者直接改属性)
		desc_label.text = "你整理了缩进和注释。\n强迫症得到了满足，精神状态恢复了。"
	_leave_event()

func _on_option_2_clicked():
	if player_ref:
		# 随机判定
		var chance = randf()
		
		if chance > 0.4: # 60% 几率成功
			if reward_pool.size() > 0:
				var random_skill = reward_pool.pick_random()
				desc_label.text = "编译成功！\n这段代码居然能跑！你获得了一个新模块。"
				# 调用 C# 的测试接口直接给技能
				player_ref.TestSkill(random_skill)
			else:
				desc_label.text = "仓库是空的... (请在编辑器里配置 Reward Pool)"
		else:
			# 40% 几率失败
			player_ref.TakeDamage(15)
			desc_label.text = "编译错误！\n产生了 999 个报错，你的心态崩了 (受到 15 点伤害)。"
	_leave_event()
