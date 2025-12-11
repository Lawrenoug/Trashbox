using Godot;
using System;
using System.Collections.Generic; // 引用 List 需要这个
using Attack;

namespace CharacterManager
{
	public partial class PlayerManager : RigidBody2D
	{
		// --- 信号定义 (供 GDScript 连接) ---
		[Signal] public delegate void HealthChangedEventHandler(float current, float max);
		[Signal] public delegate void PlayerDiedEventHandler();

		[Export]
		public Node2D startShootPosition;
		
		[Export]
		public Control UIControl;

		[Export]
		public Node BulletContainer; 

		// --- 血量属性 (修改为 Public/Export 以便 GDScript 访问) ---
		[Export] 
		public float MaxBlood = 100; // 最大血量

		public float CurrentBlood;   // 当前血量 (Public 让 GDScript 能 get)

		private float speed = 300;
		private float SpeedCutTime=3;
		public AttackManager attackManager;
		private SkillGroupsUIManager skillGroupsUIManager;

		public override void _Ready()
		{
			// 1. 初始化血量
			CurrentBlood = MaxBlood;

			// 2. 初始化 UI 管理器
			if (UIControl != null)
			{
				skillGroupsUIManager = UIControl.GetChildOrNull<SkillGroupsUIManager>(0);
			}

			// 3. 初始化攻击管理器
			if (BulletContainer != null)
			{
				attackManager = new AttackManager(BulletContainer);
			}
			else 
			{
				var node = GetNodeOrNull<Node>("/root/DesktopScreen/WindowBase/BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport/SkillPreviewStage/玩家子弹");
				if (node != null)
				{
					attackManager = new AttackManager(node);
				}
				else
				{
					// 在预览场景里如果没有 BulletContainer 是正常的，不报错，但在实际游戏里需要
					// GD.Print("PlayerManager: 未设置 BulletContainer，暂时无法攻击。");
				}
			}

			// 4. 发送初始血量信号 (让 UI 更新)
			// 使用 CallDeferred 确保在 GDScript 连接信号之后再发送
			CallDeferred("EmitInitialSignal");
		}

		private void EmitInitialSignal()
		{
			EmitSignal(SignalName.HealthChanged, CurrentBlood, MaxBlood);
		}

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);
			Move(delta);
			
			if(attackManager != null)
			{
				attackManager.AttackLoop((float)delta,startShootPosition.GlobalPosition);
                
            }
			if(speed>=0)
			{
				SpeedCutTime--;
			}
			else
            {
                SpeedRestore();
            }
		}

		private void Move(double delta)
		{
			// 获取当前视口大小 (兼容 SubViewport)
			var viewSize = GetViewportRect().Size;

			if(GlobalPosition.X <= 0) GlobalPosition = new Vector2(0, GlobalPosition.Y);
			if(GlobalPosition.X >= viewSize.X) GlobalPosition = new Vector2(viewSize.X, GlobalPosition.Y);
			if(GlobalPosition.Y <= 0) GlobalPosition = new Vector2(GlobalPosition.X, 0);
			if(GlobalPosition.Y >= viewSize.Y) GlobalPosition = new Vector2(GlobalPosition.X, viewSize.Y);

			Vector2 inputDirection = Input.GetVector("左", "右", "上", "下");
			var Velocity = inputDirection * speed;
			Position += Velocity * (float)delta;
		}

		// --- 受伤逻辑 ---
		public void TakeDamage(float damage)
		{
			CurrentBlood -= damage;
			if (CurrentBlood < 0) CurrentBlood = 0;

			// 发送信号给 GDScript (更新血条)
			EmitSignal(SignalName.HealthChanged, CurrentBlood, MaxBlood);
			
			if (CurrentBlood <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			GD.Print("Player Died");
			EmitSignal(SignalName.PlayerDied);
			// 这里可以添加死亡动画，或者暂时直接销毁
			// QueueFree(); // 建议先不销毁，或者由 Engine 处理失败逻辑后再销毁
		}

		// --- 供 GDScript 调用的单技能测试接口 ---
		// public void TestSkill(PackedScene skillScene)
		// {
		// 	// GD.Print("C# Player: 收到测试技能 -> " + skillScene.ResourcePath);
			
		// 	if (attackManager == null) return;

		// 	// 1. 实例化技能场景
		// 	var skillNode = skillScene.Instantiate();
			
		// 	// 2. 获取 Skill 组件
		// 	// 注意：你的技能场景根节点是 Sprite2D 并且挂了 Skill 脚本 (如 Pixel.cs)
		// 	// 所以我们需要把它转为 Skill 类型
		// 	var skillScript = skillNode as Skill;

		// 	if (skillScript != null)
		// 	{
		// 		// 3. 构造一个临时的技能列表
		// 		var testSkillList = new System.Collections.Generic.List<Skill>();
		// 		testSkillList.Add(skillScript);

		// 		// 4. 塞给 AttackManager
		// 		// 这会覆盖当前的技能组，正好符合“演示”的需求
		// 		attackManager.InsertSkill(testSkillList);
				
		// 		// 5. 确保子弹容器有效 (防止这里还是空的)
		// 		if (attackManager.BulletNode == null && BulletContainer != null)
		// 		{
		// 			attackManager.BulletNode = BulletContainer;
		// 		}
		// 	}
		// 	else
		// 	{
		// 		GD.PrintErr("TestSkill 错误: 传入的场景根节点不是 Skill 类型！");
		// 		skillNode.QueueFree(); // 清理无效实例
		// 	}
		// }

		// // --- 【新增】供 GDScript 调用的技能序列测试接口 ---
		// // 这个方法接收一个 PackedScene 数组，实例化后作为一个完整的技能组传给 AttackManager
		// public void TestSkillSequence(Godot.Collections.Array<PackedScene> skillScenes)
		// {
		// 	if (attackManager == null) return;
		// 	if (skillScenes == null || skillScenes.Count == 0) return;

		// 	// 1. 创建一个新的 C# 列表
		// 	var sequenceList = new System.Collections.Generic.List<Skill>();

		// 	// 2. 遍历传入的所有场景
		// 	foreach (var scene in skillScenes)
		// 	{
		// 		if (scene == null) continue;

		// 		// 实例化技能节点
		// 		var node = scene.Instantiate();
				
		// 		// 尝试转为 Skill 类型
		// 		if (node is Skill skillScript)
		// 		{
		// 			sequenceList.Add(skillScript);
		// 		}
		// 		else
		// 		{
		// 			// 如果不是 Skill 类型，清理掉防止内存泄漏
		// 			node.QueueFree();
		// 		}
		// 	}

		// 	// 3. 将整个列表塞给攻击管理器
		// 	// 这样 AttackManager 的 AttackLoop 就会按顺序通过 index 循环这些技能
		// 	attackManager.InsertSkill(sequenceList);
			
		// 	// 4. 确保子弹容器有效
		// 	if (attackManager.BulletNode == null && BulletContainer != null)
		// 	{
		// 		attackManager.BulletNode = BulletContainer;
		// 	}
			
		// 	GD.Print($"C# Player: 已更新技能序列，包含 {sequenceList.Count} 个技能模块");
		// }

		public void SpeedCut()
        {
            speed *= 0.5f;
			SpeedCutTime = 3;
        }

		public void SpeedRestore()
        {
            speed *= 2f;
        }
		
	}
}
