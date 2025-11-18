using Godot;
using System;

namespace Attack
{
	public partial class SymbolBullet : RigidBody2D
	{
		public float ATK = 0;
        public float ATS = 1;
        public bool enableTarcking = false;

        private Node2D target; // 跟踪目标
        private float trackingRange = 1000f; // 跟踪范围

        public override void _Ready()
        {
            base._Ready();
            
            // 连接碰撞信号
            Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
        }

        public void Initialize(SkillData _skillData,string _char)
        {
            var label = GetNode<Label>("Label");
            label.Scale = new Godot.Vector2(_skillData.RNG*0.1f, _skillData.RNG*0.1f);
            var collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape2D.Scale = new Godot.Vector2(_skillData.RNG, _skillData.RNG);
			label.Text=_char;
            ATK += _skillData.ATK;
            ATS += _skillData.ATS;
            enableTarcking = _skillData.enableTarcking;
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

        // 碰撞检测处理
        public void OnBodyEntered(Node body)
		{
            if (body.IsInGroup("enemy"))
            {
                QueueFree();
            }
        }

        public override void _Process(double delta)
        {
            if (enableTarcking)
            {
                HandleTrackingMovement(delta);
            }
            else
            {
                HandleStraightMovement(delta);
            }
        }

        // 直线移动
        private void HandleStraightMovement(double delta)
        {
            float moveDistance = ATS * (float)delta * NormalData.ATS;
            Godot.Vector2 movement = Transform.X * moveDistance;
            GlobalPosition += movement;
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
	}
}