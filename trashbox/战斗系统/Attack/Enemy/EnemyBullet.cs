using Godot;
using System;
using CharacterManager;

namespace Enemy
{
	public partial class EnemyBullet : RigidBody2D
	{
		public virtual float speed{get; set;} = 100f;
		public virtual float angle {get; set;} = 0f;
		public virtual float ATK {get; set;} = 5f;
		
		// 屏幕边界
		private Vector2 screen_size;

		public virtual void init(Vector2 _position,float _angle,float _ATK)
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
			// 获取屏幕尺寸
			screen_size = GetViewportRect().Size;
            //Visible = false; // 确保子弹可见
        }



		public override void _Process(double delta)
		{
			//Visible = true;
			base._Process(delta);
			Move(delta);
			
			// 检查是否超出屏幕边界
			CheckBounds();
		}

		public virtual void Move(double delta)
		{
			//Visible = true;
			// 使用向量移动，避免复杂的三角函数计算
			float angleInRadians = angle * Mathf.Pi / 180f;
			Vector2 direction = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
			GlobalPosition += direction * speed * (float)delta;
		}
		
		// 检查子弹是否超出屏幕边界
		private void CheckBounds()
		{
			// 定义一个边界缓冲区，避免子弹刚出屏幕就销毁
			const float BUFFER = 50.0f;
			
			if (GlobalPosition.X < -BUFFER || GlobalPosition.X > screen_size.X + BUFFER ||
				GlobalPosition.Y < -BUFFER || GlobalPosition.Y > screen_size.Y + BUFFER)
			{
				// 超出边界，销毁子弹
				GD.Print("敌方子弹超出屏幕（含边界缓冲区），自动销毁");
				QueueFree();
			}
		}

		public virtual void OnBodyEntered(Node body)
		{
			if (body.IsInGroup("player"))
			{
				GD.Print("弹幕碰撞到玩家，自动销毁");
				PlayerManager player = body as PlayerManager;
				player.TakeDamage(ATK);
				QueueFree();
			}
		}
	}
}