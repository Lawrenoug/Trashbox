using Godot;
using System;

namespace Enemy
{

	public partial class PerformanceBottleneckBullet_2 : EnemyBullet
	{
		public override float speed { get; set; } = 100f;
		public override float angle { get; set; } = 0f;

		public override float ATK { get; set; } = 5f;

		private float len=100f;
		private float lenhaveDone=0f;
		private float time=0f;
		private float timeWaited=0f;
		private bool wait=false;

		public override void init(Vector2 _position,float _angle,float _ATK)
		{
			GlobalPosition = _position;
			angle = _angle;
			ATK = _ATK;
			// 设置初始角度
			//RotationDegrees = angle;
		}

		public void init(Vector2 _position,float _angle,float _ATK,float index)
        {
            init(_position,_angle,_ATK);
			time = index*5f;
			len=index*100f;
        }
        
		public override void _Ready()
        {
            base._Ready();
        }

		public override void _Process(double delta)
		{
			base._Process(delta);
			Move(delta);
		}

		public override void Move(double delta)
		{
			if (!wait)
			{
				if(lenhaveDone<len)
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
				}
			}
			else
			{
				// 停顿时间增加
				timeWaited += (float)delta;
				if (timeWaited >= time)
				{
					// 等待结束，继续移动
					float angleInRadians = angle * Mathf.Pi / 180f;
					Vector2 direction = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
					GlobalPosition += direction * speed * (float)delta;
				}
			}
		}
	}
}