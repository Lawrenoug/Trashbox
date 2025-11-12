using Godot;
using System;
using Attack;

namespace CharacterManager
{
	public partial class PlayerManager : RigidBody2D
	{
		[Export]
		public Node2D startShootPosition;
		private float blood = 100;

		private float speed = 150;
		private AttackManager attackManager;

		public override void _Ready()
		{
			GD.Print(startShootPosition.GlobalPosition);
			//var node = GetTree().Root.FindChild("玩家子弹", true);
			var node = GetNode<Node>("/root/Node2D/玩家子弹");
			if (node != null)
			{
				attackManager = new AttackManager(node);
			}
			else
            {
				GD.Print("没找到");
            }
        }

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);
			Move(delta);
			if(attackManager!=null)
			{
				attackManager.AttackLoop((float)delta,startShootPosition.GlobalPosition);
                
            }
		}

		private void Move(double delta)
		{
			Vector2 inputDirection = Input.GetVector("左", "右", "上", "下");
			var Velocity = inputDirection * speed;
			Position += Velocity * (float)delta;
		}
		
		
	}
}