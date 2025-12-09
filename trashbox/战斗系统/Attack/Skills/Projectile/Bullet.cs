using Godot;
using System;
using Enemy;

namespace Attack
{
	public abstract partial class Bullet : RigidBody2D
	{
		public virtual float ATK { get; set; } = 0;
		public virtual float ATS { get; set; } = 1;
		public virtual bool enableTarcking { get; set; } = false;

		public virtual SkillData skillData { get; set; }=new SkillData();

		private Node2D target; // 跟踪目标
		private float trackingRange = 1000f; // 跟踪范围

		public override void _Ready()
		{
			base._Ready();
			
			// 【修复】强制开启接触监视器，否则无法检测碰撞
			ContactMonitor = true;
			MaxContactsReported = 5;

			// 连接碰撞信号
			if (!IsConnected("body_entered", new Callable(this, nameof(OnBodyEntered))))
			{
				Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
			}
		}

		public virtual void Initialize(SkillData _skillData)
		{
			skillData = _skillData;
		}

		public override void _Process(double delta)
		{
			_on_VisibilityNotifier2D_screen_exited();
			if (enableTarcking)
			{
				HandleTrackingMovement(delta);
			}
			else
			{
				HandleStraightMovement(delta);
			}
		}

		// 超出屏幕自动销毁
		private void _on_VisibilityNotifier2D_screen_exited()
		{
			Vector2 screenSize = GetViewportRect().Size;
			Vector2 globalPos = GlobalPosition;
			float extraBounds = 50; 

			bool isOffScreen =
				globalPos.X < -extraBounds ||          
				globalPos.X > screenSize.X + extraBounds ||  
				globalPos.Y < -extraBounds ||          
				globalPos.Y > screenSize.Y + extraBounds;   

			if (isOffScreen)
			{
				QueueFree();
			}
		}

		public virtual void OnBodyEntered(Node body)
		{
			if (body.IsInGroup("enemy"))
			{
				GD.Print("子弹命中: " + body.Name);
				EnemyBase enemy = body as EnemyBase;
				// 判空，防止碰到了在 enemy 组但不是 EnemyBase 的东西
				if (enemy != null)
				{
					enemy.TakeDamage(ATK);
				}
				QueueFree();
			}
		}

		// 直线移动
		private void HandleStraightMovement(double delta)
		{
			float moveDistance = ATS * (float)delta * NormalData.ATS;
			// 修正方向计算，确保沿子弹朝向飞行
			Godot.Vector2 movement = Transform.X * moveDistance; 
			GlobalPosition += movement;
		}

		// 寻找最近的目标
		private void FindNearestTarget()
		{
			var enemies = GetTree().GetNodesInGroup("enemy");
			Node2D nearestEnemy = null;
			float nearestDistance = float.MaxValue;

			foreach (Node node in enemies)
			{
				if (node is Node2D enemy)
				{
					float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
					
					if (distance < trackingRange && distance < nearestDistance)
					{
						nearestDistance = distance;
						nearestEnemy = enemy;
					}
				}
			}

			target = nearestEnemy;
		}

		// 跟踪移动
		private void HandleTrackingMovement(double delta)
		{
			if (target == null || !IsInstanceValid(target))
			{
				FindNearestTarget();
				
				if (target == null)
				{
					HandleStraightMovement(delta);
					return;
				}
			}

			Godot.Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
			float moveDistance = ATS * (float)delta * NormalData.ATS;
			
			GlobalPosition += direction * moveDistance;
			LookAt(target.GlobalPosition);
			CheckTargetDistance();
		}

		private void CheckTargetDistance()
		{
			if (target != null && IsInstanceValid(target))
			{
				float distance = GlobalPosition.DistanceTo(target.GlobalPosition);
				if (distance > trackingRange)
				{
					target = null;
				}
			}
		}

		public virtual void SetBuff(Node body)
		{
			EnemyBase enemy = body as EnemyBase;
			if (enemy != null)
			{
				enemy.ApplyBuff(null);
			}
		}
	}
}
