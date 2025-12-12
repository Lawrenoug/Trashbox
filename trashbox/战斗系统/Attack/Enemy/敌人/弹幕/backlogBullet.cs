using Godot;
using System;

namespace Enemy
{
	public partial class backlogBullet : EnemyBullet
	{
		public override float speed { get; set; } = 300f;
		public override float angle { get; set; } = 0f;

		public override float ATK { get; set; } = 5f;
		
		private Vector2 targetPosition;
		// 标记是否已设置目标方向
		private bool hasTargetDirection = false;
		// 存储计算出的移动方向
		private Vector2 moveDirection;
		public override void init(Vector2 _position,float _angle,float _ATK)
		{
			GlobalPosition = _position;
			angle = _angle;
			ATK = _ATK;
			// 设置初始角度
			//RotationDegrees = angle;
		}
		public void init(Vector2 _position,float _angle,float _ATK,Vector2 _target)
		{
			GlobalPosition = _position;
			angle = _angle;
			ATK = _ATK;
			targetPosition = _target;
			// 计算并存储移动方向
			moveDirection = (targetPosition - GlobalPosition).Normalized();
			hasTargetDirection = true;
		}
        
		public override void _Ready()
        {
            base._Ready();
            //Visible = false; // 确保子弹可见
        }



		public override void _Process(double delta)
		{
			//Visible = true;
			base._Process(delta);
			Move(delta);
		}

		public override void Move(double delta)
		{
			// 如果已设置目标方向，则始终朝着该方向移动
			if (hasTargetDirection)
			{
				GlobalPosition += moveDirection * speed * (float)delta;
			}
			else
			{
				// 如果没有设置目标方向，使用基类的移动方式
				base.Move(delta);
			}
		}
	}
}