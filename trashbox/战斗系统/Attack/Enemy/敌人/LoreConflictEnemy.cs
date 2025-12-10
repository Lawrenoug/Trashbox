using Godot;
using System;

namespace Enemy
{

	public partial class LoreConflictEnemy : EnemyBase
	{
		[Export] public PackedScene bulletPrefab_2;
		[Export] public PackedScene bulletPrefab_3;
		public override float MaxHP { get; set; } = 500;
		public override float CurrentHP { get; set; } = 500;
		public override float ATK { get; set; } = 15;
		public override float ATS { get; set; } = 1;
		public override float MoveSpeed { get; set; } = 80;
		public override string enemyName { get; set; } = "设定冲突";

		private float timeSinceLastAttack_2=0f;
		private float timeSinceLastAttack_3 = 0f;
		private int attackstate = 0;
		
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
			timeSinceLastAttack_2+=(float)delta;
			timeSinceLastAttack_3 += (float)delta;
			float attackInterval = 1.0f / ATS;
			if (timeSinceLastAttack >= attackInterval)
			{
				timeSinceLastAttack = 0f;
				attackstate++;

				if(attackstate>=10)
                {
                    Attack_1();
					if (timeSinceLastAttack_2 >= attackInterval * 10)
					{
						timeSinceLastAttack_2 = 0f;
						Attack_2();
					}
					if(attackstate >= 20)
                    {
                        attackstate = 0;
                    }
                }
				else
                {
                    Attack_3();
                }

			}
			


		}


		private void Attack_1()
		{

			// 发射奇数位置的弹幕 (-170度到170度，步长20度，共18颗)
			for (int i = 0; i < 36; i += 2)
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
				prefab.init(GetNode<Node2D>("发射点").GlobalPosition,  10 * i, ATK);
			}

		}

		private void Attack_2()
		{
			int randomValue = GD.Randi() % 2 == 0 ? -1 : 1;
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
			GD.Print($"{enemyName} 发射子弹2，位置: {GlobalPosition}");
			prefab.init(GetNode<Node2D>("发射点").GlobalPosition + new Vector2(0, randomValue * 200), 0, ATK);

		}

		private void Attack_3()
        {
            for (int i = -2;i  < 3; i++)
            {
                var BulletContainer = GetNode<Node2D>("BulletContainer");
			if (BulletContainer == null)
			{
				GD.Print("错误: 未找到BulletContainer节点");
				return;
			}

			var prefab = bulletPrefab_3.Instantiate<EnemyBullet>();
			//prefab.GlobalPosition=GlobalPosition;
			if (prefab == null)
			{
				GD.Print("错误: 无法实例化子弹预制体");
				return;
			}

			BulletContainer.AddChild(prefab);
			GD.Print($"{enemyName} 发射子弹3，位置: {GlobalPosition}");
			prefab.init(GetNode<Node2D>("发射点").GlobalPosition + new Vector2(0, i * 100), 0, ATK);

            }
        }
	}
}