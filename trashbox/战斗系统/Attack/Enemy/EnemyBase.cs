using Godot;
using System;
using Attack;
using Buff;

namespace Enemy
{
	public abstract partial class EnemyBase : RigidBody2D
	{
		public virtual float MaxHP { get; set; } = 100;
		public virtual float CurrentHP { get; set; } = 100;
		public virtual float ATK { get; set; } = 10;
		public virtual float ATS { get; set; } = 1;
		public virtual float MoveSpeed { get; set; } = 100;
		public virtual string enemyName { get; set; } = "Enemy";
		public virtual string state { get; set; } = "";

		public virtual BuffBase[] CurrentBuff { get; set; }

		public virtual float DamageMultiplier { get; set; } = 1;

		public override void _Ready()
		{
			base._Ready();

			// 连接碰撞信号
			Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
		}

		public override void _Process(double delta)
		{
			if (CurrentHP <= 0)
			{
				Die();
			}
		}

		public void OnBodyEntered(Node body)
		{
			// if (body.IsInGroup("Bullet"))
			// {
			// 	Bullet bullet = body as Bullet;
			// 	TakeDamage(bullet.ATK);
			// }
		}

		public virtual void Move()
		{

		}

		public virtual void Attack()
		{

		}

		public virtual void TakeDamage(float damage)
		{
			CurrentHP -=damage* DamageMultiplier;
			GD.Print($"{enemyName} 受到 {damage} 点伤害，当前生命值：{CurrentHP}/{MaxHP}");
		}

		public virtual void Die()
		{
			QueueFree();
			GD.Print($"{enemyName} 死亡");
		}

		public virtual void ApplyBuff(BuffBase buff)
		{
			
		}
	}
}