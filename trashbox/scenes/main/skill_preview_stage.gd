extends Node2D

@onready var anim_player = $AnimationPlayer

func play_demo(anim_name: String):
	# 检查动画是否存在
	if anim_player.has_animation(anim_name):
		anim_player.play(anim_name)
	else:
		print("警告：练功房里没有名为 %s 的动画！" % anim_name)
		# 可以播放一个默认的“攻击”动画代替
		# anim_player.play("default_hit")
