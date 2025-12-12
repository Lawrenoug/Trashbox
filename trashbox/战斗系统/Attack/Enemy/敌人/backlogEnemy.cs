using Godot;
using System;

namespace Enemy
{
	public partial class backlogEnemy : EnemyBase
	{
		public override float MaxHP { get; set; } = 100;
		public override float CurrentHP { get; set; } = 100;
		public override float ATK { get; set; } = 10;
		public override float ATS { get; set; } = 10;
		public override float MoveSpeed { get; set; } = 150;
		public override string enemyName { get; set; } = "待办事项";

		private Vector2 targetPosition=new Vector2();

		private float timeSinceLastAttack_2 = 0;
		// 添加用于追踪快速射击的变量
		private float burstTimer = 0f;
		private int bulletsFiredInBurst = 0;
		private bool isBursting = false;
		public override void _Ready()
		{
			base._Ready();
			var node=GetTree().GetFirstNodeInGroup("player") as RigidBody2D;
			targetPosition=node.GlobalPosition;
			//state = EnemyState.Attacking;
		}
		
		public override void _Process(double delta)
		{
			base._Process(delta);
			//Attack(delta);
		}

		public override void Attack(double delta)
		{
			timeSinceLastAttack+=(float)delta;
			timeSinceLastAttack_2+=(float)delta;
			// 每2秒更新目标位置
			if(timeSinceLastAttack_2>=1)
			{
				var node=GetTree().GetFirstNodeInGroup("player") as RigidBody2D;
				targetPosition=node.GlobalPosition;
				timeSinceLastAttack_2 = 0; // 重置计时器
				
				// 开始一轮快速射击
				isBursting = true;
				burstTimer = 0f;
				bulletsFiredInBurst = 0;
			}
			
			// 处理快速射击逻辑
			if (isBursting)
			{
				burstTimer += (float)delta;
				
				// 每0.3秒发射一颗子弹，总共5颗
				if (burstTimer >= 0.3f && bulletsFiredInBurst < 5)
				{
					Attack_1();
					bulletsFiredInBurst++;
					burstTimer = 0f;
				}
				
				// 如果已经发射了10颗子弹，停止快速射击
				if (bulletsFiredInBurst >= 10)
				{
					isBursting = false;
				}
			}
		}

		private void Attack_1()
		{
			
				var BulletContainer = GetNode<Node2D>("BulletContainer");
				if (BulletContainer == null)
				{
					GD.Print("错误: 未找到BulletContainer节点");
					return;
				}

				var prefab = bulletPrefab.Instantiate<backlogBullet>();
				//prefab.GlobalPosition=GlobalPosition;
				if (prefab == null)
				{
					GD.Print("错误: 无法实例化子弹预制体");
					return;
				}

				var node = GetTree().GetFirstNodeInGroup("敌人子弹") as Node2D;
				node.AddChild(prefab);
				GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
				prefab.init(GetNode<Node2D>("发射点").GlobalPosition,0, ATK,targetPosition);
			
		}
	}
}
