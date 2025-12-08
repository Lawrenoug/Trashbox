using Godot;
using System;

namespace Enemy
{
	public partial class EnemyBullet : RigidBody2D
	{
		public BulletPattern pattern;

		public void init(Vector2 _position, BulletPattern _pattern)
		{
			Position = _position;
			pattern = _pattern;
		}
        
		public override void _Ready()
        {
            base._Ready();

        }

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
            base._Process(delta);
		}
	}
}