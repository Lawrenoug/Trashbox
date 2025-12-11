using Godot;
using System;

namespace Enemy
{
	public partial class TheFinalDeadlineBullet_4 : EnemyBullet
	{
		public override float speed { get; set; } = 100f;
		public override float angle { get; set; } = 0f;

		public override float ATK { get; set; } = 5f;

		private float size = 1f;
		

		public override void init(Vector2 _position,float _angle,float _ATK)
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
			ATK = _ATK*_size;
			var sprite = GetNode<Sprite2D>("Sprite2D");
			sprite.Scale=new Vector2(_size,_size);
			var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
			collisionShape.Scale=new Vector2(_size,_size);
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
			size+=(float)delta;
			var sprite = GetNode<Sprite2D>("Sprite2D");
			sprite.Scale=new Vector2(size,size);
			var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
			collisionShape.Scale=new Vector2(size,size);
			ATK+=(float)delta*10f;
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