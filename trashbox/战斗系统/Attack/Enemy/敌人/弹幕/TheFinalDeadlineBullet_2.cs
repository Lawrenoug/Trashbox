using Godot;
using System;

namespace Enemy
{

	public partial class TheFinalDeadlineBullet_2 : EnemyBullet
	{
		[Export] public PackedScene chileBullet;
		public override float speed { get; set; } = 100f;
		public override float angle { get; set; } = 0f;

		public override float ATK { get; set; } = 5f;

		private float timeSinceLastAttack=0f;
		

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
			timeSinceLastAttack += (float)delta;
			//Visible = true;
			base._Process(delta);
			Move(delta);
			if (timeSinceLastAttack >= 0.5f)
			{
				GD.Print("生成子弹");
				// 创建子弹
				timeSinceLastAttack = 0f;
				var parentNode = GetParent() as Node2D;
				var childBullet_1 = chileBullet.Instantiate<TheFinalDeadlineBullet_2_child>();
				parentNode.AddChild(childBullet_1);
				childBullet_1.init(GlobalPosition, angle, ATK*0.5f, 1);

				var childBullet_2 = chileBullet.Instantiate<TheFinalDeadlineBullet_2_child>();
				parentNode.AddChild(childBullet_2);
				childBullet_2.init(GlobalPosition, angle, ATK*0.5f, -1);
			}
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