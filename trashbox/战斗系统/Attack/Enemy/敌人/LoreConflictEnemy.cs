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

		private float timeState=1f;
		private bool Attack_1_StateFlag=false;

		private bool enableAttack_2=false;
		private int Attack_2_index=0;
		private float timeSinceLastAttack_2=0f;

		
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
				Attack_1();
				
			}

			
		}


		private void Attack_1()
		{

			// 发射奇数位置的弹幕 (-170度到170度，步长20度，共18颗)
			for (int i = 0; i < 18; i += 2)
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
				prefab.init(GetNode<Node2D>("发射点").GlobalPosition, -170 + 20 * i, ATK);
			}

		}

		private void Attack_2(int index)
		{
			for (int i = 1; i < 36; i++)
			{
				var BulletContainer = GetNode<Node2D>("BulletContainer");
				if (BulletContainer == null)
				{
					GD.Print("错误: 未找到BulletContainer节点");
					return;
				}

				var prefab = bulletPrefab_2.Instantiate<PerformanceBottleneckBullet_2>();
				//prefab.GlobalPosition=GlobalPosition;
				if (prefab == null)
				{
					GD.Print("错误: 无法实例化子弹预制体");
					return;
				}

				BulletContainer.AddChild(prefab);
				GD.Print($"{enemyName} 发射子弹2，位置: {GlobalPosition}");
				prefab.init(GetNode<Node2D>("发射点").GlobalPosition, 10 * i, ATK,index);
			}
		}
	}
}