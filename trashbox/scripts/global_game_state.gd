extends Node

# 记录当前解锁到的关卡索引 (-1:未开始, 0:第一关...)
var current_level_progress: int = -1 

# 桌面场景路径
var desktop_scene_path: String = "res://trashbox/scenes/main/desktop_screen.tscn"

# 【新增】一个标记：如果为 true，桌面加载完毕后会自动打开引擎 App
var should_open_engine_automatically: bool = false
