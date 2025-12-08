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
			// 检测是否与玩家子弹碰撞
			if (body.IsInGroup("PlayerBullet"))
			{
				OnHitByPlayerBullet(body);
			}
		}

		// 虚方法：当被玩家子弹击中时调用
		public virtual void OnHitByPlayerBullet(Node bullet)
		{
			// 默认实现可以根据需要在子类中重写
			GD.Print($"{enemyName} 被玩家子弹击中");
		}

		public virtual void Move()
		{

		}

		public virtual void Attack()
		{

		}

		// 虚方法：开始特殊攻击（如弹幕攻击）
		public virtual void StartSpecialAttack()
		{
			// 可在子类中实现具体的特殊攻击逻辑
		}

		// 虚方法：结束特殊攻击
		public virtual void EndSpecialAttack()
		{
			// 可在子类中实现具体的结束攻击逻辑
		}

		public virtual void TakeDamage(float damage)
		{
			CurrentHP -=damage* DamageMultiplier;
			GD.Print($"{enemyName} 受到 {damage} 点伤害，当前生命值：{CurrentHP}/{MaxHP}");
			
			// 调用受伤处理虚方法
			OnTakeDamage(damage);
		}

		// 虚方法：受到伤害时调用
		public virtual void OnTakeDamage(float damage)
		{
			// 可在子类中实现受击效果，如闪烁、震动等
		}

		public virtual void Die()
		{
			// 调用死亡前处理虚方法
			BeforeDie();
			
			QueueFree();
			GD.Print($"{enemyName} 死亡");
		}

		// 虚方法：死亡前调用
		public virtual void BeforeDie()
		{
			// 可在子类中实现死亡前的清理工作
		}

		public virtual void ApplyBuff(BuffBase buff)
		{
			
		}
	}

	public class EnemyBulletTool
    {
		private PackedScene bulletPrefab;
		private BulletPattern pattern;

		public EnemyBulletTool(PackedScene _bulletPrefab, BulletPattern _pattern)
		{
			bulletPrefab = _bulletPrefab;
			pattern = _pattern;
		}

        public static Vector2 UpdateBulletPosition(Vector2 _lastPosition)
        {
            return Vector2.Zero;
        }
    }

	public enum BulletPattern
	{
		Straight,//直线
		Ring,//环形
		Spiral,//螺旋
		Bezier,//贝塞尔曲线
		Track,//追踪
		Wave//波浪
	}
}