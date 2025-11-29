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
			
			// 连接碰撞信号
			Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
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

		//超出屏幕自动销毁
		private void _on_VisibilityNotifier2D_screen_exited()
		{
			Vector2 screenSize = GetViewportRect().Size;
			Vector2 globalPos = GlobalPosition;
			float extraBounds = 50; // 额外边界（像素）

			// 正确的判断逻辑：只要满足任意一个边界超出，就销毁
			bool isOffScreen =
				globalPos.X < -extraBounds ||          // 左超出
				globalPos.X > screenSize.X + extraBounds ||  // 右超出
				globalPos.Y < -extraBounds ||          // 上超出
				globalPos.Y > screenSize.Y + extraBounds;   // 下超出

			if (isOffScreen)
			{
				GD.Print("子弹超出屏幕（含额外边界），自动销毁");
				QueueFree();
			}
		}

		public virtual void OnBodyEntered(Node body)
		{
			if (body.IsInGroup("enemy"))
			{
				GD.Print("子弹碰撞到敌人，自动销毁");
				EnemyBase enemy = body as EnemyBase;
				enemy.TakeDamage(ATK);
				QueueFree();
			}
		}

		// 直线移动
        private void HandleStraightMovement(double delta)
        {
            float moveDistance = ATS * (float)delta * NormalData.ATS;
            Godot.Vector2 movement = Transform.X * moveDistance;
            GlobalPosition += movement;
        }

		// 寻找最近的目标
        private void FindNearestTarget()
        {
            // 假设敌人都在 "enemy" 组中
            var enemies = GetTree().GetNodesInGroup("enemy");
            Node2D nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (Node node in enemies)
            {
                if (node is Node2D enemy)
                {
                    float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                    
                    // 只考虑在跟踪范围内的敌人
                    if (distance < trackingRange && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }

            target = nearestEnemy;
            
            if (target != null)
            {
                //GD.Print("找到目标，距离: " + nearestDistance);
            }
            else
            {
                //GD.Print("未找到目标");
            }
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

            // 计算朝向目标的移动
            Godot.Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
            float moveDistance = ATS * (float)delta * NormalData.ATS;
            
            // 更新位置
            GlobalPosition += direction * moveDistance;
            
            // 更新旋转朝向目标
            LookAt(target.GlobalPosition);
            
            // 检查是否超出跟踪范围
            CheckTargetDistance();
        }

        // 检查目标距离，如果超出跟踪范围则放弃目标
        private void CheckTargetDistance()
        {
            if (target != null && IsInstanceValid(target))
            {
                float distance = GlobalPosition.DistanceTo(target.GlobalPosition);
                if (distance > trackingRange)
                {
                    GD.Print("目标超出跟踪范围，放弃目标");
                    target = null;
                }
            }
        }

        public virtual void SetBuff(Node body)
        {
            EnemyBase enemy = body as EnemyBase;
            enemy.ApplyBuff(null);
        }
	}
}