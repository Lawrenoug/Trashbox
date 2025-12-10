class_name SkillData extends Resource

# 暴露给编辑器填写的属性
@export_group("基本信息")
@export var skill_name: String = "新技能"
@export_multiline var description: String = "技能描述..."
@export var icon: Texture2D 

@export_group("战斗参数")
@export var damage: int = 0
@export var cost: int = 0
@export var animation_name: String = "default_attack"

@export_group("效果配置")
@export var is_passive: bool = false # 是否是被动
# 这里可以加更多参数，比如 effect_type, duration 等
	  
	  
