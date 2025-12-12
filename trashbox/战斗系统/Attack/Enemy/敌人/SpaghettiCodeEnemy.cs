using Godot;
using System;


namespace Enemy
{
	public partial class SpaghettiCodeEnemy : EnemyBase
	{
		public override float MaxHP { get; set; } = 500;
		public override float CurrentHP { get; set; } = 500;
		public override float ATK { get; set; } = 15;
		public override float ATS { get; set; } = 1;
		public override float MoveSpeed { get; set; } = 80;
		public override string enemyName { get; set; } = "屎山代码";

		
		
		public override void _Ready()
		{
			base._Ready();
			//state = EnemyState.Attacking;
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
			if (timeSinceLastAttack >= attackInterval)
			{
				timeSinceLastAttack = 0f;
				
                Attack_1();
					
                

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

				var prefab = bulletPrefab.Instantiate<EnemyBullet>();
				//prefab.GlobalPosition=GlobalPosition;
				if (prefab == null)
				{
					GD.Print("错误: 无法实例化子弹预制体");
					return;
				}
				var node = GetTree().GetFirstNodeInGroup("敌人子弹") as Node2D;
				node.AddChild(prefab);
				//BulletContainer.AddChild(prefab);
				//GD.Print($"{enemyName} 发射子弹，位置: {GlobalPosition}");
				// 角度从-170度开始，每隔20度发射一颗
				prefab.init(GetNode<Node2D>("发射点").GlobalPosition,0, ATK);
			

		}
	}
}