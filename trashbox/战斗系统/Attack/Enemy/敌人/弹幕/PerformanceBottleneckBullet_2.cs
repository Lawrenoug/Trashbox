using Godot;
using System;

namespace Enemy
{

	public partial class PerformanceBottleneckBullet_2 : EnemyBullet
	{
		public override float speed { get; set; } = 200f;
		public override float angle { get; set; } = 0f;

		public override float ATK { get; set; } = 5f;

		private float len = 100f;
		private float lenhaveDone = 0f;
		private float time = 0f;
		private float timeWaited = 0f;
		private bool wait = false;
		private Vector2 targetPosition = new Vector2();
		private bool startTracking = false;
		private Vector2 trackingDirection = new Vector2(); // 添加追踪方向向量
		private Vector2 center = new Vector2();
		private float rotationAngle = 0f; // 旋转角度
		private float rotationRadius = 0f; // 旋转半径

		public override void init(Vector2 _position, float _angle, float _ATK)
		{
			GlobalPosition = _position;
			angle = _angle;
			ATK = _ATK;
			// 设置初始角度
			//RotationDegrees = angle;
		}

		public void init(Vector2 _position, float _angle, float _ATK, float index)
		{
			init(_position, _angle, _ATK);
			time = index * 5f;
			len = index * 100f;
		}

		public override void _Ready()
		{
			base._Ready();
			// 初始化时获取目标位置
			var node = GetTree().GetFirstNodeInGroup("性能瓶颈发射点") as Node2D;
			center = node.GlobalPosition;

		}

		public override void _Process(double delta)
		{

			base._Process(delta);
			Move(delta);
			var node = GetTree().GetFirstNodeInGroup("性能瓶颈发射点") as Node2D;
			center = node.GlobalPosition;
		}

		public override void Move(double delta)
		{
			if (!wait)
			{
				if (lenhaveDone < len)
				{
					//Visible = true;
					// 使用向量移动，避免复杂的三角函数计算
					float angleInRadians = angle * Mathf.Pi / 180f;
					Vector2 direction = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
					GlobalPosition += direction * speed * (float)delta;

					lenhaveDone += speed * (float)delta;
				}
				else
				{
					// 开始等待
					wait = true;
					// 计算旋转半径
					rotationRadius = GlobalPosition.DistanceTo(center);
					// 初始化旋转角度
					Vector2 offset = GlobalPosition - center;
					rotationAngle = Mathf.Atan2(offset.Y, offset.X);
				}
			}
			else
			{
				// 停顿时间增加
				timeWaited += (float)delta;
				if (timeWaited >= time)
				{
					if (!startTracking)
					{
						// 获取玩家位置并计算追踪方向
						var node = GetTree().GetFirstNodeInGroup("player") as RigidBody2D;
						if (node != null)
						{
							targetPosition = node.GlobalPosition;
							// 计算并保存追踪方向，之后一直沿这个方向移动
							trackingDirection = (targetPosition - GlobalPosition).Normalized();
						}
						startTracking = true;
					}
					// 等待结束，继续移动，朝向之前确定的方向移动
					GlobalPosition += trackingDirection * speed * (float)delta;
				}
				else
				{
					// 在等待期间围绕中心点旋转
					// 更新旋转角度（缓慢旋转）
					rotationAngle += 0.5f * (float)delta;
					
					// 根据角度和半径计算新位置
					float x = center.X + rotationRadius * Mathf.Cos(rotationAngle);
					float y = center.Y + rotationRadius * Mathf.Sin(rotationAngle);
					GlobalPosition = new Vector2(x, y);
				}
			}
		}
	}
}