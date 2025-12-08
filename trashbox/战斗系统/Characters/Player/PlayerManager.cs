using Godot;
using System;
using Attack;

namespace CharacterManager
{
	public partial class PlayerManager : RigidBody2D
	{
		[Export]
		public Node2D startShootPosition;
		
		[Export]
		public Control UIControl; // 注意：在预览场景里这个可能是空的，要判空

		// 【新增】用来手动指定子弹生成的父节点，替代硬编码路径
		[Export]
		public Node BulletContainer; 

		private float blood = 100;
		private float speed = 300;
		public AttackManager attackManager;
		private SkillGroupsUIManager skillGroupsUIManager;

		public override void _Ready()
		{
			// 加个判空，防止在预览场景里因为没有 UI 而报错
			if (UIControl != null)
			{
				skillGroupsUIManager = UIControl.GetChildOrNull<SkillGroupsUIManager>(0);
			}

			// 【修改】优先使用拖拽赋值的节点
			if (BulletContainer != null)
			{
				attackManager = new AttackManager(BulletContainer);
			}
			else 
			{
				// 兼容旧逻辑：如果没拖拽，尝试找旧路径（但建议以后都用拖拽）
				var node = GetNodeOrNull<Node>("/root/Node2D/玩家子弹");
				if (node != null)
				{
					attackManager = new AttackManager(node);
				}
				else
				{
					GD.PrintErr("PlayerManager: 未找到子弹容器 (BulletContainer)！无法攻击。");
				}
			}
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

		// 【新增接口】供 GDScript 调用的测试方法
		public void TestSkill(PackedScene skillScene)
		{
			GD.Print("C# Player: 收到测试技能 -> " + skillScene.ResourcePath);
			// 这里需要对接 AttackManager 的切换技能逻辑
			// 假设 AttackManager 并没有直接切换技能的方法，
			// 你可能需要在这里实例化技能或者通知管理器
			// attackManager.SwitchSkill(skillScene); <--- 需要你队友配合
		}
	}
}
