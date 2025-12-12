using Godot;
using System;
using Attack;
using Buff;
using Godot.Collections;

namespace Enemy
{
	public abstract partial class EnemyBase : RigidBody2D
	{
		[Export] public PackedScene bulletPrefab;
		public virtual float MaxHP { get; set; } = 100;
		public virtual float CurrentHP { get; set; } = 100;
		public virtual float ATK { get; set; } = 10;
		public virtual float ATS { get; set; } = 10;
		public virtual float MoveSpeed { get; set; } = 100;
		public virtual string enemyName { get; set; } = "Enemy";
		public virtual EnemyState state { get; set; } = EnemyState.Idle;//默认空闲状态

		public virtual BuffBase[] CurrentBuff { get; set; }

		public virtual float DamageMultiplier { get; set; } = 1;

		public float timeSinceLastAttack = 0f;
		
		// 添加用于随机移动的计时器
		public float timeSinceLastMove = 0f;

		public Vector2 nextPosition=new Vector2();

		public Vector2 moveRange=new Vector2(1920,1080);

		//private bool isMoving = false;

		

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
				state = EnemyState.Dead;
			}
			switch(state)
			{
				case EnemyState.Moving:
					if(GlobalPosition.DistanceTo(nextPosition)<0.1f)
                    {
                        state=EnemyState.Attacking;
						break;
					}
					else
                    {
                        Move();
                    }
					break;
				case EnemyState.Attacking:
					Attack(delta);
					// 每隔3秒执行一次随机移动
					timeSinceLastMove += (float)delta;
					if (timeSinceLastMove >= 2.0f)
					{
						RandomMove();
						timeSinceLastMove = 0f;
					}
					// 在攻击状态下也执行移动逻辑
					Move();
					break;
				case EnemyState.Dead:
					Die();
					break;
				default:
					break;
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

		public virtual void MoveTo(Vector2 _position)
		{
			//state = EnemyState.Moving;
			nextPosition = _position;
		}

		public virtual void Move()
		{
			// 移动 towards() 方法
			Vector2 direction = nextPosition - GlobalPosition;
			float distance = direction.Length();
			if (distance > 0)
			{
				direction = direction.Normalized();
				GlobalPosition += direction * MoveSpeed * (float)GetProcessDeltaTime();
			}
		}
		public virtual void Attack(double delta)
		{
			timeSinceLastAttack += (float)delta;
			if (ATS <= 0)
			{
				ATS = 0.1f; // 防止除零错误
			}
			
			// 检查是否到了攻击时间
			float attackInterval = 1.0f / ATS;
			if (timeSinceLastAttack >= 1)
			{
				// 重置计时器
				timeSinceLastAttack = 0f;
				
				// 添加空值检查以防止NullReferenceException
				if (bulletPrefab == null)
				{
					GD.Print("警告: bulletPrefab未分配");
					return;
				}
				
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
				prefab.init(GetNode<Node2D>("发射点").GlobalPosition,0f,ATK);
			}
		}

        public virtual void RandomMove()
		{
			var random_X = new Random().Next(980, 1900);
			var random_Y = new Random().Next(20,1060);
			MoveTo(new Vector2(random_X, random_Y));
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

	

	public enum EnemyState
	{
		Idle,
		Moving,
		Attacking,
		Dead
	}
	
}