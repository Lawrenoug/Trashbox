using Godot;
using System;
using Attack;

namespace CharacterManager
{
	public partial class PlayerManager : RigidBody2D
	{
		private float blood = 100;

		private float speed = 150;
		private AttackManager attackManager;

		public override void _Ready()
        {
			attackManager = new AttackManager();
        }

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);
			Move(delta);
			attackManager.AttackLoop((float)delta);
		}

		private void Move(double delta)
		{
			Vector2 inputDirection = Input.GetVector("左", "右", "上", "下");
			var Velocity = inputDirection * speed;
			Position += Velocity * (float)delta;
		}
		
		
	}
}