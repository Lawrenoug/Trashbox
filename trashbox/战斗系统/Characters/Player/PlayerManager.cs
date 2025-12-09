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
				var node = GetNodeOrNull<Node>("/root/Node2D/玩家子弹");
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
				attackManager.AttackLoop((float)delta, startShootPosition.GlobalPosition);
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

		// --- 供 GDScript 调用的技能演示接口 ---
		// 【修改接口】供 GDScript 调用的测试方法
		public void TestSkill(PackedScene skillScene)
		{
			// GD.Print("C# Player: 收到测试技能 -> " + skillScene.ResourcePath);
			
			if (attackManager == null) return;

			// 1. 实例化技能场景
			var skillNode = skillScene.Instantiate();
			
			// 2. 获取 Skill 组件
			// 注意：你的技能场景根节点是 Sprite2D 并且挂了 Skill 脚本 (如 Pixel.cs)
			// 所以我们需要把它转为 Skill 类型
			var skillScript = skillNode as Skill;

			if (skillScript != null)
			{
				// 3. 构造一个临时的技能列表
				var testSkillList = new System.Collections.Generic.List<Skill>();
				testSkillList.Add(skillScript);

				// 4. 塞给 AttackManager
				// 这会覆盖当前的技能组，正好符合“演示”的需求
				attackManager.InsertSkill(testSkillList);
				
				// 5. 确保子弹容器有效 (防止这里还是空的)
				if (attackManager.BulletNode == null && BulletContainer != null)
				{
					attackManager.BulletNode = BulletContainer;
				}
				
				// 技能被 Insert 后，AttackLoop 会自动在下一帧处理发射
				// 如果你想强制立即发射，可能需要手动重置 _timeDelay
				// 但通常 InsertSkill 会重置状态，等待自然发射即可
			}
			else
			{
				GD.PrintErr("TestSkill 错误: 传入的场景根节点不是 Skill 类型！");
				skillNode.QueueFree(); // 清理无效实例
			}
		}
	}
}
