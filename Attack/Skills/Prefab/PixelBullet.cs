using Godot;
using System;
using System.Numerics;

namespace Attack
{
    public partial class PixelBullet : RigidBody2D
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

        public void Initialize(SkillData _skillData)
        {
            //Scale = new Godot.Vector2(_skillData.RNG,_skillData.RNG);
            //ApplyScale(new Godot.Vector2(_skillData.RNG, _skillData.RNG));
            //GetChild()
            var sprite2D = GetNode<Sprite2D>("Sprite2D");
            sprite2D.Scale = new Godot.Vector2(_skillData.RNG, _skillData.RNG);
            var collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape2D.Scale = new Godot.Vector2(_skillData.RNG, _skillData.RNG);
            //GD.Print(_skillData.RNG);
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
			//GD.Print("子弹碰到障碍物");
            // 检查碰撞到的物体是否是敌人
            if (body.IsInGroup("enemy"))
            {
                //GD.Print("子弹击中敌人，造成伤害: " + ATK);
                
                // 销毁子弹
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
            // 如果没有目标或目标已无效，尝试寻找新目标
            if (target == null || !IsInstanceValid(target))
            {
                FindNearestTarget();
                
                // 如果仍然没有目标，改为直线移动
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