using Godot;
using System;

namespace Enemy
{
	public partial class TheFinalDeadlineBullet_2_child : EnemyBullet
	{
		public override float speed { get; set; } = 50f;
		public override float angle { get; set; } = 0f;

		public override float ATK { get; set; } = 5f;

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
			index = _size;
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
			if(index==1)
            {
                // 向上移动
				GlobalPosition += new Vector2(0, -speed * (float)delta);
            }
			else if(index==-1)
            {
                // 向下移动
				GlobalPosition += new Vector2(0, speed * (float)delta);
            }

			//Visible = true;
			// 使用向量移动，避免复杂的三角函数计算
			
		}
	}
}