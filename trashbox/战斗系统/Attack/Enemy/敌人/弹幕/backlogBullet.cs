using Godot;
using System;

namespace Enemy
{
	public partial class backlogBullet : EnemyBullet
	{
		public override float speed { get; set; } = 100f;
		public override float angle { get; set; } = 0f;

		public override float ATK { get; set; } = 5f;
		

		public override void init(Vector2 _position,float _angle,float _ATK)
		{
			GlobalPosition = _position;
			angle = _angle;
			ATK = _ATK;
			// 设置初始角度
			//RotationDegrees = angle;
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
			//Visible = true;
			// 使用向量移动，避免复杂的三角函数计算
			float angleInRadians = angle * Mathf.Pi / 180f;
			Vector2 direction = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
			GlobalPosition += direction * speed * (float)delta;
		}
	}
}