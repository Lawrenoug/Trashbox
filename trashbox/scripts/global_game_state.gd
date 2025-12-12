extends Node

# --- 关卡进度 ---
var current_level_progress: int = -1 
var desktop_scene_path: String = "res://trashbox/scenes/main/desktop_screen.tscn"
var should_open_engine_automatically: bool = false

# --- 玩家状态存档 ---
var player_current_hp: float = 100.0
var player_max_hp: float = 100.0

# 1. 战斗列表技能存档 (正在装备的)
var saved_skill_paths: Array[String] = [] 

# 2. 【新增】背包技能存档 (仓库里的)
var saved_backpack_paths: Array[String] = []
