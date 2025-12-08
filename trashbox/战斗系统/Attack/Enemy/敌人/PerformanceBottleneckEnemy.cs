using Godot;
using System;

namespace Enemy
{
	public partial class PerformanceBottleneckEnemy : EnemyBase
	{
		public override float MaxHP { get; set; } = 100;
		public override float CurrentHP { get; set; } = 100;
		public override float ATK { get; set; } = 15;
		public override float ATS { get; set; } = 5;
		public override float MoveSpeed { get; set; } = 80;
		public override string enemyName { get; set; } = "语法错误";
		public override void _Ready()
		{
			base._Ready();
			state = EnemyState.Attacking;
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
		}

		public override void Attack(double delta)
		{
			timeSinceLastAttack += (float)delta;
			if (ATS <= 0)
			{
				ATS = 0.1f; // 防止除零错误
			}

			// 检查是否到了攻击时间
			float attackInterval = 1.0f / ATS;
			if (timeSinceLastAttack >= attackInterval)
			{
				// 重置计时器
				timeSinceLastAttack = 0f;

				// 添加空值检查以防止NullReferenceException
				if (bulletPrefab == null)
				{
					GD.Print("警告: bulletPrefab未分配");
					return;
				}
				for (int i = -2; i < 3; i++)
				{
					var BulletContainer = GetNode<Node2D>("BulletContainer");
					if (BulletContainer == null)
					{
						GD.Print("错误: 未找到BulletContainer节点");
						return;
					}

					var prefab = bulletPrefab.Instantiate<EnemyBullet>();
					//prefab.GlobalPosition=GlobalPosition;
					if (prefab == null)
					{
						GD.Print("错误: 无法实例化子弹预制体");
						return;
					}

					BulletContainer.AddChild(prefab);
					GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
					prefab.init(GetNode<Node2D>("发射点").GlobalPosition, -10 * i, ATK);
				}
			}
		}
	}
}