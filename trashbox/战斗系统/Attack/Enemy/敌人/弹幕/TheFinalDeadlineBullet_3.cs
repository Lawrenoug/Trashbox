using Godot;
using System;

namespace Enemy
{

	public partial class TheFinalDeadlineBullet_3 : EnemyBullet
	{
		public override float speed { get; set; } = 100f;
		public override float angle { get; set; } = 0f;
		public override float ATK { get; set; } = 5f;
		
		// 波浪参数
		private float waveAmplitude = 50.0f; // 波浪振幅
		private float waveFrequency = 1.0f;  // 波浪频率
		private float timePassed = 0.0f;     // 累计时间

		private int index = 0;


		public override void init(Vector2 _position, float _angle, float _ATK)
		{
			GlobalPosition = _position;
			angle = _angle;
			ATK = _ATK;
			// 设置初始角度
			//RotationDegrees = angle;
		}

		public void init(Vector2 _position,float _angle,float _ATK,int _size)
		{
			GlobalPosition = _position;
			angle = _angle;
			ATK = _ATK;
			index=_size;
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
			// 累计时间
			timePassed += (float)delta;
			
			// 计算主移动方向
			float angleInRadians = angle * Mathf.Pi / 180f;
			Vector2 direction = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
			
			// 计算垂直于主方向的向量（用于波浪运动）
			Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
			
			// 主方向移动
			Vector2 mainMovement = direction * speed * (float)delta;
			
			// 波浪偏移，根据 index 的值决定方向
			Vector2 waveOffset = perpendicular * Mathf.Sin(timePassed * waveFrequency) * waveAmplitude * (float)delta * index;
			
			// 合成最终移动
			GlobalPosition += mainMovement + waveOffset;
		}
	}
}