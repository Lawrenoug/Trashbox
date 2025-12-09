using Godot;
using System;

namespace Enemy
{
	public partial class PerformanceBottleneckEnemy : EnemyBase
	{
		[Export] public PackedScene bulletPrefab_2;
		public override float MaxHP { get; set; } = 500;
		public override float CurrentHP { get; set; } = 500;
		public override float ATK { get; set; } = 15;
		public override float ATS { get; set; } = 5;
		public override float MoveSpeed { get; set; } = 80;
		public override string enemyName { get; set; } = "性能瓶颈";

		private float timeState=1f;
		private bool Attack_1_StateFlag=false;

		
		public override void _Ready()
		{
			base._Ready();
			state = EnemyState.Attacking;
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			Attack(delta);
		}

		public override void Attack(double delta)
		{
			timeSinceLastAttack+=(float)delta;
			float attackInterval = 1.0f / ATS;
			if(timeSinceLastAttack>=attackInterval)
			{
				timeSinceLastAttack=0f;
				timeState++;
				if(timeState>=1000)//设定阈值
				{
					timeState=1f;
				}
				if(timeState%10==0)
                {
                    Attack_1(Attack_1_StateFlag);
					Attack_1_StateFlag=!Attack_1_StateFlag;
                }
				else
                {
					Attack_1(Attack_1_StateFlag);
                }
			}
			
		}


		private void Attack_1(bool index)
		{
			if (index)
			{
				// 发射奇数位置的弹幕 (-170度到170度，步长20度，共18颗)
				for (int i = 0; i < 18; i+=2)
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
					// 角度从-170度开始，每隔20度发射一颗
					prefab.init(GetNode<Node2D>("发射点").GlobalPosition, -170 + 20 * i, ATK);
				}
			}
			else
			{
				// 发射偶数位置的弹幕 (-160度到160度，步长20度，共17颗)
				for (int i = 1; i < 18; i+=2)
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
					// 角度从-160度开始，每隔20度发射一颗
					prefab.init(GetNode<Node2D>("发射点").GlobalPosition, -170 + 20 * i, ATK);
				}
			}
		}

		private void Attack_2(int index)
		{
			// for (int i = -10; i < 11; i++)
			// {
			// 	var BulletContainer = GetNode<Node2D>("BulletContainer");
			// 	if (BulletContainer == null)
			// 	{
			// 		GD.Print("错误: 未找到BulletContainer节点");
			// 		return;
			// 	}

			// 	var prefab = bulletPrefab_2.Instantiate<PerformanceBottleneckBullet_2>();
			// 	//prefab.GlobalPosition=GlobalPosition;
			// 	if (prefab == null)
			// 	{
			// 		GD.Print("错误: 无法实例化子弹预制体");
			// 		return;
			// 	}

			// 	BulletContainer.AddChild(prefab);
			// 	GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
			// 	prefab.init(GetNode<Node2D>("发射点").GlobalPosition, -10 * i, ATK,index);
			// }
		}
	}
}