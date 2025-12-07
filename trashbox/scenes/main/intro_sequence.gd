extends Control

# --- 配置区域 ---
# 1. 插画列表 (请在编辑器里把图片按顺序拖进去)
@export var images: Array[Texture2D]

# 2. 对应的文字列表 (数量要和图片一样)
@export_multiline var texts: Array[String]

# 3. 播放完要去哪里？(把 login_screen.tscn 拖进来)
@export var next_scene: PackedScene

# 4. 每张图停留几秒？
@export var display_time: float = 3.0
@export var fade_time: float = 1.0

# --- 内部变量 ---
@onready var illustration = $Illustration
@onready var story_text = $StoryText
@onready var fade_layer = $FadeLayer
@onready var skip_button = $SkipButton

var current_index = 0
var is_transitioning = false

func _ready():
	# 初始化：全黑
	fade_layer.color.a = 1.0
	skip_button.pressed.connect(_on_skip_pressed)
	
	# 开始播放序列
	if images.size() > 0:
		_play_slide()
	else:
		# 如果没配图片，直接进游戏
		_change_scene()

func _play_slide():
	if current_index >= images.size():
		_change_scene() # 播完了
		return
	
	is_transitioning = true
	
	# 1. 换图、换字
	illustration.texture = images[current_index]
	# 防止文字数组越界
	if current_index < texts.size():
		story_text.text = texts[current_index]
	else:
		story_text.text = ""
	
	# 2. 淡入 (黑 -> 透明)
	var tween = create_tween()
	tween.tween_property(fade_layer, "color:a", 0.0, fade_time)
	await tween.finished
	
	is_transitioning = false
	
	# 3. 停留展示
	# 使用 create_timer 并等待它，这样方便被跳过打断
	await get_tree().create_timer(display_time).timeout
	
	# 4. 淡出 (透明 -> 黑)
	is_transitioning = true
	var tween_out = create_tween()
	tween_out.tween_property(fade_layer, "color:a", 1.0, fade_time)
	await tween_out.finished
	
	# 5. 下一张
	current_index += 1
	_play_slide()

# 跳转到登录界面
func _change_scene():
	if next_scene:
		get_tree().change_scene_to_packed(next_scene)
	else:
		print("错误：未设置 Next Scene！")

# 跳过按钮逻辑
func _on_skip_pressed():
	_change_scene()

# (可选) 允许点击鼠标左键加速/下一步
func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		# 这里可以写点击屏幕立即显示完文字，或者立即切下一张的逻辑
		# 简单起见，这里暂不处理，防止打断 Tween 导致报错
		pass
