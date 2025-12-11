using Godot;
using System;

namespace Enemy
{
	public partial class TheFinalDeadlineEnemy : EnemyBase
	{
		[Export] public PackedScene bulletPrefab_2;
		[Export] public PackedScene bulletPrefab_3;
		[Export] public PackedScene bulletPrefab_4;
		public override float MaxHP { get; set; } = 500;
		public override float CurrentHP { get; set; } = 50;
		public override float ATK { get; set; } = 15;
		public override float ATS { get; set; } = 5;
		public override float MoveSpeed { get; set; } = 80;
		public override string enemyName { get; set; } = "最终截止日期";

		private float[] angle_1 = { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 55, 45, 40, 35, 30, 25, 20, 15, 10, 5, 0, -5, -10, -15, -20, -25, -30, -35, -40, -45, -50, -55, -60, -55, -45, -40, -35, -30, -25, -20, -15, -10, -5 };
		private float[] angle_2 = { 0, -5, -10, -15, -20, -25, -30, -35, -40, -45, -50, -55, -60, -55, -45, -40, -35, -30, -25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 55, 45, 40, 35, 30, 25, 20, 15, 10, 5 };
		private int angle_Index = 0;

		private float radio = 30f;

		private float timeSinceLastAttack_2 = 0f;

		private float timeSinceLastAttack_3 = 0f;

		private int attack5_index = 0;


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
			timeSinceLastAttack += (float)delta;
			timeSinceLastAttack_2 += (float)delta;
			timeSinceLastAttack_3+= (float)delta;
			float attackInterval = 1.0f / ATS;


			if (CurrentHP / MaxHP >= 0.65f)
			{
				if (timeSinceLastAttack >= attackInterval)
				{
					timeSinceLastAttack = 0;
					if (angle_Index == angle_1.Length)
					{
						angle_Index = 0;
					}
					Attack_1(angle_1[angle_Index++]);

				}
				if (timeSinceLastAttack_2 >= attackInterval * 10)
				{
					timeSinceLastAttack_2 = 0;
					int attack2_index = GD.Randi() % 2 == 0 ? 1 : -1;
					Attack_2(attack2_index);
				}

			}
			else if (CurrentHP / MaxHP >= 0.35f)
			{
				if (timeSinceLastAttack >= attackInterval * 2)
				{
					timeSinceLastAttack = 0;
					if (angle_Index == angle_2.Length)
					{
						angle_Index = 0;
					}
					Attack_3(angle_1[angle_Index++], -1);
					Attack_3(angle_2[angle_Index++], 1);

				}
				if (timeSinceLastAttack_2 >= attackInterval * 15)
				{
					timeSinceLastAttack_2 = 0;
					Attack_4();
				}
			}
			else
			{

				
				if (timeSinceLastAttack_2 >= attackInterval * 2.5f)
				{
					timeSinceLastAttack_2 = 0;
					Attack_6();
				}
				if (timeSinceLastAttack >= attackInterval * 5)
				{
					timeSinceLastAttack = 0;

					if (attack5_index == 0)
					{
						for (int i = 1; i < 36; i = i + 2)
						{
							Attack_5(i * 10f);
						}
						attack5_index = 1;
					}
					else if (attack5_index == 1)
					{
						for (int i = 0; i < 36; i = i + 2)
						{
							Attack_5(i * 10f);
						}
						attack5_index = 0;
					}
				}
				if (timeSinceLastAttack_3 >= attackInterval *15 )
                {
                    timeSinceLastAttack_3 = 0;
					Attack_7();
                }

			}






		}


		private void Attack_1(float _angle)
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
			//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
			// 角度从-170度开始，每隔20度发射一颗
			prefab.init(GetNode<Node2D>("发射点").GlobalPosition, _angle, ATK);


		}

		private void Attack_2(int _index)
		{

			for (int i = 0; i < 36; i++)
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
				//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
				// 角度从-170度开始，每隔20度发射一颗
				// 计算圆形排列的子弹位置和角度
				float bulletAngle = i * 10f; // 每10度一个子弹
				float angleInRadians = bulletAngle * Mathf.Pi / 180f;
				Vector2 circleOffset = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * radio;
				Vector2 centerPosition = GetNode<Node2D>("发射点").GlobalPosition + new Vector2(0, _index * 100);
				Vector2 bulletPosition = centerPosition + circleOffset;

				prefab.init(bulletPosition, bulletAngle, ATK);
			}


		}

		private void Attack_3(float _angle, int _index)
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
			//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
			// 角度从-170度开始，每隔20度发射一颗
			prefab.init(GetNode<Node2D>("发射点").GlobalPosition + new Vector2(0, _index * 100), _angle, ATK);
		}

		private void Attack_4()
		{
			int attack4_index = GD.Randi() % 2 == 0 ? 1 : -1;
			var BulletContainer = GetNode<Node2D>("BulletContainer");
			if (BulletContainer == null)
			{
				GD.Print("错误: 未找到BulletContainer节点");
				return;
			}

			var prefab = bulletPrefab_2.Instantiate<EnemyBullet>();
			//prefab.GlobalPosition=GlobalPosition;
			if (prefab == null)
			{
				GD.Print("错误: 无法实例化子弹预制体");
				return;
			}

			BulletContainer.AddChild(prefab);
			//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
			// 角度从-170度开始，每隔20度发射一颗
			prefab.init(GetNode<Node2D>("发射点").GlobalPosition + new Vector2(0, attack4_index * 150), 0, ATK);
		}

		private void Attack_5(float _angle)
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
			//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
			// 角度从-170度开始，每隔20度发射一颗
			prefab.init(GetNode<Node2D>("发射点").GlobalPosition, _angle, ATK);


		}

		private void Attack_6()
		{
			for (int i = -1; i <= 1; i += 2)
			{
				var BulletContainer = GetNode<Node2D>("BulletContainer");
				if (BulletContainer == null)
				{
					GD.Print("错误: 未找到BulletContainer节点");
					return;
				}

				var prefab = bulletPrefab_3.Instantiate<TheFinalDeadlineBullet_3>();
				//prefab.GlobalPosition=GlobalPosition;
				if (prefab == null)
				{
					GD.Print("错误: 无法实例化子弹预制体");
					return;
				}

				BulletContainer.AddChild(prefab);
				//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
				// 角度从-170度开始，每隔20度发射一颗
				prefab.init(GetNode<Node2D>("发射点").GlobalPosition + new Vector2(0, i * 200), 0, ATK, i);

			}
		}

		private void Attack_7()
		{

			var BulletContainer = GetNode<Node2D>("BulletContainer");
			if (BulletContainer == null)
			{
				GD.Print("错误: 未找到BulletContainer节点");
				return;
			}

			var prefab = bulletPrefab_4.Instantiate<EnemyBullet>();
			//prefab.GlobalPosition=GlobalPosition;
			if (prefab == null)
			{
				GD.Print("错误: 无法实例化子弹预制体");
				return;
			}

			BulletContainer.AddChild(prefab);
			//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
			// 角度从-170度开始，每隔20度发射一颗
			prefab.init(GetNode<Node2D>("发射点").GlobalPosition, 0, ATK);

		}
	}
}