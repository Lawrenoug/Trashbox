extends Control
# --- 导出变量 ---
@export var correct_password: String = "123456"
@export var desktop_scene: PackedScene

# --- 节点引用 (最终版路径，请根据你的场景树核对MarginContainer的名字) ---
@onready var password_input: LineEdit = $LoginBox/MarginContainer/PasswordInput
@onready var error_label: Label = $LoginBox/ErrorLabel
@onready var login_button: Button = $LoginBox/MarginContainer2/LoginButton # 假设第二个是MarginContainer2
@onready var error_timer: Timer = $ErrorTimer


func _ready():
	password_input.secret = true
	password_input.grab_focus()
	
	# --- 连接信号 ---
	login_button.pressed.connect(_on_login_attempt)
	error_timer.timeout.connect(_on_error_timer_timeout)
	
	# 【新增】连接输入框的回车信号
	# 当用户在输入框按回车时，会触发这个信号
	password_input.text_submitted.connect(_on_password_enter)
	
func _on_password_enter(_new_text: String):
	_on_login_attempt()

func _on_login_attempt():
	var input_text = password_input.text
	
	if input_text == correct_password:
		if desktop_scene:
			# 【恢复为最直接的场景切换方式】
			get_tree().change_scene_to_packed(desktop_scene)
		else:
			print("错误：未在检查器中设置要跳转的桌面场景！")
	else:
		# 密码错误的代码保持不变
		error_label.visible = true
		password_input.clear()
		password_input.grab_focus()
		error_timer.start(2.0)

func _on_error_timer_timeout():
	error_label.visible = false

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.is_pressed():
		if event.keycode == KEY_ENTER or event.keycode == KEY_KP_ENTER:
			_on_login_attempt()
			
			get_tree().get_root().set_input_as_handled()
